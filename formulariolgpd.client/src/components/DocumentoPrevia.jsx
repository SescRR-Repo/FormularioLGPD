// src/components/DocumentoPrevia.jsx
import React from 'react';

const DocumentoPrevia = ({ formData, onConfirm, onPrev, loading }) => {
    const formatCPF = (cpf) => {
        if (!cpf) return '';
        const numbers = cpf.replace(/\D/g, '');
        if (numbers.length === 11) {
            return `${numbers.substring(0, 3)}.${numbers.substring(3, 6)}.${numbers.substring(6, 9)}-${numbers.substring(9, 11)}`;
        }
        return cpf;
    };

    const obterDescricaoQualificacao = (qualificacao) => {
        const qualificacoes = {
            'titular': 'Titular',
            'dependenteMaior': 'Dependente maior de idade',
            'responsavelMenor': 'Responsável por menor trabalhador',
            'responsavelOrfao': 'Responsável por dependente órfão do titular',
            'curadorTutor': 'Curador, Tutor ou Guardião legal'
        };
        return qualificacoes[qualificacao] || 'Não especificado';
    };

    const obterEscolaridadeLabel = (value) => {
        const opcoes = {
            'fundamental-incompleto': 'Ensino Fundamental Incompleto',
            'fundamental-completo': 'Ensino Fundamental Completo',
            'medio-incompleto': 'Ensino Médio Incompleto',
            'medio-completo': 'Ensino Médio Completo',
            'tecnico-incompleto': 'Ensino Técnico Incompleto',
            'tecnico-completo': 'Ensino Técnico Completo',
            'superior-incompleto': 'Ensino Superior Incompleto',
            'superior-completo': 'Ensino Superior Completo',
            'pos-graduacao': 'Pós-Graduação',
            'mestrado': 'Mestrado',
            'doutorado': 'Doutorado'
        };
        return opcoes[value] || value || 'Não informado';
    };

    const obterSituacaoLabel = (value) => {
        const opcoes = {
            'concluido': 'Concluído',
            'cursando': 'Cursando',
            'trancado': 'Trancado',
            'interrompido': 'Interrompido',
            'nao-se-aplica': 'Não se aplica'
        };
        return opcoes[value] || value || 'Não informado';
    };

    return (
        <div className="step-container">
            <div className="row justify-content-center">
                <div className="col-md-10">
                    <div className="card border-0 shadow-sm">
                        <div className="card-body p-4">
                            <div className="d-flex align-items-center mb-4">
                                <div className="step-icon me-3">
                                    <i className="bi bi-file-earmark-text fs-4 text-success"></i>
                                </div>
                                <h4 className="mb-0">Prévia do Documento</h4>
                            </div>

                            <div className="alert alert-success">
                                <i className="bi bi-check-circle me-2"></i>
                                <strong>Documento processado com sucesso!</strong> Confira os dados abaixo antes de finalizar.
                            </div>

                            {/* Prévia do Documento */}
                            <div className="card bg-light mb-4">
                                <div className="card-body">
                                    <div className="preview-document">
                                        <div className="text-center mb-4">
                                            <h3 className="text-primary fw-bold">CREDENCIAL SESC</h3>
                                            <div className="mb-3">
                                                <span className="badge bg-primary me-2">✓ Cadastro</span>
                                                <span className="badge bg-secondary me-2">Renovação</span>
                                                <span className="badge bg-secondary">Inclusão de dependente</span>
                                            </div>
                                            <h5 className="text-muted">CONSENTIMENTO PARA TRATAMENTO DE DADOS PESSOAIS</h5>
                                        </div>

                                        <div className="documento-content">
                                            <p className="text-justify">
                                                Por meio deste instrumento, eu, <strong>{formData.nome}</strong>, 
                                                inscrito(a) no CPF nº <strong>{formatCPF(formData.cpf)}</strong>, 
                                                estado civil <strong>{formData.estadoCivil || 'Não informado'}</strong>, 
                                                natural de <strong>{formData.naturalidade || 'Não informado'}</strong>, 
                                                na qualidade de <strong>{obterDescricaoQualificacao(formData.qualificacao)}</strong>, 
                                                autorizo o Serviço Social do Comércio – Departamento Regional em Roraima, 
                                                inscrito no CNPJ sob nº 03.488.834/0001-86, doravante denominado CONTROLADOR, 
                                                a realizar o tratamento dos meus dados pessoais, inclusive dados sensíveis, bem como os dados dos meus dependentes devidamente cadastrados.
                                            </p>

                                            <div className="row mt-4">
                                                <div className="col-md-6">
                                                    <h6 className="fw-bold text-primary">Dados do Declarante:</h6>
                                                    <div className="bg-white p-3 rounded">
                                                        <p className="mb-1"><strong>Escolaridade:</strong> {obterEscolaridadeLabel(formData.escolaridade)}</p>
                                                        <p className="mb-1"><strong>Situação:</strong> {obterSituacaoLabel(formData.situacao)}</p>
                                                        <p className="mb-1"><strong>Telefone:</strong> {formData.telefone || 'Não informado'}</p>
                                                        <p className="mb-0"><strong>E-mail:</strong> {formData.email}</p>
                                                    </div>
                                                </div>

                                                {formData.dadosTitular && (
                                                    <div className="col-md-6">
                                                        <h6 className="fw-bold text-warning">Dados do Titular:</h6>
                                                        <div className="bg-white p-3 rounded">
                                                            <p className="mb-1"><strong>Nome:</strong> {formData.dadosTitular.nome}</p>
                                                            <p className="mb-1"><strong>CPF:</strong> {formatCPF(formData.dadosTitular.cpf)}</p>
                                                            <p className="mb-1"><strong>E-mail:</strong> {formData.dadosTitular.email}</p>
                                                            <p className="mb-0"><strong>Parentesco:</strong> {formData.dadosTitular.grauParentesco}</p>
                                                        </div>
                                                    </div>
                                                )}
                                            </div>

                                            {formData.dependentes && formData.dependentes.length > 0 && (
                                                <div className="mt-4">
                                                    <h6 className="fw-bold text-success">Dependentes menores de 18 anos:</h6>
                                                    <div className="bg-white p-3 rounded">
                                                        {formData.dependentes.map((dep, index) => (
                                                            <div key={index} className="mb-2">
                                                                <strong>Nome:</strong> {dep.nome} | 
                                                                <strong> CPF:</strong> {formatCPF(dep.cpf)} | 
                                                                <strong> Parentesco:</strong> {dep.grauParentesco}
                                                            </div>
                                                        ))}
                                                    </div>
                                                </div>
                                            )}

                                            <div className="mt-4 p-3 bg-info bg-opacity-10 rounded">
                                                <h6 className="fw-bold">DADOS DO ACEITE ELETRÔNICO:</h6>
                                                <p className="mb-1"><strong>Data/Hora do Aceite:</strong> {new Date().toLocaleString('pt-BR')}</p>
                                                <p className="mb-1"><strong>IP de Origem:</strong> {navigator.userAgent.includes('localhost') ? '::1' : 'IP do usuário'}</p>
                                                <p className="mb-0"><strong>Versão do Termo:</strong> 1.0</p>
                                            </div>

                                            <div className="text-center mt-4">
                                                <div style={{ borderBottom: '2px solid #333', width: '400px', margin: '0 auto', height: '50px' }}></div>
                                                <strong>Assinatura do(a) declarante dos dados ou responsável legal</strong>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div className="alert alert-info">
                                <i className="bi bi-info-circle me-2"></i>
                                <strong>Próximo passo:</strong> Confirme os dados acima e clique em "Finalizar" para salvar o termo no sistema.
                            </div>

                            <div className="d-flex justify-content-between">
                                <button
                                    type="button"
                                    className="btn btn-outline-secondary"
                                    onClick={onPrev}
                                    disabled={loading}
                                >
                                    <i className="bi bi-arrow-left me-1"></i> Voltar e Corrigir
                                </button>
                                <button
                                    type="button"
                                    className="btn btn-success"
                                    onClick={onConfirm}
                                    disabled={loading}
                                >
                                    {loading ? (
                                        <>
                                            <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                                            Salvando...
                                        </>
                                    ) : (
                                        <>
                                            <i className="bi bi-check-circle me-1"></i>
                                            Confirmar e Finalizar
                                        </>
                                    )}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default DocumentoPrevia;