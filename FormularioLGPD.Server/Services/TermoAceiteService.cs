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
                // ✅ LOG: Registrar tentativa de criação de termo (pode ser renovação)
                var termosExistentes = await _termoRepository.ContarTermosPorCpfAsync(dto.Titular.CPF);
                if (termosExistentes > 0)
                {
                    _logger.LogInformation("📝 Criando termo adicional para CPF existente. Total de termos: {Count}", termosExistentes + 1);
                }

                // ✅ NOVO: Verificar se já existe titular com este CPF
                var titularExistente = await _termoRepository.ObterTitularPorCpfAsync(dto.Titular.CPF);
                
                Titular titular;
                if (titularExistente != null)
                {
                    // ✅ REUTILIZAR titular existente e atualizar dados
                    _logger.LogInformation("♻️ Reutilizando titular existente para CPF: {CPF}", MascararCpf(dto.Titular.CPF));
                    
                    titular = titularExistente;
                    // Atualizar dados do titular com as informações mais recentes
                    titular.Nome = dto.Titular.Nome;
                    titular.RG = dto.Titular.RG;
                    titular.DataNascimento = dto.Titular.DataNascimento;
                    titular.EstadoCivil = dto.Titular.EstadoCivil;
                    titular.Naturalidade = dto.Titular.Naturalidade;
                    titular.Endereco = dto.Titular.Endereco;
                    titular.Telefone = dto.Titular.Telefone;
                    titular.Email = dto.Titular.Email;
                    titular.Escolaridade = dto.Titular.Escolaridade;
                    titular.SerieSemestre = dto.Titular.SerieSemestre;
                    titular.QualificacaoLegal = dto.Titular.QualificacaoLegal;
                    
                    // ✅ LIMPAR dependentes existentes se for renovação/atualização
                    titular.Dependentes.Clear();
                }
                else
                {
                    // ✅ CRIAR novo titular (primeiro termo)
                    _logger.LogInformation("🆕 Criando novo titular para CPF: {CPF}", MascararCpf(dto.Titular.CPF));
                    
                    titular = new Titular
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
                }

                // Adicionar dependentes atuais
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
                    TipoCadastro = dto.TipoCadastro,
                    NumeroTermo = GerarNumeroTermo(),
                    ConteudoTermo = conteudoTermo,
                    AceiteConfirmado = dto.AceiteConfirmado,
                    DataHoraAceite = DateTime.UtcNow,
                    IpOrigem = ipOrigem,
                    UserAgent = userAgent,
                    HashIntegridade = hashIntegridade,
                    CaminhoArquivoPDF = string.Empty,
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
                    $"Termo de aceite criado com sucesso - Tipo: {dto.TipoCadastro}",
                    ipOrigem,
                    userAgent,
                    StatusOperacao.Sucesso);

                _logger.LogInformation("✅ Termo de aceite criado com sucesso. Número: {NumeroTermo}, Tipo: {Tipo}", 
                    termoSalvo.NumeroTermo, dto.TipoCadastro);

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
                _logger.LogError(ex, "💥 Erro ao criar termo de aceite para CPF: {CPF}", dto.Titular.CPF);

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

        // ✅ NOVOS MÉTODOS IMPLEMENTADOS
        public async Task<int> ContarTermosPorCpfAsync(string cpf)
        {
            return await _termoRepository.ContarTermosPorCpfAsync(cpf);
        }

        public async Task<List<TermoAceite>> ObterTermosPorCpfAsync(string cpf)
        {
            return await _termoRepository.ObterTermosPorCpfAsync(cpf);
        }

        public async Task<TermoAceite?> ObterUltimoTermoPorCpfAsync(string cpf)
        {
            var termos = await _termoRepository.ObterTermosPorCpfAsync(cpf);
            return termos.FirstOrDefault(); // Já vem ordenado por data DESC
        }

        // ✅ MÉTODO FALTANTE IMPLEMENTADO
        public async Task<Titular?> ObterTitularPorCpfAsync(string cpf)
        {
            return await _termoRepository.ObterTitularPorCpfAsync(cpf);
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

        // ✅ NOVO MÉTODO AUXILIAR
        private string MascararCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf) || cpf.Length < 3)
                return cpf;
            return new string('*', cpf.Length - 3) + cpf[^3..];
        }
    }
}