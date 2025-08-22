// Program.cs (Atualizado para Produção)
using Microsoft.EntityFrameworkCore;
using FormularioLGPD.Server.Data;
using FormularioLGPD.Server.Services.Interfaces;
using FormularioLGPD.Server.Services;
using FormularioLGPD.Server.Repositories.Interfaces;
using FormularioLGPD.Server.Repositories;
using Serilog;

namespace FormularioLGPD.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configurar Serilog para SEMPRE funcionar (desenvolvimento E produção)
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/lgpd-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllers();

            // Entity Framework
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repositórios
            builder.Services.AddScoped<ITermoAceiteRepository, TermoAceiteRepository>();

            // Serviços
            builder.Services.AddScoped<ITermoAceiteService, TermoAceiteService>();
            builder.Services.AddScoped<IPdfService, AlternativePdfService>();
            builder.Services.AddScoped<ILogService, LogService>();

            // CORS MELHORADO para Produção
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.AllowAnyOrigin() // Permite qualquer origem em produção
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Swagger/OpenAPI - SEMPRE ativo para debug
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new()
                {
                    Title = "Formulário LGPD API",
                    Version = "v1",
                    Description = "API para processamento de termos de aceite LGPD - Sesc Roraima"
                });
            });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline - SWAGGER SEMPRE ATIVO
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Formulário LGPD API v1");
                c.RoutePrefix = "swagger"; // Acessível em /swagger
            });

            // Middleware de log para debug
            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("🌐 Request: {Method} {Path} from {RemoteIp}", 
                    context.Request.Method, 
                    context.Request.Path, 
                    context.Connection.RemoteIpAddress);
                
                await next();
                
                logger.LogInformation("📤 Response: {StatusCode}", context.Response.StatusCode);
            });

            app.UseHttpsRedirection();
            app.UseCors("AllowReactApp");
            app.UseAuthorization();
            app.MapControllers();
            app.MapFallbackToFile("/index.html");

            // Executar migrations SEMPRE (desenvolvimento E produção)
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("🚀 Iniciando aplicação...");
                logger.LogInformation("🔗 Connection String: {ConnectionString}", 
                    builder.Configuration.GetConnectionString("DefaultConnection")?.Substring(0, 50) + "...");

                // Verificar se o banco existe e criar se necessário
                var canConnect = context.Database.CanConnect();
                if (!canConnect)
                {
                    logger.LogWarning("⚠️ Banco não encontrado. Criando...");
                    context.Database.EnsureCreated();
                    logger.LogInformation("✅ Banco de dados criado com sucesso!");
                }
                else
                {
                    logger.LogInformation("✅ Banco de dados conectado com sucesso");

                    // Aplicar migrations pendentes se houver
                    var pendingMigrations = context.Database.GetPendingMigrations();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("📦 Aplicando {Count} migrations pendentes...", pendingMigrations.Count());
                        context.Database.Migrate();
                        logger.LogInformation("✅ Migrations aplicadas com sucesso!");
                    }
                    else
                    {
                        logger.LogInformation("ℹ️ Nenhuma migration pendente");
                    }
                }

                logger.LogInformation("🌟 Aplicação iniciada com sucesso!");
                logger.LogInformation("📍 URLs:");
                logger.LogInformation("   - Swagger: /swagger");
                logger.LogInformation("   - API: /api/TermoAceite");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "💥 Erro crítico ao configurar banco de dados: {Message}", ex.Message);
                
                // Tentar connection string alternativa se a primeira falhar
                try
                {
                    logger.LogWarning("🔄 Tentando criar banco com EnsureCreated...");
                    context.Database.EnsureCreated();
                    logger.LogInformation("✅ Banco criado com sucesso usando EnsureCreated");
                }
                catch (Exception ex2)
                {
                    logger.LogError(ex2, "💥 Falha total ao criar banco de dados. Verifique a connection string: {Message}", ex2.Message);
                    throw; // Re-throw para parar a aplicação se não conseguir conectar no banco
                }
            }

            app.Run();
        }
    }
}