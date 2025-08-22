// src/components/Step2DadosPessoais.jsx
import React, { useState } from 'react';
import { validateCPF, applyCPFMask } from '../utils/cpfValidator'; // IMPORTAR VALIDADOR

const Step2DadosPessoais = ({ formData, onInputChange, onNext, onPrev }) => {
    const [errors, setErrors] = useState({});

    const formatPhone = (phone) => {
        const numbers = phone.replace(/\D/g, '');
        if (numbers.length <= 10) {
            return numbers.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
        }
        return numbers.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
    };

    const validateForm = () => {
        const newErrors = {};

        // Validações existentes
        if (!formData.nome?.trim()) newErrors.nome = 'Nome é obrigatório';
        if (!formData.estadoCivil) newErrors.estadoCivil = 'Estado civil é obrigatório';
        if (!formData.naturalidade?.trim()) newErrors.naturalidade = 'Naturalidade é obrigatória';
        if (!formData.email?.trim()) newErrors.email = 'E-mail é obrigatório';

        // VALIDAÇÃO MELHORADA DO CPF - AGORA VALIDA PARA TODAS AS QUALIFICAÇÕES
        const cpfLimpo = formData.cpf?.replace(/\D/g, '');
        if (!cpfLimpo) {
            newErrors.cpf = 'CPF é obrigatório';
        } else if (cpfLimpo.length !== 11) {
            newErrors.cpf = 'CPF deve ter 11 dígitos';
        } else if (!validateCPF(cpfLimpo)) {
            newErrors.cpf = 'CPF inválido';
        }

        // VALIDAÇÃO ESPECIAL PARA DEPENDENTE MAIOR DE IDADE
        if (formData.qualificacao === 'dependenteMaior') {
            if (!formData.dadosTitular?.nome?.trim()) {
                newErrors.nomeTitular = 'Nome do titular é obrigatório para dependente maior';
            }
            if (!formData.dadosTitular?.cpf?.replace(/\D/g, '')) {
                newErrors.cpfTitular = 'CPF do titular é obrigatório para dependente maior';
            } else {
                const cpfTitularLimpo = formData.dadosTitular.cpf.replace(/\D/g, '');
                if (cpfTitularLimpo.length === 11 && !validateCPF(cpfTitularLimpo)) {
                    newErrors.cpfTitular = 'CPF do titular é inválido';
                }
            }
            if (!formData.dadosTitular?.email?.trim()) {
                newErrors.emailTitular = 'E-mail do titular é obrigatório para dependente maior';
            }
            if (!formData.dadosTitular?.grauParentesco?.trim()) {
                newErrors.grauParentesco = 'Grau de parentesco é obrigatório para dependente maior';
            }
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        if (validateForm()) {
            onNext();
        }
    };

    // NOVA FUNÇÃO COM VALIDAÇÃO EM TEMPO REAL
    const handleCPFChange = (value) => {
        const formatted = applyCPFMask(value);
        onInputChange('cpf', formatted);

        // Validar em tempo real quando o CPF estiver completo
        const cpfLimpo = formatted.replace(/\D/g, '');
        if (cpfLimpo.length === 11) {
            if (!validateCPF(cpfLimpo)) {
                setErrors(prev => ({ ...prev, cpf: 'CPF inválido' }));
            } else {
                setErrors(prev => {
                    const newErrors = { ...prev };
                    delete newErrors.cpf;
                    return newErrors;
                });
            }
        }
    };

    // NOVA FUNÇÃO PARA VALIDAR CPF DO TITULAR
    const handleCPFTitularChange = (value) => {
        const formatted = applyCPFMask(value);
        onInputChange('dadosTitular', { 
            ...formData.dadosTitular, 
            cpf: formatted 
        });

        // Validar em tempo real quando o CPF estiver completo
        const cpfLimpo = formatted.replace(/\D/g, '');
        if (cpfLimpo.length === 11) {
            if (!validateCPF(cpfLimpo)) {
                setErrors(prev => ({ ...prev, cpfTitular: 'CPF do titular é inválido' }));
            } else {
                setErrors(prev => {
                    const newErrors = { ...prev };
                    delete newErrors.cpfTitular;
                    return newErrors;
                });
            }
        }
    };

    const handlePhoneChange = (value) => {
        const formatted = formatPhone(value);
        onInputChange('telefone', formatted);
    };

    // Opções de escolaridade
    const opcoesEscolaridade = [
        { value: '', label: 'Selecione...' },
        { value: 'fundamental-incompleto', label: 'Ensino Fundamental Incompleto' },
        { value: 'fundamental-completo', label: 'Ensino Fundamental Completo' },
        { value: 'medio-incompleto', label: 'Ensino Médio Incompleto' },
        { value: 'medio-completo', label: 'Ensino Médio Completo' },
        { value: 'tecnico-incompleto', label: 'Ensino Técnico Incompleto' },
        { value: 'tecnico-completo', label: 'Ensino Técnico Completo' },
        { value: 'superior-incompleto', label: 'Ensino Superior Incompleto' },
        { value: 'superior-completo', label: 'Ensino Superior Completo' },
        { value: 'pos-graduacao', label: 'Pós-Graduação' },
        { value: 'mestrado', label: 'Mestrado' },
        { value: 'doutorado', label: 'Doutorado' }
    ];

    // Opções de situação acadêmica
    const opcoesSituacao = [
        { value: '', label: 'Selecione...' },
        { value: 'concluido', label: 'Concluído' },
        { value: 'cursando', label: 'Cursando' },
        { value: 'trancado', label: 'Trancado' },
        { value: 'interrompido', label: 'Interrompido' },
        { value: 'nao-se-aplica', label: 'Não se aplica' }
    ];

    return (
        <div className="step-container">
            <div className="row justify-content-center">
                <div className="col-md-8">
                    <div className="card border-0 shadow-sm">
                        <div className="card-body p-4">
                            <div className="d-flex align-items-center mb-4">
                                <div className="step-icon me-3">
                                    <i className="bi bi-person fs-4 text-primary"></i>
                                </div>
                                <h4 className="mb-0">Dados Pessoais</h4>
                            </div>

                            <form onSubmit={handleSubmit}>
                                <div className="row">
                                    <div className="col-md-12 mb-3">
                                        <label className="form-label">Nome Completo *</label>
                                        <input
                                            type="text"
                                            className={`form-control ${errors.nome ? 'is-invalid' : ''}`}
                                            placeholder="Digite seu nome completo"
                                            value={formData.nome || ''}
                                            onChange={(e) => onInputChange('nome', e.target.value)}
                                        />
                                        {errors.nome && <div className="invalid-feedback">{errors.nome}</div>}
                                    </div>

                                    <div className="col-md-6 mb-3">
                                        <label className="form-label">CPF *</label>
                                        <input
                                            type="text"
                                            className={`form-control ${errors.cpf ? 'is-invalid' : ''}`}
                                            placeholder="000.000.000-00"
                                            value={formData.cpf || ''}
                                            onChange={(e) => handleCPFChange(e.target.value)}
                                            maxLength="14"
                                        />
                                        {errors.cpf && <div className="invalid-feedback">{errors.cpf}</div>}
                                        {/* FEEDBACK VISUAL PARA CPF VÁLIDO */}
                                        {formData.cpf?.replace(/\D/g, '').length === 11 && !errors.cpf && (
                                            <div className="text-success small mt-1">
                                                <i className="bi bi-check-circle me-1"></i>CPF válido
                                            </div>
                                        )}
                                    </div>

                                    <div className="col-md-6 mb-3">
                                        <label className="form-label">Estado Civil *</label>
                                        <select
                                            className={`form-select ${errors.estadoCivil ? 'is-invalid' : ''}`}
                                            value={formData.estadoCivil || ''}
                                            onChange={(e) => onInputChange('estadoCivil', e.target.value)}
                                        >
                                            <option value="">Selecione</option>
                                            <option value="solteiro">Solteiro(a)</option>
                                            <option value="casado">Casado(a)</option>
                                            <option value="divorciado">Divorciado(a)</option>
                                            <option value="viuvo">Viúvo(a)</option>
                                            <option value="uniao-estavel">União Estável</option>
                                        </select>
                                        {errors.estadoCivil && <div className="invalid-feedback">{errors.estadoCivil}</div>}
                                    </div>

                                    <div className="col-md-12 mb-3">
                                        <label className="form-label">Naturalidade *</label>
                                        <input
                                            type="text"
                                            className={`form-control ${errors.naturalidade ? 'is-invalid' : ''}`}
                                            placeholder="Cidade/Estado onde nasceu"
                                            value={formData.naturalidade || ''}
                                            onChange={(e) => onInputChange('naturalidade', e.target.value)}
                                        />
                                        {errors.naturalidade && <div className="invalid-feedback">{errors.naturalidade}</div>}
                                    </div>

                                    <div className="col-md-6 mb-3">
                                        <label className="form-label">Escolaridade</label>
                                        <select
                                            className="form-select"
                                            value={formData.escolaridade || ''}
                                            onChange={(e) => onInputChange('escolaridade', e.target.value)}
                                        >
                                            {opcoesEscolaridade.map(opcao => (
                                                <option key={opcao.value} value={opcao.value}>
                                                    {opcao.label}
                                                </option>
                                            ))}
                                        </select>
                                    </div>

                                    <div className="col-md-6 mb-3">
                                        <label className="form-label">Situação</label>
                                        <select
                                            className="form-select"
                                            value={formData.situacao || ''}
                                            onChange={(e) => onInputChange('situacao', e.target.value)}
                                        >
                                            {opcoesSituacao.map(opcao => (
                                                <option key={opcao.value} value={opcao.value}>
                                                    {opcao.label}
                                                </option>
                                            ))}
                                        </select>
                                    </div>

                                    <div className="col-md-6 mb-3">
                                        <label className="form-label">Telefone</label>
                                        <input
                                            type="text"
                                            className="form-control"
                                            placeholder="(95) 99999-9999"
                                            value={formData.telefone || ''}
                                            onChange={(e) => handlePhoneChange(e.target.value)}
                                        />
                                    </div>

                                    <div className="col-md-6 mb-3">
                                        <label className="form-label">E-mail *</label>
                                        <input
                                            type="email"
                                            className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                                            placeholder="seu@email.com"
                                            value={formData.email || ''}
                                            onChange={(e) => onInputChange('email', e.target.value)}
                                        />
                                        {errors.email && <div className="invalid-feedback">{errors.email}</div>}
                                    </div>

                                    {/* SEÇÃO ADICIONAL PARA DEPENDENTE MAIOR DE IDADE */}
                                    {formData.qualificacao === 'dependenteMaior' && (
                                        <div className="col-md-12">
                                            <div className="card bg-warning bg-opacity-10 border-warning mb-3">
                                                <div className="card-header">
                                                    <h6 className="mb-0 text-warning">
                                                        <i className="bi bi-exclamation-triangle me-2"></i>
                                                        Dados do Titular (Obrigatório para Dependente Maior)
                                                    </h6>
                                                </div>
                                                <div className="card-body">
                                                    <div className="row">
                                                        <div className="col-md-6 mb-3">
                                                            <label className="form-label">Nome do Titular *</label>
                                                            <input
                                                                type="text"
                                                                className={`form-control ${errors.nomeTitular ? 'is-invalid' : ''}`}
                                                                placeholder="Nome completo do titular"
                                                                value={formData.dadosTitular?.nome || ''}
                                                                onChange={(e) => onInputChange('dadosTitular', { 
                                                                    ...formData.dadosTitular, 
                                                                    nome: e.target.value 
                                                                })}
                                                            />
                                                            {errors.nomeTitular && <div className="invalid-feedback">{errors.nomeTitular}</div>}
                                                        </div>

                                                        <div className="col-md-6 mb-3">
                                                            <label className="form-label">CPF do Titular *</label>
                                                            <input
                                                                type="text"
                                                                className={`form-control ${errors.cpfTitular ? 'is-invalid' : ''}`}
                                                                placeholder="000.000.000-00"
                                                                value={formData.dadosTitular?.cpf || ''}
                                                                onChange={(e) => handleCPFTitularChange(e.target.value)}
                                                                maxLength="14"
                                                            />
                                                            {errors.cpfTitular && <div className="invalid-feedback">{errors.cpfTitular}</div>}
                                                            {/* FEEDBACK VISUAL PARA CPF VÁLIDO */}
                                                            {formData.dadosTitular?.cpf?.replace(/\D/g, '').length === 11 && !errors.cpfTitular && (
                                                                <div className="text-success small mt-1">
                                                                    <i className="bi bi-check-circle me-1"></i>CPF do titular válido
                                                                </div>
                                                            )}
                                                        </div>

                                                        <div className="col-md-6 mb-3">
                                                            <label className="form-label">E-mail do Titular *</label>
                                                            <input
                                                                type="email"
                                                                className={`form-control ${errors.emailTitular ? 'is-invalid' : ''}`}
                                                                placeholder="email@titular.com"
                                                                value={formData.dadosTitular?.email || ''}
                                                                onChange={(e) => onInputChange('dadosTitular', { 
                                                                    ...formData.dadosTitular, 
                                                                    email: e.target.value 
                                                                })}
                                                            />
                                                            {errors.emailTitular && <div className="invalid-feedback">{errors.emailTitular}</div>}
                                                        </div>

                                                        <div className="col-md-6 mb-3">
                                                            <label className="form-label">Grau de Parentesco *</label>
                                                            <select
                                                                className={`form-select ${errors.grauParentesco ? 'is-invalid' : ''}`}
                                                                value={formData.dadosTitular?.grauParentesco || ''}
                                                                onChange={(e) => onInputChange('dadosTitular', { 
                                                                    ...formData.dadosTitular, 
                                                                    grauParentesco: e.target.value 
                                                                })}
                                                            >
                                                                <option value="">Selecione o parentesco</option>
                                                                <option value="filho">Filho(a)</option>
                                                                <option value="enteado">Enteado(a)</option>
                                                                <option value="neto">Neto(a)</option>
                                                                <option value="sobrinho">Sobrinho(a)</option>
                                                                <option value="irmao">Irmão/Irmã</option>
                                                                <option value="outro">Outro</option>
                                                            </select>
                                                            {errors.grauParentesco && <div className="invalid-feedback">{errors.grauParentesco}</div>}
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    )}
                                </div>

                                <div className="d-flex justify-content-between">
                                    <button type="button" className="btn btn-outline-secondary" onClick={onPrev}>
                                        <i className="bi bi-arrow-left me-1"></i> Voltar
                                    </button>
                                    <button type="submit" className="btn btn-primary">
                                        Próximo <i className="bi bi-arrow-right ms-1"></i>
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

export default Step2DadosPessoais;