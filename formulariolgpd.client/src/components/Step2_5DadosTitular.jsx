// src/components/Step2_5DadosTitular.jsx
import React, { useState } from 'react';

const Step2_5DadosTitular = ({ formData, onInputChange, onNext, onPrev }) => {
    const [errors, setErrors] = useState({});

    const formatCPF = (cpf) => {
        const numbers = cpf.replace(/\D/g, '');
        return numbers.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    };

    const validateForm = () => {
        const newErrors = {};
        const dadosTitular = formData.dadosTitular || {};

        if (!dadosTitular.nome?.trim()) newErrors.nome = 'Nome do titular é obrigatório';
        if (!dadosTitular.cpf?.replace(/\D/g, '')) newErrors.cpf = 'CPF do titular é obrigatório';
        if (!dadosTitular.email?.trim()) newErrors.email = 'E-mail do titular é obrigatório';

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        if (validateForm()) {
            onNext();
        }
    };

    const handleCPFChange = (value) => {
        const formatted = formatCPF(value);
        onInputChange('dadosTitular', { 
            ...formData.dadosTitular, 
            cpf: formatted 
        });
    };

    const handleInputChange = (field, value) => {
        onInputChange('dadosTitular', { 
            ...formData.dadosTitular, 
            [field]: value 
        });
    };

    const dadosTitular = formData.dadosTitular || {};

    return (
        <div className="step-container">
            <div className="row justify-content-center">
                <div className="col-md-8">
                    <div className="card border-0 shadow-sm">
                        <div className="card-body p-4">
                            <div className="d-flex align-items-center mb-4">
                                <div className="step-icon me-3">
                                    <i className="bi bi-person-check fs-4 text-primary"></i>
                                </div>
                                <h4 className="mb-0">Dados do Titular</h4>
                            </div>

                            <div className="alert alert-info mb-4">
                                <i className="bi bi-info-circle me-2"></i>
                                <strong>Atenção:</strong> Informe os dados do titular ao qual você (dependente maior) está vinculado.
                            </div>

                            <form onSubmit={handleSubmit}>
                                <div className="row">
                                    <div className="col-md-12 mb-3">
                                        <label className="form-label">Nome Completo do Titular *</label>
                                        <input
                                            type="text"
                                            className={`form-control ${errors.nome ? 'is-invalid' : ''}`}
                                            placeholder="Digite o nome completo do titular"
                                            value={dadosTitular.nome || ''}
                                            onChange={(e) => handleInputChange('nome', e.target.value)}
                                        />
                                        {errors.nome && <div className="invalid-feedback">{errors.nome}</div>}
                                    </div>

                                    <div className="col-md-6 mb-3">
                                        <label className="form-label">CPF do Titular *</label>
                                        <input
                                            type="text"
                                            className={`form-control ${errors.cpf ? 'is-invalid' : ''}`}
                                            placeholder="000.000.000-00"
                                            value={dadosTitular.cpf || ''}
                                            onChange={(e) => handleCPFChange(e.target.value)}
                                            maxLength="14"
                                        />
                                        {errors.cpf && <div className="invalid-feedback">{errors.cpf}</div>}
                                    </div>

                                    <div className="col-md-6 mb-3">
                                        <label className="form-label">E-mail do Titular *</label>
                                        <input
                                            type="email"
                                            className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                                            placeholder="email@titular.com"
                                            value={dadosTitular.email || ''}
                                            onChange={(e) => handleInputChange('email', e.target.value)}
                                        />
                                        {errors.email && <div className="invalid-feedback">{errors.email}</div>}
                                    </div>

                                    <div className="col-md-12 mb-3">
                                        <label className="form-label">Grau de Parentesco</label>
                                        <select
                                            className="form-select"
                                            value={dadosTitular.grauParentesco || ''}
                                            onChange={(e) => handleInputChange('grauParentesco', e.target.value)}
                                        >
                                            <option value="">Selecione o grau de parentesco</option>
                                            <option value="filho">Filho(a)</option>
                                            <option value="conjuge">Cônjuge</option>
                                            <option value="enteado">Enteado(a)</option>
                                            <option value="irmao">Irmão/Irmã</option>
                                            <option value="neto">Neto(a)</option>
                                            <option value="sobrinho">Sobrinho(a)</option>
                                            <option value="genro-nora">Genro/Nora</option>
                                            <option value="outro">Outro</option>
                                        </select>
                                    </div>
                                </div>

                                <div className="alert alert-warning">
                                    <i className="bi bi-exclamation-triangle me-2"></i>
                                    <strong>Importante:</strong> Certifique-se de que o titular autoriza o tratamento dos seus dados pessoais.
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

export default Step2_5DadosTitular;