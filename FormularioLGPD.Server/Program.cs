// Program.cs (Atualizado)
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

            // Configurar Serilog
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
            builder.Services.AddScoped<IPdfService, AlternativePdfService>(); // Usando implementação sem DLL nativa
            builder.Services.AddScoped<ILogService, LogService>();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("https://localhost:56200", "http://localhost:56200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Swagger/OpenAPI
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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowReactApp");

            app.UseAuthorization();

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            // Executar migrations automaticamente em desenvolvimento
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    // Verificar se o banco existe e criar se necessário
                    var canConnect = context.Database.CanConnect();
                    if (!canConnect)
                    {
                        logger.LogInformation("Criando banco de dados...");
                        context.Database.EnsureCreated();
                        logger.LogInformation("Banco de dados criado com sucesso!");
                    }
                    else
                    {
                        logger.LogInformation("Banco de dados já existe e está acessível");
                        
                        // Aplicar migrations pendentes se houver
                        var pendingMigrations = context.Database.GetPendingMigrations();
                        if (pendingMigrations.Any())
                        {
                            logger.LogInformation("Aplicando migrations pendentes...");
                            context.Database.Migrate();
                            logger.LogInformation("Migrations aplicadas com sucesso!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao configurar banco de dados: {Message}", ex.Message);
                    
                    // Tentar connection string alternativa se a primeira falhar
                    logger.LogWarning("Tentando criar banco com EnsureCreated como fallback...");
                    try
                    {
                        context.Database.EnsureCreated();
                        logger.LogInformation("Banco criado com sucesso usando EnsureCreated");
                    }
                    catch (Exception ex2)
                    {
                        logger.LogError(ex2, "Falha total ao criar banco de dados. Verifique a connection string.");
                    }
                }
            }

            app.Run();
        }
    }
}