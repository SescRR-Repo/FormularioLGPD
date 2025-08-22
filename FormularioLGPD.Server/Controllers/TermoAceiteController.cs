// Controllers/TermoAceiteController.cs
using Microsoft.AspNetCore.Mvc;
using FormularioLGPD.Server.Services.Interfaces;
using FormularioLGPD.Server.DTOs;
using FormularioLGPD.Server.Models;
using FormularioLGPD.Server.Utils; // ADICIONAR ESTA LINHA

namespace FormularioLGPD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TermoAceiteController : ControllerBase
    {
        private readonly ITermoAceiteService _termoService;
        private readonly ILogger<TermoAceiteController> _logger;

        public TermoAceiteController(ITermoAceiteService termoService, ILogger<TermoAceiteController> logger)
        {
            _termoService = termoService;
            _logger = logger;
        }

        /// <summary>
        /// Cria um novo termo de aceite LGPD
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TermoAceiteResponseDTO>> CriarTermoAceite([FromBody] TermoAceiteCreateDTO dto)
        {
            try
            {
                var cpfMascarado = MascararCpf(dto?.Titular?.CPF);
                
                // VALIDAR CPF DO TITULAR
                if (!CpfValidator.IsValid(dto.Titular.CPF))
                {
                    _logger.LogWarning("❌ CPF inválido recebido: {CPF}", cpfMascarado);
                    return BadRequest(new { message = "CPF do titular é inválido" });
                }

                // VALIDAR CPF DOS DEPENDENTES
                foreach (var dep in dto.Dependentes)
                {
                    if (!string.IsNullOrEmpty(dep.CPF) && !CpfValidator.IsValid(dep.CPF))
                    {
                        var depCpfMascarado = MascararCpf(dep.CPF);
                        _logger.LogWarning("❌ CPF de dependente inválido: {CPF}", depCpfMascarado);
                        return BadRequest(new { message = $"CPF do dependente {dep.Nome} é inválido" });
                    }
                }

                _logger.LogInformation("🎯 Iniciando criação de termo. CPF: {CPF}, Tipo: {Tipo}", 
                    cpfMascarado, dto.TipoCadastro);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ ModelState inválido para CPF: {CPF}", cpfMascarado);
                    return BadRequest(ModelState);
                }

                // Capturar IP e User-Agent
                var ipOrigem = ObterIpOrigem();
                var userAgent = Request.Headers["User-Agent"].ToString();

                _logger.LogInformation("🌐 Aceite recebido de IP: {IP}", MascararIp(ipOrigem));

                var resultado = await _termoService.CriarTermoAceiteAsync(dto, ipOrigem, userAgent);

                _logger.LogInformation("✅ Termo criado com sucesso. ID: {Id}, Número: {NumeroTermo}", 
                    resultado.Id, resultado.NumeroTermo);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                var cpfMascarado = MascararCpf(dto?.Titular?.CPF);
                _logger.LogWarning(ex, "⚠️ Erro de validação para CPF: {CPF}. Erro: {Message}", 
                    cpfMascarado, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var cpfMascarado = MascararCpf(dto?.Titular?.CPF);
                _logger.LogError(ex, "💥 Erro interno para CPF: {CPF}. Tipo: {ErrorType}", 
                    cpfMascarado, ex.GetType().Name);
                
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                
                if (isDevelopment)
                {
                    return StatusCode(500, new { 
                        message = "Erro interno do servidor", 
                        error = ex.Message,
                        details = ex.InnerException?.Message 
                    });
                }
                else
                {
                    var codigoErro = Guid.NewGuid().ToString("N")[..8];
                    _logger.LogError("🔍 Código de rastreamento: {CodigoErro}", codigoErro);
                    
                    return StatusCode(500, new { 
                        message = "Erro interno do servidor",
                        codigo = codigoErro
                    });
                }
            }
        }

        /// <summary>
        /// Verifica se um CPF já possui termo ativo
        /// </summary>
        [HttpGet("validar-cpf/{cpf}")]
        public async Task<ActionResult<bool>> ValidarCpf(string cpf)
        {
            try
            {
                var cpfLimpo = new string(cpf.Where(char.IsDigit).ToArray());
                var cpfMascarado = MascararCpf(cpfLimpo);

                if (cpfLimpo.Length != 11)
                {
                    _logger.LogWarning("❌ CPF inválido recebido: {CPF}", cpfMascarado);
                    return BadRequest(new { message = "CPF deve conter 11 dígitos" });
                }

                if (!CpfValidator.IsValid(cpfLimpo))
                {
                    _logger.LogWarning("❌ CPF inválido (algoritmo): {CPF}", cpfMascarado);
                    return BadRequest(new { message = "CPF inválido" });
                }

                // ✅ NOVO: Não bloqueia mais, apenas informa
                var existe = await _termoService.ValidarCpfExistenteAsync(cpfLimpo);
                _logger.LogInformation("🔍 Validação CPF: {CPF} - Existe: {Existe}", cpfMascarado, existe);
                
                return Ok(new { 
                    existe = existe,
                    message = existe 
                        ? "CPF já possui termo(s). Você pode criar um novo termo para renovação ou inclusão de dependentes."
                        : "CPF disponível para primeiro termo"
                });
            }
            catch (Exception ex)
            {
                var cpfMascarado = MascararCpf(cpf);
                _logger.LogError(ex, "💥 Erro ao validar CPF: {CPF}", cpfMascarado);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Faz download do documento do termo (HTML pronto para impressão)
        /// </summary>
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            try
            {
                var termo = await _termoService.ObterTermoPorIdAsync(id);
                if (termo == null)
                {
                    return NotFound(new { message = "Termo não encontrado" });
                }

                var htmlBytes = await _termoService.GerarPdfTermoAsync(id);
                var nomeArquivo = $"termo_lgpd_{termo.NumeroTermo}.html";

                // Retornar como HTML que pode ser impresso como PDF
                return File(htmlBytes, "text/html; charset=utf-8", nomeArquivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar documento do termo {Id}", id);
                return StatusCode(500, new { message = "Erro ao gerar documento" });
            }
        }

        /// <summary>
        /// Obtém detalhes de um termo por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> ObterTermo(int id)
        {
            try
            {
                var termo = await _termoService.ObterTermoPorIdAsync(id);
                if (termo == null)
                {
                    return NotFound(new { message = "Termo não encontrado" });
                }

                var response = new
                {
                    id = termo.Id,
                    numeroTermo = termo.NumeroTermo,
                    dataHoraAceite = termo.DataHoraAceite,
                    hashIntegridade = termo.HashIntegridade,
                    statusTermo = termo.StatusTermo.ToString(),
                    titular = new
                    {
                        nome = termo.Titular.Nome,
                        cpf = termo.Titular.CPF,
                        email = termo.Titular.Email
                    },
                    dependentes = termo.Titular.Dependentes.Select(d => new
                    {
                        nome = d.Nome,
                        cpf = d.CPF,
                        grauParentesco = d.GrauParentesco
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter termo {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém o conteúdo do termo LGPD para exibição
        /// </summary>
        [HttpGet("conteudo-termo")]
        public ActionResult<string> ObterConteudoTermo()
        {
            try
            {
                // Aqui você pode colocar o texto completo do termo LGPD
                var conteudo = @"
**CREDENCIAL SESC**

**CONSENTIMENTO PARA TRATAMENTO DE DADOS PESSOAIS**

Este documento tem por finalidade registrar, de forma clara e objetiva, a manifestação livre, informada e inequívoca pela qual o DECLARANTE consente com o tratamento de seus dados pessoais para finalidades específicas, nos termos da Lei nº 13.709/2018 – Lei Geral de Proteção de Dados Pessoais (LGPD).

Por meio deste instrumento, autorizo o Serviço Social do Comércio – Departamento Regional em Roraima, inscrito no CNPJ sob nº 03.488.834/0001-86, doravante denominado CONTROLADOR, a realizar o tratamento dos meus dados pessoais, inclusive dados sensíveis, bem como os dados dos meus dependentes devidamente cadastrados, em razão do uso das instalações, matrículas/credenciamentos, inscrições e/ou participações nas ações e modalidades de cultura, esporte, lazer, assistência, saúde, educação, ou qualquer outra atividade promovida pela instituição.

**1. Dados Pessoais**

O Controlador fica autorizado a tomar decisões referentes ao tratamento e a realizar o tratamento dos seguintes dados pessoais do Titular:

- Nome completo.
- Data de nascimento.
- Número e imagem da Carteira de Identidade (RG), Número e imagem do Cadastro de Pessoas Físicas (CPF) ou Número e imagem da Carteira Nacional de Habilitação (CNH).
- Fotografia 3x4 ou Fotografia digital - Atualizada
- Estado civil.
- Nível de instrução ou escolaridade, com comprovação via imagem do documento (certificado/diploma e histórico escolar).
- Endereço completo, com comprovação via imagem do documento (comprovante de residência).
- Números de telefone, WhatsApp e endereços de e-mail.

**2. Finalidades do Tratamento dos Dados**

O tratamento dos dados pessoais listados neste termo tem as seguintes finalidades:

- Possibilitar que o Controlador identifique e entre em contato com o Titular para fins de relacionamento comercial.
- Possibilitar que o Controlador elabore contratos comerciais e emita cobranças contra o Titular.
- Possibilitar que o Controlador envie ou forneça ao Titular seus produtos e serviços, de forma remunerada ou gratuita.
- Possibilitar que o Controlador estruture, teste, promova e faça propaganda de produtos e serviços, personalizados ou não ao perfil do Titular.

**3. Compartilhamento de Dados**

O Controlador fica autorizado a compartilhar os dados pessoais do Titular com outros agentes de tratamento de dados para as finalidades listadas neste termo, observados os princípios e as garantias estabelecidas pela Lei nº 13.709.

**4. Segurança dos Dados**

O Controlador responsabiliza-se pela manutenção de medidas de segurança, técnicas e administrativas aptas a proteger os dados pessoais de acessos não autorizados e de situações acidentais ou ilícitas de destruição, perda, alteração, comunicação ou qualquer forma de tratamento inadequado ou ilícito.

**5. Término do Tratamento dos Dados**

O Controlador poderá manter e tratar os dados pessoais do Titular durante todo o período em que eles forem pertinentes ao alcance das finalidades listadas neste termo. Após o alcance da finalidade, os dados pessoais serão armazenados apenas pelo prazo necessário para cumprimento de obrigações legais ou regulatórias.

**6. Direitos do Titular**

O Titular tem direito a obter do Controlador, em relação aos dados por ele tratados, a qualquer momento e mediante requisição:

I. Confirmação da existência de tratamento;
II. Acesso aos dados;
III. Correção de dados incompletos, inexatos ou desatualizados;
IV. Anonimização, bloqueio ou eliminação de dados desnecessários, excessivos ou tratados em desconformidade com o disposto na Lei nº 13.709;
V. Portabilidade dos dados a outro fornecedor de serviço ou produto;
VI. Eliminação dos dados pessoais tratados com o consentimento do titular;
VII. Informação das entidades públicas e privadas com as quais o controlador realizou uso compartilhado de dados;
VIII. Informação sobre a possibilidade de não fornecer consentimento e sobre as consequências da negativa;
IX. Revogação do consentimento, nos termos do § 5º do art. 8º da Lei nº 13.709.

**7. Direito de Revogação do Consentimento**

Este consentimento poderá ser revogado pelo DECLARANTE, a qualquer momento, mediante solicitação via e-mail ou correspondência ao Controlador.

**ENCARREGADO PELO TRATAMENTO DE DADOS (DPO)**

Para dúvidas, solicitações ou exercício de direitos relacionados aos dados pessoais, o titular poderá entrar em contato com o Encarregado pelo Tratamento de Dados (DPO) do SESC Roraima:

- **Nome/Cargo**: Cláudia Abreu – DPO – Data Protection Officer do Sesc/RR
- **E-mail**: lgpd@sescrr.com.br
                ";

                return Ok(new { conteudo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter conteúdo do termo");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        // ✅ NOVO MÉTODO: Obter titular por CPF
        [HttpGet("titular/{cpf}")]
        public async Task<ActionResult> ObterTitularPorCpf(string cpf)
        {
            try
            {
                // VALIDAR CPF
                if (string.IsNullOrEmpty(cpf) || cpf.Length != 11 || !CpfValidator.IsValid(cpf))
                {
                    _logger.LogWarning("❌ CPF inválido recebido: {CPF}", cpf);
                    return BadRequest(new { message = "CPF inválido" });
                }

                var titular = await _termoService.ObterTitularPorCpfAsync(cpf);
                if (titular == null)
                {
                    return NotFound(new { message = "Titular não encontrado" });
                }

                var response = new
                {
                    nome = titular.Nome,
                    cpf = titular.CPF,
                    email = titular.Email,
                    dependentes = titular.Dependentes.Select(d => new
                    {
                        nome = d.Nome,
                        cpf = d.CPF,
                        grauParentesco = d.GrauParentesco
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter titular por CPF: {CPF}", cpf);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        private string ObterIpOrigem()
        {
            // Verificar se há proxy/load balancer
            var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                return forwarded.Split(',').First().Trim();
            }

            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconhecido";
        }

        private string MascararCpf(string? cpf)
        {
            if (string.IsNullOrEmpty(cpf) || cpf.Length < 3)
                return cpf;

            // Mantém apenas os últimos 3 dígitos visíveis
            var mascara = new string('*', cpf.Length - 3) + cpf[^3..];
            return mascara;
        }

        private string MascararIp(string ip)
        {
            // Máscara padrão para IPv4: xxx.xxx.xxx.XXX
            var partes = ip.Split('.');
            if (partes.Length == 4)
            {
                return $"{partes[0]}.{partes[1]}.{partes[2]}.xxx";
            }

            return ip; // Retorna IP original se não for IPv4
        }
    }
}