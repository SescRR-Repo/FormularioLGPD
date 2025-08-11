// src/components/Step3Dependentes.jsx
import React, { useState } from 'react';

const Step3Dependentes = ({ formData, onInputChange, onNext, onPrev }) => {
    const [dependentes, setDependentes] = useState(formData.dependentes || []);

    const adicionarDependente = () => {
        const novoDependente = {
            id: Date.now(),
            nome: '',
            cpf: '',
            grauParentesco: ''
        };
        const novosDependentes = [...dependentes, novoDependente];
        setDependentes(novosDependentes);
        onInputChange('dependentes', novosDependentes);
    };

    const removerDependente = (id) => {
        const novosDependentes = dependentes.filter(dep => dep.id !== id);
        setDependentes(novosDependentes);
        onInputChange('dependentes', novosDependentes);
    };

    const atualizarDependente = (id, campo, valor) => {
        const novosDependentes = dependentes.map(dep =>
            dep.id === id ? { ...dep, [campo]: valor } : dep
        );
        setDependentes(novosDependentes);
        onInputChange('dependentes', novosDependentes);
    };

    const formatCPF = (cpf) => {
        const numbers = cpf.replace(/\D/g, '');
        return numbers.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        onNext();
    };

    return (
        <div className="step-container">
            <div className="row justify-content-center">
                <div className="col-md-10">
                    <div className="card border-0 shadow-sm">
                        <div className="card-body p-4">
                            <div className="d-flex align-items-center mb-4">
                                <div className="step-icon me-3">
                                    <i className="bi bi-people fs-4 text-primary"></i>
                                </div>
                                <h4 className="mb-0">Dependentes</h4>
                            </div>

                            <p className="text-muted mb-4">
                                Adicione os dependentes menores de 18 anos ou legalmente representados (opcional):
                            </p>

                            <form onSubmit={handleSubmit}>
                                {dependentes.map((dependente, index) => (
                                    <div key={dependente.id} className="card mb-3 border">
                                        <div className="card-header d-flex justify-content-between align-items-center">
                                            <h6 className="mb-0">Dependente {index + 1}</h6>
                                            <button
                                                type="button"
                                                className="btn btn-sm btn-outline-danger"
                                                onClick={() => removerDependente(dependente.id)}
                                            >
                                                Remover
                                            </button>
                                        </div>
                                        <div className="card-body">
                                            <div className="row">
                                                <div className="col-md-6 mb-3">
                                                    <input
                                                        type="text"
                                                        className="form-control"
                                                        placeholder="Nome completo"
                                                        value={dependente.nome}
                                                        onChange={(e) => atualizarDependente(dependente.id, 'nome', e.target.value)}
                                                    />
                                                </div>
                                                <div className="col-md-3 mb-3">
                                                    <input
                                                        type="text"
                                                        className="form-control"
                                                        placeholder="CPF"
                                                        value={dependente.cpf}
                                                        onChange={(e) => atualizarDependente(dependente.id, 'cpf', formatCPF(e.target.value))}
                                                        maxLength="14"
                                                    />
                                                </div>
                                                <div className="col-md-3 mb-3">
                                                    <input
                                                        type="text"
                                                        className="form-control"
                                                        placeholder="Grau de parentesco"
                                                        value={dependente.grauParentesco}
                                                        onChange={(e) => atualizarDependente(dependente.id, 'grauParentesco', e.target.value)}
                                                    />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                ))}

                                <div className="text-center mb-4">
                                    <button
                                        type="button"
                                        className="btn btn-outline-primary"
                                        onClick={adicionarDependente}
                                    >
                                        <i className="bi bi-plus-circle me-2"></i>
                                        Adicionar Dependente
                                    </button>
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

export default Step3Dependentes;