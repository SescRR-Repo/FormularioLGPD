// src/components/Step1TipoCadastro.jsx
import React from 'react';

const Step1TipoCadastro = ({ formData, onInputChange, onNext }) => {
    const handleSubmit = (e) => {
        e.preventDefault();
        if (formData.tipoCadastro && formData.qualificacao) {
            onNext();
        }
    };

    return (
        <div className= "step-container" >
        <div className="row justify-content-center" >
            <div className="col-md-8" >
                <div className="card border-0 shadow-sm" >
                    <div className="card-body p-4" >
                        <div className="d-flex align-items-center mb-4" >
                            <div className="step-icon me-3" >
                                <i className="bi bi-clipboard-data fs-4 text-primary" > </i>
                                    < /div>
                                    < h4 className = "mb-0" > Tipo de Cadastro < /h4>
                                        < /div>

                                        < form onSubmit = { handleSubmit } >
                                            <div className="mb-4" >
                                                <label className="form-label fw-bold" > Selecione o tipo de cadastro: </label>

                                                    < div className = "mt-3" >
                                                    {
                                                        ['cadastro', 'renovacao', 'inclusao'].map((tipo) => (
                                                            <div key= { tipo } className = "form-check mb-2" >
                                                            <input
                          className="form-check-input"
                          type = "radio"
                          name = "tipoCadastro"
                          id = { tipo }
                          value = { tipo }
                          checked = { formData.tipoCadastro === tipo }
                          onChange = {(e) => onInputChange('tipoCadastro', e.target.value)}
/>
    < label className = "form-check-label" htmlFor = { tipo } >
        { tipo === 'cadastro' && 'Cadastro'}
{ tipo === 'renovacao' && 'Renovação' }
{ tipo === 'inclusao' && 'Inclusão de dependente' }
</label>
    < /div>
                    ))}
</div>
    < /div>

    < div className = "mb-4" >
        <label className="form-label fw-bold" > Na qualidade de: </label>

            < div className = "mt-3" >
                {
                    [
                    { value: 'titular', label: 'Titular' },
                    { value: 'dependenteMaior', label: 'Dependente maior de idade' },
                    { value: 'responsavelMenor', label: 'Responsável por menor trabalhador' },
                    { value: 'responsavelOrfao', label: 'Responsável por dependente órfão do titular' },
                    { value: 'curadorTutor', label: 'Curador, Tutor ou Guardião legal' }
                    ].map((item) => (
                        <div key= { item.value } className = "form-check mb-2" >
                        <input
                          className="form-check-input"
                          type = "radio"
                          name = "qualificacao"
                          id = { item.value }
                          value = { item.value }
                          checked = { formData.qualificacao === item.value }
                          onChange = {(e) => onInputChange('qualificacao', e.target.value)}
                />
                <label className="form-check-label" htmlFor = { item.value } >
                    { item.label }
                    < /label>
                    < /div>
                    ))}
</div>
    < /div>

    < div className = "d-flex justify-content-between" >
        <button type="button" className = "btn btn-outline-secondary" disabled >
            Voltar
            < /button>
            < button
type = "submit"
className = "btn btn-primary"
disabled = {!formData.tipoCadastro || !formData.qualificacao}
                  >
    Próximo < i className = "bi bi-arrow-right ms-1" > </i>
        < /button>
        < /div>
        < /form>
        < /div>
        < /div>
        < /div>
        < /div>
        < /div>
  );
};

export default Step1TipoCadastro;