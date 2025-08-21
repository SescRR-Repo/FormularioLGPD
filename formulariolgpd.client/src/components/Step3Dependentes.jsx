// src/components/Step3Dependentes.jsx
import React, { useState } from 'react';

const Step3Dependentes = ({ formData, onInputChange, onNext, onPrev }) => {
    const [dependentes, setDependentes] = useState(formData.dependentes || []);
    const [errors, setErrors] = useState({});

    const calcularIdade = (dataNascimento) => {
        if (!dataNascimento) return null;
        
        const hoje = new Date();
        const nascimento = new Date(dataNascimento);
        let idade = hoje.getFullYear() - nascimento.getFullYear();
        const mesAtual = hoje.getMonth();
        const mesNascimento = nascimento.getMonth();
        
        if (mesAtual < mesNascimento || (mesAtual === mesNascimento && hoje.getDate() < nascimento.getDate())) {
            idade--;
        }
        
        return idade;
    };

    const adicionarDependente = () => {
        const novoDependente = {
            id: Date.now(),
            nome: '',
            cpf: '',
            dataNascimento: '',
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
        
        // Limpar erros do dependente removido
        const newErrors = { ...errors };
        delete newErrors[id];
        setErrors(newErrors);
    };

    const atualizarDependente = (id, campo, valor) => {
        const novosDependentes = dependentes.map(dep => {
            if (dep.id === id) {
                const dependenteAtualizado = { ...dep, [campo]: valor };
                
                // Validar idade quando data de nascimento for atualizada
                if (campo === 'dataNascimento') {
                    const idade = calcularIdade(valor);
                    if (idade !== null && idade >= 18) {
                        setErrors(prev => ({
                            ...prev,
                            [id]: 'Este dependente é maior de idade. Para dependentes maiores de 18 anos, eles devem fazer seu próprio cadastro individualmente.'
                        }));
                    } else {
                        setErrors(prev => {
                            const newErrors = { ...prev };
                            delete newErrors[id];
                            return newErrors;
                        });
                    }
                }
                
                return dependenteAtualizado;
            }
            return dep;
        });
        
        setDependentes(novosDependentes);
        onInputChange('dependentes', novosDependentes);
    };

    const formatCPF = (cpf) => {
        const numbers = cpf.replace(/\D/g, '');
        return numbers.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    };

    const validateForm = () => {
        // Verificar se há erros de idade
        const hasErrors = Object.keys(errors).length > 0;
        
        if (hasErrors) {
            alert('Por favor, corrija os erros antes de continuar. Dependentes maiores de idade devem fazer cadastro individual.');
            return false;
        }

        return true;
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        if (validateForm()) {
            onNext();
        }
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

                            <div className="alert alert-info mb-4">
                                <i className="bi bi-info-circle me-2"></i>
                                <strong>Importante:</strong> Adicione apenas dependentes <strong>menores de 18 anos</strong> ou legalmente representados. 
                                Dependentes maiores de idade devem fazer seu próprio cadastro individual.
                            </div>

                            <form onSubmit={handleSubmit}>
                                {dependentes.map((dependente, index) => {
                                    const idade = calcularIdade(dependente.dataNascimento);
                                    const isError = errors[dependente.id];
                                    
                                    return (
                                        <div key={dependente.id} className={`card mb-3 border ${isError ? 'border-danger' : ''}`}>
                                            <div className="card-header d-flex justify-content-between align-items-center">
                                                <div>
                                                    <h6 className="mb-0">Dependente {index + 1}</h6>
                                                    {idade !== null && (
                                                        <small className={`text-${idade >= 18 ? 'danger' : 'success'}`}>
                                                            Idade: {idade} anos {idade >= 18 ? '⚠️ Maior de idade' : '✅ Menor de idade'}
                                                        </small>
                                                    )}
                                                </div>
                                                <button
                                                    type="button"
                                                    className="btn btn-sm btn-outline-danger"
                                                    onClick={() => removerDependente(dependente.id)}
                                                >
                                                    Remover
                                                </button>
                                            </div>
                                            <div className="card-body">
                                                {isError && (
                                                    <div className="alert alert-danger alert-sm mb-3">
                                                        <i className="bi bi-exclamation-triangle me-2"></i>
                                                        {isError}
                                                    </div>
                                                )}
                                                
                                                <div className="row">
                                                    <div className="col-md-6 mb-3">
                                                        <label className="form-label">Nome Completo</label>
                                                        <input
                                                            type="text"
                                                            className="form-control"
                                                            placeholder="Nome completo do dependente"
                                                            value={dependente.nome}
                                                            onChange={(e) => atualizarDependente(dependente.id, 'nome', e.target.value)}
                                                        />
                                                    </div>
                                                    
                                                    <div className="col-md-3 mb-3">
                                                        <label className="form-label">Data de Nascimento</label>
                                                        <input
                                                            type="date"
                                                            className={`form-control ${isError ? 'is-invalid' : ''}`}
                                                            value={dependente.dataNascimento}
                                                            onChange={(e) => atualizarDependente(dependente.id, 'dataNascimento', e.target.value)}
                                                            max={new Date().toISOString().split('T')[0]} // Não permite datas futuras
                                                        />
                                                    </div>
                                                    
                                                    <div className="col-md-3 mb-3">
                                                        <label className="form-label">CPF</label>
                                                        <input
                                                            type="text"
                                                            className="form-control"
                                                            placeholder="000.000.000-00"
                                                            value={dependente.cpf}
                                                            onChange={(e) => atualizarDependente(dependente.id, 'cpf', formatCPF(e.target.value))}
                                                            maxLength="14"
                                                        />
                                                    </div>
                                                    
                                                    <div className="col-md-12 mb-3">
                                                        <label className="form-label">Grau de Parentesco</label>
                                                        <select
                                                            className="form-select"
                                                            value={dependente.grauParentesco}
                                                            onChange={(e) => atualizarDependente(dependente.id, 'grauParentesco', e.target.value)}
                                                        >
                                                            <option value="">Selecione o grau de parentesco</option>
                                                            <option value="filho">Filho(a)</option>
                                                            <option value="enteado">Enteado(a)</option>
                                                            <option value="neto">Neto(a)</option>
                                                            <option value="sobrinho">Sobrinho(a)</option>
                                                            <option value="irmao">Irmão/Irmã</option>
                                                            <option value="tutelado">Tutelado(a)</option>
                                                            <option value="outro">Outro</option>
                                                        </select>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}

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

                                {dependentes.length === 0 && (
                                    <div className="text-center mb-4">
                                        <div className="alert alert-light">
                                            <i className="bi bi-info-circle me-2"></i>
                                            Nenhum dependente adicionado. Você pode pular esta etapa se não possuir dependentes menores de 18 anos.
                                        </div>
                                    </div>
                                )}

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