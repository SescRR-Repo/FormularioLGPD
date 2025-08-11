// Services/PdfService.cs
using FormularioLGPD.Server.Services.Interfaces;
using FormularioLGPD.Server.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Text;

namespace FormularioLGPD.Server.Services
{
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PdfService> _logger;

        public PdfService(IConverter converter, IConfiguration configuration, ILogger<PdfService> logger)
        {
            _converter = converter;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<byte[]> GerarPdfTermoAsync(TermoAceite termo)
        {
            try
            {
                var htmlContent = GerarHtmlTermo(termo);

                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings() { Top = 20, Bottom = 20, Left = 15, Right = 15 }
                    },
                    Objects = {
                        new ObjectSettings() {
                            PagesCount = true,
                            HtmlContent = htmlContent,
                            WebSettings = { DefaultEncoding = "utf-8" },
                            HeaderSettings = { FontSize = 9, Right = "Página [page] de [toPage]", Line = true },
                            FooterSettings = { FontSize = 8, Line = true, Center = "Documento gerado eletronicamente em " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") }
                        }
                    }
                };

                return _converter.Convert(doc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar PDF para o termo {NumeroTermo}", termo.NumeroTermo);
                throw;
            }
        }

        public async Task<string> SalvarPdfAsync(byte[] pdfBytes, string numeroTermo)
        {
            try
            {
                var pastaStorage = _configuration["PdfSettings:StoragePath"] ?? "PDFs";

                if (!Directory.Exists(pastaStorage))
                {
                    Directory.CreateDirectory(pastaStorage);
                }

                var nomeArquivo = $"{numeroTermo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var caminhoCompleto = Path.Combine(pastaStorage, nomeArquivo);

                await File.WriteAllBytesAsync(caminhoCompleto, pdfBytes);

                _logger.LogInformation("PDF salvo com sucesso: {CaminhoArquivo}", caminhoCompleto);

                return caminhoCompleto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar PDF para o termo {NumeroTermo}", numeroTermo);
                throw;
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

            var html = new StringBuilder();

            html.Append(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Termo de Consentimento LGPD</title>
    <style>
        body { font-family: Arial, sans-serif; font-size: 12px; line-height: 1.4; margin: 0; }
        .header { text-align: center; margin-bottom: 30px; }
        .title { font-size: 16px; font-weight: bold; margin-bottom: 10px; }
        .subtitle { font-size: 14px; margin-bottom: 20px; }
        .content { margin-bottom: 20px; }
        .field { margin-bottom: 8px; }
        .bold { font-weight: bold; }
        .signature-area { margin-top: 40px; text-align: center; }
        .signature-line { border-bottom: 1px solid #000; width: 400px; margin: 0 auto; }
        .dependentes { margin: 20px 0; }
        .dependente-item { margin-bottom: 5px; }
        .metadata { margin-top: 30px; font-size: 10px; color: #666; }
    </style>
</head>
<body>");

            html.Append(@"
    <div class='header'>
        <div class='title'>CREDENCIAL SESC</div>
        <div style='margin: 10px 0;'>[ X ] Cadastro [ ] Renovação [ ] Inclusão de dependente</div>
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
        <div class='field'>Escolaridade: {titular.Escolaridade ?? "Não informado"}</div>
        <div class='field'>Série/Semestre: {titular.SerieSemestre ?? "Não informado"}</div>
        <div class='field'>Tel.: {titular.Telefone ?? "Não informado"}</div>
        <div class='field'>E-mail: {titular.Email}</div>
    </div>");

            if (dependentes.Any())
            {
                html.Append("<div class='dependentes'>");
                html.Append("<div class='bold'>Dependentes menores de 18 anos ou legalmente representados:</div><br/>");

                foreach (var dep in dependentes)
                {
                    html.Append($@"
                    <div class='dependente-item'>
                        Nome: {dep.Nome} &nbsp;&nbsp;&nbsp; 
                        CPF: {FormatarCpf(dep.CPF)} &nbsp;&nbsp;&nbsp; 
                        Grau de parentesco: {dep.GrauParentesco}
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
        <p>Boa Vista-RR, {DateTime.Now:dd} de {DateTime.Now:MMMM} de {DateTime.Now:yyyy}.</p>
    </div>

    <div class='signature-area'>
        <div style='margin-bottom: 40px;'></div>
        <div class='signature-line'></div>
        <div style='margin-top: 5px;'>Assinatura do(a) declarante dos dados ou responsável legal</div>
    </div>");

            html.Append($@"
    <div class='metadata'>
        <div class='bold'>DADOS DO ACEITE ELETRÔNICO:</div>
        <div>Número do Termo: {termo.NumeroTermo}</div>
        <div>Data/Hora do Aceite: {termo.DataHoraAceite:dd/MM/yyyy HH:mm:ss} UTC</div>
        <div>IP de Origem: {termo.IpOrigem}</div>
        <div>Hash de Integridade: {termo.HashIntegridade}</div>
        <div>Versão do Termo: {termo.VersaoTermo}</div>
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
    }
}