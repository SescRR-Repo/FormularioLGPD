// Services/AlternativePdfService.cs
using FormularioLGPD.Server.Services.Interfaces;
using FormularioLGPD.Server.Models;
using System.Text;

namespace FormularioLGPD.Server.Services
{
    public class AlternativePdfService : IPdfService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AlternativePdfService> _logger;

        public AlternativePdfService(IConfiguration configuration, ILogger<AlternativePdfService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<byte[]> GerarPdfTermoAsync(TermoAceite termo)
        {
            try
            {
                // Por enquanto, vamos gerar um arquivo HTML em vez de PDF
                // Isso resolve o problema de dependências nativas
                var htmlContent = GerarHtmlTermo(termo);
                var htmlBytes = Encoding.UTF8.GetBytes(htmlContent);

                _logger.LogInformation("HTML gerado com sucesso para o termo {NumeroTermo}", termo.NumeroTermo);
                
                return htmlBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar HTML para o termo {NumeroTermo}", termo.NumeroTermo);
                throw;
            }
        }

        public async Task<string> SalvarPdfAsync(byte[] pdfBytes, string numeroTermo)
        {
            try
            {
                // Usar pasta Temp se a pasta configurada não funcionar
                var pastaStorage = _configuration["PdfSettings:StoragePath"] ?? "PDFs";
                var pastaAlternativa = Path.Combine(Path.GetTempPath(), "FormularioLGPD", "PDFs");
                
                string pastaFinal;
                
                try
                {
                    // Tentar criar a pasta principal primeiro
                    if (!Directory.Exists(pastaStorage))
                    {
                        Directory.CreateDirectory(pastaStorage);
                    }
                    
                    // Testar se consegue escrever na pasta
                    var testFile = Path.Combine(pastaStorage, "test.txt");
                    await File.WriteAllTextAsync(testFile, "test");
                    File.Delete(testFile);
                    
                    pastaFinal = pastaStorage;
                    _logger.LogInformation("✅ Usando pasta principal: {Pasta}", pastaStorage);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("⚠️ Erro na pasta principal ({Pasta}): {Erro}. Usando pasta temp.", pastaStorage, ex.Message);
                    
                    // Usar pasta temp como fallback
                    if (!Directory.Exists(pastaAlternativa))
                    {
                        Directory.CreateDirectory(pastaAlternativa);
                    }
                    
                    pastaFinal = pastaAlternativa;
                    _logger.LogInformation("✅ Usando pasta temp: {Pasta}", pastaAlternativa);
                }

                // Salvar como .html por enquanto
                var nomeArquivo = $"{numeroTermo}_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                var caminhoCompleto = Path.Combine(pastaFinal, nomeArquivo);

                await File.WriteAllBytesAsync(caminhoCompleto, pdfBytes);

                _logger.LogInformation("✅ Arquivo HTML salvo com sucesso: {CaminhoArquivo}", caminhoCompleto);

                return caminhoCompleto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Erro ao salvar arquivo para o termo {NumeroTermo}", numeroTermo);
                throw new InvalidOperationException($"Erro ao salvar arquivo: {ex.Message}", ex);
            }
        }

        public string GerarHashPdf(byte[] pdfBytes)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(pdfBytes);
            return Convert.ToHexString(hash);
        }

