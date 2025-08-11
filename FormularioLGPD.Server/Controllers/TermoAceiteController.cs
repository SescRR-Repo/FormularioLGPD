// Controllers/TermoAceiteController.cs
using Microsoft.AspNetCore.Mvc;
using FormularioLGPD.Server.Services.Interfaces;
using FormularioLGPD.Server.DTOs;
using FormularioLGPD.Server.Models;

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
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Capturar IP e User-Agent
                var ipOrigem = ObterIpOrigem();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var resultado = await _termoService.CriarTermoAceiteAsync(dto, ipOrigem, userAgent);

                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar termo de aceite");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao criar termo de aceite");
                return StatusCode(500, new { message = "Erro interno do servidor" });
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
                // Limpar CPF (remover pontos e traços)
                var cpfLimpo = new string(cpf.Where(char.IsDigit).ToArray());

                if (cpfLimpo.Length != 11)
                {
                    return BadRequest(new { message = "CPF deve conter 11 dígitos" });
                }

                var existe = await _termoService.ValidarCpfExistenteAsync(cpfLimpo);
                return Ok(new { existe, message = existe ? "CPF já possui termo ativo" : "CPF disponível" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar CPF: {CPF}", cpf);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Faz download do PDF do termo
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

                var pdfBytes = await _termoService.GerarPdfTermoAsync(id);
                var nomeArquivo = $"termo_lgpd_{termo.NumeroTermo}.pdf";

                return File(pdfBytes, "application/pdf", nomeArquivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar PDF do termo {Id}", id);
                return StatusCode(500, new { message = "Erro ao gerar PDF" });
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
    }
}