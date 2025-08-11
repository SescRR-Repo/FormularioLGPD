// Services/TermoAceiteService.cs
using FormularioLGPD.Server.Services.Interfaces;
using FormularioLGPD.Server.Repositories.Interfaces;
using FormularioLGPD.Server.DTOs;
using FormularioLGPD.Server.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FormularioLGPD.Server.Services
{
    public class TermoAceiteService : ITermoAceiteService
    {
        private readonly ITermoAceiteRepository _termoRepository;
        private readonly IPdfService _pdfService;
        private readonly ILogService _logService;
        private readonly ILogger<TermoAceiteService> _logger;

        public TermoAceiteService(
            ITermoAceiteRepository termoRepository,
            IPdfService pdfService,
            ILogService logService,
            ILogger<TermoAceiteService> logger)
        {
            _termoRepository = termoRepository;
            _pdfService = pdfService;
            _logService = logService;
            _logger = logger;
        }

        public async Task<TermoAceiteResponseDTO> CriarTermoAceiteAsync(TermoAceiteCreateDTO dto, string ipOrigem, string userAgent)
        {
            try
            {
                // Validar se CPF já existe
                if (await ValidarCpfExistenteAsync(dto.Titular.CPF))
                {
                    throw new InvalidOperationException("CPF já possui um termo ativo.");
                }

                // Criar entidade Titular
                var titular = new Titular
                {
                    Nome = dto.Titular.Nome,
                    CPF = dto.Titular.CPF,
                    RG = dto.Titular.RG,
                    DataNascimento = dto.Titular.DataNascimento,
                    EstadoCivil = dto.Titular.EstadoCivil,
                    Naturalidade = dto.Titular.Naturalidade,
                    Endereco = dto.Titular.Endereco,
                    Telefone = dto.Titular.Telefone,
                    Email = dto.Titular.Email,
                    Escolaridade = dto.Titular.Escolaridade,
                    SerieSemestre = dto.Titular.SerieSemestre,
                    QualificacaoLegal = dto.Titular.QualificacaoLegal,
                    DataCadastro = DateTime.UtcNow
                };

                // Adicionar dependentes
                foreach (var depDto in dto.Dependentes)
                {
                    var dependente = new Dependente
                    {
                        Nome = depDto.Nome,
                        CPF = depDto.CPF,
                        DataNascimento = depDto.DataNascimento,
                        GrauParentesco = depDto.GrauParentesco,
                        DataCadastro = DateTime.UtcNow
                    };

                    // Validar se é menor de idade
                    if (!dependente.IsMenorIdade)
                    {
                        throw new InvalidOperationException($"Dependente {dependente.Nome} deve ser menor de 18 anos.");
                    }

                    titular.Dependentes.Add(dependente);
                }

                // Gerar conteúdo do termo
                var conteudoTermo = await GerarConteudoTermoAsync(titular);
                var hashIntegridade = GerarHashIntegridade(conteudoTermo);

                // Criar TermoAceite
                var termo = new TermoAceite
                {
                    Titular = titular,
                    NumeroTermo = GerarNumeroTermo(),
                    ConteudoTermo = conteudoTermo,
                    AceiteConfirmado = dto.AceiteConfirmado,
                    DataHoraAceite = DateTime.UtcNow,
                    IpOrigem = ipOrigem,
                    UserAgent = userAgent,
                    HashIntegridade = hashIntegridade,
                    CaminhoArquivoPDF = string.Empty, // Será preenchido após gerar o PDF
                    DataCriacao = DateTime.UtcNow
                };

                // Salvar no banco
                var termoSalvo = await _termoRepository.CriarAsync(termo);

                // Gerar PDF
                var pdfBytes = await _pdfService.GerarPdfTermoAsync(termoSalvo);
                var caminhoArquivo = await _pdfService.SalvarPdfAsync(pdfBytes, termoSalvo.NumeroTermo);

                // Atualizar caminho do PDF
                termoSalvo.CaminhoArquivoPDF = caminhoArquivo;
                await _termoRepository.AtualizarAsync(termoSalvo);

                // Log de auditoria
                await _logService.RegistrarOperacaoAsync(
                    termoSalvo.Id,
                    "ACEITE_CRIADO",
                    "Termo de aceite criado com sucesso",
                    ipOrigem,
                    userAgent,
                    StatusOperacao.Sucesso);

                _logger.LogInformation("Termo de aceite criado com sucesso. Número: {NumeroTermo}", termoSalvo.NumeroTermo);

                return new TermoAceiteResponseDTO
                {
                    Id = termoSalvo.Id,
                    NumeroTermo = termoSalvo.NumeroTermo,
                    DataHoraAceite = termoSalvo.DataHoraAceite,
                    HashIntegridade = termoSalvo.HashIntegridade,
                    CaminhoArquivoPDF = termoSalvo.CaminhoArquivoPDF,
                    PodeDownloadPdf = File.Exists(caminhoArquivo),
                    Mensagem = "Termo de aceite processado com sucesso!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar termo de aceite para CPF: {CPF}", dto.Titular.CPF);

                // Log de erro
                await _logService.RegistrarOperacaoAsync(
                    null,
                    "ACEITE_ERRO",
                    $"Erro ao criar termo: {ex.Message}",
                    ipOrigem,
                    userAgent,
                    StatusOperacao.Erro);

                throw;
            }
        }

        public async Task<byte[]> GerarPdfTermoAsync(int termoId)
        {
            var termo = await _termoRepository.ObterPorIdAsync(termoId);
            if (termo == null)
                throw new ArgumentException("Termo não encontrado");

            return await _pdfService.GerarPdfTermoAsync(termo);
        }

        public async Task<bool> ValidarCpfExistenteAsync(string cpf)
        {
            return await _termoRepository.ExisteCpfAsync(cpf);
        }

        public async Task<TermoAceite?> ObterTermoPorIdAsync(int id)
        {
            return await _termoRepository.ObterPorIdAsync(id);
        }

        public string GerarNumeroTermo()
        {
            var ano = DateTime.Now.Year;
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var random = new Random().Next(1000, 9999);
            return $"TRM{ano}{timestamp}{random}";
        }

        private async Task<string> GerarConteudoTermoAsync(Titular titular)
        {
            // Aqui você coloca o texto completo do termo LGPD do documento Word
            // Por simplicidade, vou colocar uma versão resumida
            var conteudo = $@"
CREDENCIAL SESC

CONSENTIMENTO PARA TRATAMENTO DE DADOS PESSOAIS

Por meio deste instrumento, eu, {titular.Nome}, inscrito(a) no CPF nº {titular.CPF}, 
estado civil {titular.EstadoCivil}, natural de {titular.Naturalidade}, 
na qualidade de {ObterDescricaoQualificacao(titular.QualificacaoLegal)}, 
autorizo o Serviço Social do Comércio – Departamento Regional em Roraima, 
inscrito no CNPJ sob nº 03.488.834/0001-86, doravante denominado CONTROLADOR, 
a realizar o tratamento dos meus dados pessoais.

Escolaridade: {titular.Escolaridade}
Série/Semestre: {titular.SerieSemestre}
Tel.: {titular.Telefone}
E-mail: {titular.Email}";

            if (titular.Dependentes.Any())
            {
                conteudo += "\n\nDependentes menores de 18 anos ou legalmente representados:\n";
                foreach (var dep in titular.Dependentes)
                {
                    conteudo += $"Nome: {dep.Nome} CPF: {dep.CPF} Grau de parentesco: {dep.GrauParentesco}\n";
                }
            }

            conteudo += $@"

O tratamento autorizado compreende a realização de operações como: coleta, produção, 
recepção, classificação, utilização, acesso, reprodução, transmissão, distribuição, 
processamento, arquivamento, armazenamento, eliminação, avaliação ou controle da informação.

Boa Vista-RR, {DateTime.Now:dd} de {DateTime.Now:MMMM} de {DateTime.Now:yyyy}.

____________________________________________________
Assinatura do(a) declarante dos dados ou responsável legal

Aceite eletrônico realizado em: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC
            ";

            return conteudo;
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

        private string GerarHashIntegridade(string conteudo)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(conteudo));
            return Convert.ToHexString(bytes);
        }
    }
}