        private string GerarHtmlTermo(TermoAceite termo)
        {
            var titular = termo.Titular;
            var dependentes = titular.Dependentes;

            // Obter tipo de cadastro do formData (precisa ser passado no DTO)
            var tipoCadastro = ObterTipoCadastroFromTermo(termo);

            var html = new StringBuilder();

            html.Append(@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Termo de Consentimento LGPD</title>
    <style>
        body { 
            font-family: Arial, sans-serif; 
            font-size: 14px; 
            line-height: 1.6; 
            margin: 40px;
            color: #333;
        }
        .header { 
            text-align: center; 
            margin-bottom: 40px; 
            border-bottom: 2px solid #4285f4;
            padding-bottom: 20px;
        }
        .title { 
            font-size: 24px; 
            font-weight: bold; 
            margin-bottom: 10px; 
            color: #4285f4;
        }
        .subtitle { 
            font-size: 18px; 
            margin-bottom: 20px; 
            color: #666;
        }
        .content { 
            margin-bottom: 25px; 
            text-align: justify;
        }
        .field { 
            margin-bottom: 12px; 
            padding: 8px;
            background: #f8f9fa;
            border-left: 4px solid #4285f4;
        }
        .bold { 
            font-weight: bold; 
            color: #4285f4;
        }
        .signature-area { 
            margin-top: 60px; 
            text-align: center; 
        }
        .signature-line { 
            border-bottom: 2px solid #333; 
            width: 400px; 
            margin: 20px auto; 
            height: 50px;
        }
        .dependentes { 
            margin: 30px 0; 
            background: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
        }
        .dependente-item { 
            margin-bottom: 10px; 
            padding: 10px;
            background: white;
            border-radius: 4px;
            border-left: 4px solid #34a853;
        }
        .metadata { 
            margin-top: 40px; 
            font-size: 12px; 
            color: #666; 
            background: #f1f3f4;
            padding: 20px;
            border-radius: 8px;
        }
        .checkbox { 
            margin: 15px 0; 
            font-size: 16px;
        }
        @media print {
            body { margin: 20px; }
            .no-print { display: none; }
        }
    </style>
</head>
<body>");

            // Checkboxes dinâmicos baseados no tipo de cadastro
            var (cadastroChecked, renovacaoChecked, inclusaoChecked) = ObterCheckboxStates(tipoCadastro);

            html.Append($@"
    <div class='header'>
        <div class='title'>CREDENCIAL SESC</div>
        <div class='checkbox'>
            <label><input type='checkbox' {cadastroChecked} disabled> Cadastro</label>
            <label><input type='checkbox' {renovacaoChecked} disabled> Renovação</label>
            <label><input type='checkbox' {inclusaoChecked} disabled> Inclusão de dependente</label>
        </div>
        <div class='subtitle'>CONSENTIMENTO PARA TRATAMENTO DE DADOS PESSOAIS</div>
    </div>");

            html.Append($@"
    <div class='content'>
        <p>Por meio deste instrumento, eu, <span class='bold'>{titular.Nome}</span>, 
        inscrito(a) no CPF nº <span class='bold'>{FormatarCpf(titular.CPF)}</span>, 
        estado civil <span class='bold'>{titular.EstadoCivil}</span>, 
        natural de <span class='bold'>{titular.Naturalidade}</span>, 
        na qualidade de <span class='bold'>{ObterDescricaoQualificacao(titular.QualificacaoLegal)}</span>, 
        autorizo o Serviço Social do Comércio – Departamento Regional em Roraima, 
        inscrito no CNPJ sob nº 03.488.834/0001-86, doravante denominado CONTROLADOR, 
        a realizar o tratamento dos meus dados pessoais, inclusive dados sensíveis, bem como os dados dos meus dependentes devidamente cadastrados, 
        em razão do uso das instalações, matrículas/credenciamentos, inscrições e/ou participações nas ações e modalidades de cultura, esporte, lazer, assistência, saúde, educação, ou qualquer outra atividade promovida pela instituição.</p>
    </div>");

            html.Append($@"
    <div class='content'>
        <h3>Dados do Titular:</h3>
        <div class='field'><strong>Escolaridade:</strong> {titular.Escolaridade ?? "Não informado"}</div>
        <div class='field'><strong>Série/Semestre:</strong> {titular.SerieSemestre ?? "Não informado"}</div>
        <div class='field'><strong>Telefone:</strong> {titular.Telefone ?? "Não informado"}</div>
        <div class='field'><strong>E-mail:</strong> {titular.Email}</div>
    </div>");

            if (dependentes.Any())
            {
                html.Append("<div class='dependentes'>");
                html.Append("<h3>Dependentes menores de 18 anos ou legalmente representados:</h3>");

                foreach (var dep in dependentes)
                {
                    html.Append($@"
                    <div class='dependente-item'>
                        <strong>Nome:</strong> {dep.Nome} | 
                        <strong>CPF:</strong> {FormatarCpf(dep.CPF)} | 
                        <strong>Grau de parentesco:</strong> {dep.GrauParentesco}
                    </div>");
                }

                html.Append("</div>");
            }

            html.Append(@"
    <div class='content'>
        <p>O tratamento autorizado compreende a realização de operações como: coleta, produção, recepção, classificação, utilização, acesso, reprodução, transmissão, distribuição, processamento, arquivamento, armazenamento, eliminação, avaliação ou controle da informação, modificação, comunicação, transferência, difusão ou extração, em conformidade com os artigos 7º e 11 da LGPD.</p>
        
        <p>Declaro estar ciente de que esta autorização se dá de forma voluntária, e que poderei, a qualquer momento, revogá-la mediante solicitação formal, conforme previsto na legislação vigente.</p>
    </div>");

            html.Append($@"
    <div class='content'>
        <p><strong>Boa Vista-RR, {DateTime.Now:dd} de {DateTime.Now:MMMM} de {DateTime.Now:yyyy}.</strong></p>
    </div>

    <div class='signature-area'>
        <div class='signature-line'></div>
        <div><strong>Assinatura do(a) declarante dos dados ou responsável legal</strong></div>
    </div>");

            html.Append($@"
    <div class='metadata'>
        <h4>DADOS DO ACEITE ELETRÔNICO:</h4>
        <div><strong>Número do Termo:</strong> {termo.NumeroTermo}</div>
        <div><strong>Data/Hora do Aceite:</strong> {termo.DataHoraAceite:dd/MM/yyyy HH:mm:ss} UTC</div>
        <div><strong>IP de Origem:</strong> {termo.IpOrigem}</div>
        <div><strong>Hash de Integridade:</strong> {termo.HashIntegridade}</div>
        <div><strong>Versão do Termo:</strong> {termo.VersaoTermo}</div>
        <div><strong>User Agent:</strong> {termo.UserAgent}</div>
    </div>");

            html.Append(@"
    <div class='no-print' style='margin-top: 30px; text-align: center;'>
        <button onclick='window.print()' style='background: #4285f4; color: white; padding: 10px 20px; border: none; border-radius: 4px; cursor: pointer;'>
            Imprimir / Salvar como PDF
        </button>
    </div>");

            html.Append("</body></html>");

            return html.ToString();
        }

        private string FormatarCpf(string cpf)
        {
            if (cpf.Length == 11)
            {
                return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
            }
            return cpf;
        }

        private string ObterDescricaoQualificacao(QualificacaoLegal qualificacao)
        {
            return qualificacao switch
            {
                QualificacaoLegal.Titular => "Titular",
                QualificacaoLegal.DependenteMaior => "Dependente maior de idade",
                QualificacaoLegal.ResponsavelMenor => "Responsável por menor trabalhador",
                QualificacaoLegal.ResponsavelOrfao => "Responsável por dependente órfão do titular",
                QualificacaoLegal.CuradorTutor => "Curador, Tutor ou Guardião legal",
                _ => "Não especificado"
            };
        }

        private string ObterTipoCadastroFromTermo(TermoAceite termo)
        {
            // Agora usar o campo TipoCadastro diretamente do banco
            return termo.TipoCadastro ?? "cadastro";
        }

        private (string cadastro, string renovacao, string inclusao) ObterCheckboxStates(string tipoCadastro)
        {
            return tipoCadastro?.ToLower() switch
            {
                "cadastro" => ("checked", "", ""),
                "renovacao" => ("", "checked", ""),
                "inclusao" or "inclusao-dependente" => ("", "", "checked"),
                _ => ("checked", "", "") // Padrão: cadastro
            };
        }
    }
}