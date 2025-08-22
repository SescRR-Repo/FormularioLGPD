// src/components/Step4Consentimentos.jsx
import React, { useState } from 'react';

const Step4Consentimentos = ({ formData, onInputChange, onSubmit, onPrev, loading }) => {
    const [consentimentos, setConsentimentos] = useState({
        consentimentoGeral: false,
        consentimentoMenores: false
    });

    const handleConsentimentoChange = (campo, valor) => {
        const novosConsentimentos = { ...consentimentos, [campo]: valor };
        setConsentimentos(novosConsentimentos);
        onInputChange('consentimentos', novosConsentimentos);
    };

    // Verificar se há dependentes menores de idade
    const temDependentesMenores = formData.dependentes?.some(dep => {
        if (!dep.dataNascimento) return false;
        const hoje = new Date();
        const nascimento = new Date(dep.dataNascimento);
        const idade = hoje.getFullYear() - nascimento.getFullYear();
        const mesAtual = hoje.getMonth();
        const mesNascimento = nascimento.getMonth();
        
        if (mesAtual < mesNascimento || (mesAtual === mesNascimento && hoje.getDate() < nascimento.getDate())) {
            return (idade - 1) < 18;
        }
        return idade < 18;
    }) || false;

    // Validação: consentimento geral sempre obrigatório + consentimento menores se aplicável
    const consentimentosValidos = consentimentos.consentimentoGeral && 
        (temDependentesMenores ? consentimentos.consentimentoMenores : true);

    const handleSubmit = (e) => {
        e.preventDefault();
        if (consentimentosValidos) {
            onSubmit();
        }
    };

    const conteudoTermo = `
CREDENCIAL SESC

CONSENTIMENTO PARA TRATAMENTO DE DADOS PESSOAIS

Este documento tem por finalidade registrar, de forma clara e objetiva, a manifestação livre, informada e inequívoca pela qual o DECLARANTE consente com o tratamento de seus dados pessoais para finalidades específicas, nos termos da Lei nº 13.709/2018 – Lei Geral de Proteção de Dados Pessoais (LGPD).

Por meio deste instrumento, autorizo o Serviço Social do Comércio – Departamento Regional em Roraima, inscrito no CNPJ sob nº 03.488.834/0001-86, doravante denominado CONTROLADOR, a realizar o tratamento dos meus dados pessoais, inclusive dados sensíveis, bem como os dados dos meus dependentes devidamente cadastrados, em razão do uso das instalações, matrículas/credenciamentos, inscrições e/ou participações nas ações e modalidades de cultura, esporte, lazer, assistência, saúde, educação, ou qualquer outra atividade promovida pela instituição.

1. Dados Pessoais

O Controlador fica autorizado a tomar decisões referentes ao tratamento e a realizar o tratamento dos seguintes dados pessoais do Titular:

- Nome completo.
- Data de nascimento.
- Número e imagem da Carteira de Identidade (RG), Número e imagem do Cadastro de Pessoas Físicas (CPF).
- Estado civil.
- Nível de instrução ou escolaridade.
- Endereço completo.
- Números de telefone, WhatsApp e endereços de e-mail.

2. Finalidades do Tratamento dos Dados

O tratamento dos dados pessoais listados neste termo tem as seguintes finalidades:

- Possibilitar que o Controlador identifique e entre em contato com o Titular para fins de relacionamento comercial.
- Possibilitar que o Controlador elabore contratos comerciais e emita cobranças contra o Titular.
- Possibilitar que o Controlador envie ou forneça ao Titular seus produtos e serviços.

3. Direitos do Titular

O Titular tem direito a obter do Controlador, em relação aos dados por ele tratados:

I. Confirmação da existência de tratamento;
II. Acesso aos dados;
III. Correção de dados incompletos, inexatos ou desatualizados;
IV. Anonimização, bloqueio ou eliminação de dados desnecessários;
V. Portabilidade dos dados;
VI. Eliminação dos dados pessoais tratados com o consentimento do titular;
VII. Revogação do consentimento.

ENCARREGADO PELO TRATAMENTO DE DADOS (DPO)

Para dúvidas, solicitações ou exercício de direitos relacionados aos dados pessoais:

- Nome/Cargo: Cláudia Abreu – DPO do Sesc/RR
- E-mail: lgpd@sescrr.com.br
  `;

    return (
        <div className="step-container">
            <div className="row justify-content-center">
                <div className="col-md-10">
                    <div className="card border-0 shadow-sm">
                        <div className="card-body p-4">
                            <div className="d-flex align-items-center mb-4">
                                <div className="step-icon me-3">
                                    <i className="bi bi-shield-check fs-4 text-primary"></i>
                                </div>
                                <h4 className="mb-0">Consentimentos LGPD</h4>
                            </div>

                            {/* Exibição do Termo */}
                            <div className="card mb-4 bg-light">
                                <div className="card-header">
                                    <h6 className="mb-0">
                                        <i className="bi bi-file-text me-2"></i>
                                        Termo de Consentimento - LGPD
                                    </h6>
                                </div>
                                <div className="card-body" style={{ maxHeight: '400px', overflowY: 'auto' }}>
                                    <div className="termo-content">
                                        {conteudoTermo.split('\n').map((linha, index) => (
                                            <p key={index} className="mb-2" style={{ fontSize: '0.9rem', lineHeight: '1.4' }}>
                                                {linha.startsWith('**') ? (
                                                    <strong>{linha.replace(/\*\*/g, '')}</strong>
                                                ) : (
                                                    linha
                                                )}
                                            </p>
                                        ))}
                                    </div>
                                </div>
                            </div>

                            <form onSubmit={handleSubmit}>
                                <div className="alert alert-info">
                                    <strong>Para finalizar, é necessário concordar com os termos de consentimento:</strong>
                                </div>

                                {/* Consentimentos */}
                                <div className="mb-4">
                                    {/* Consentimento Geral - SEMPRE OBRIGATÓRIO */}
                                    <div className="card mb-3 border">
                                        <div className="card-body">
                                            <div className="form-check">
                                                <input
                                                    className="form-check-input"
                                                    type="checkbox"
                                                    id="consentimentoGeral"
                                                    checked={consentimentos.consentimentoGeral}
                                                    onChange={(e) => handleConsentimentoChange('consentimentoGeral', e.target.checked)}
                                                />
                                                <label className="form-check-label" htmlFor="consentimentoGeral">
                                                    <strong>Consentimento para Tratamento de Dados Pessoais</strong>
                                                    <br />
                                                    <small className="text-justify">
                                                        Declaro que li e compreendi os termos deste documento, estou ciente das finalidades e das formas de tratamento e compartilhamento dos meus dados pessoais, bem como dos direitos que me são assegurados. Autorizo, de forma livre, informada e inequívoca, o tratamento dos meus dados pessoais e dos dados de meus dependentes, nos termos aqui estabelecidos. Reconheço que posso revogar este consentimento a qualquer momento e que os dados serão armazenados apenas pelo tempo necessário ao cumprimento das finalidades, respeitados os prazos legais.
                                                    </small>
                                                </label>
                                            </div>
                                        </div>
                                    </div>

                                    {/* Consentimento para Menores - CONDICIONAL */}
                                    {temDependentesMenores && (
                                        <div className="card mb-3 border border-warning">
                                            <div className="card-body">
                                                <div className="form-check">
                                                    <input
                                                        className="form-check-input"
                                                        type="checkbox"
                                                        id="consentimentoMenores"
                                                        checked={consentimentos.consentimentoMenores}
                                                        onChange={(e) => handleConsentimentoChange('consentimentoMenores', e.target.checked)}
                                                    />
                                                    <label className="form-check-label" htmlFor="consentimentoMenores">
                                                        <strong>Autorização para Tratamento de Dados de Menores</strong>
                                                        <br />
                                                        <small className="text-justify">
                                                            Declaro, na qualidade de pai/mãe ou responsável legal, autorizo expressamente, de forma livre, informada e inequívoca, o tratamento dos dados pessoais do(s) menor(es) sob minha responsabilidade, conforme as finalidades deste termo, estando ciente das formas de tratamento, do eventual compartilhamento e dos direitos que me assistem nos termos da Lei Geral de Proteção de Dados. 
                                                            <em>(Obrigatório conforme art. 14, §1º da LGPD)</em>
                                                        </small>
                                                    </label>
                                                </div>
                                            </div>
                                        </div>
                                    )}

                                    {temDependentesMenores && (
                                        <div className="alert alert-warning">
                                            <i className="bi bi-exclamation-triangle me-2"></i>
                                            <strong>Atenção:</strong> Foi detectado que você possui dependentes menores de 18 anos. 
                                            É obrigatório o consentimento específico para tratamento de dados de menores.
                                        </div>
                                    )}
                                </div>

                                <div className="d-flex justify-content-between">
                                    <button
                                        type="button"
                                        className="btn btn-outline-secondary"
                                        onClick={onPrev}
                                        disabled={loading}
                                    >
                                        <i className="bi bi-arrow-left me-1"></i> Voltar
                                    </button>
                                    <button
                                        type="submit"
                                        className={`btn ${consentimentosValidos ? 'btn-success' : 'btn-secondary'}`}
                                        disabled={!consentimentosValidos || loading}
                                    >
                                        {loading ? (
                                            <>
                                                <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                                                Processando...
                                            </>
                                        ) : (
                                            <>
                                                <i className="bi bi-file-earmark-pdf me-1"></i>
                                                Gerar Documento
                                            </>
                                        )}
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Step4Consentimentos;