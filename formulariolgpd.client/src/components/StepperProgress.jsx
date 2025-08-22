// src/components/StepperProgress.jsx
import React from 'react';

const StepperProgress = ({ currentStep, steps, qualificacao }) => {
    const getStepStatus = (stepIndex, currentStep) => {
        if (stepIndex < currentStep - 1) return 'completed';
        if (stepIndex === currentStep - 1) return 'active';
        return 'pending';
    };

    const getQualificacaoDisplay = (qual) => {
        const qualificacoes = {
            'titular': 'Fluxo: Titular',
            'dependenteMaior': 'Fluxo: Dependente Maior',
            'responsavelMenor': 'Fluxo: Responsável',
            'responsavelOrfao': 'Fluxo: Responsável',
            'curadorTutor': 'Fluxo: Curador/Tutor'
        };
        return qualificacoes[qual] || 'Fluxo: Padrão';
    };

    const getQualificacaoBadgeColor = (qual) => {
        const colors = {
            'titular': 'bg-primary',
            'dependenteMaior': 'bg-warning',
            'responsavelMenor': 'bg-info',
            'responsavelOrfao': 'bg-info',
            'curadorTutor': 'bg-secondary'
        };
        return colors[qual] || 'bg-light';
    };

    return (
        <div className="stepper-container">
            {qualificacao && (
                <div className="text-center mb-3">
                    <span className={`badge ${getQualificacaoBadgeColor(qualificacao)}`}>
                        {getQualificacaoDisplay(qualificacao)}
                    </span>
                </div>
            )}
            
            <div className="d-flex justify-content-center align-items-center flex-wrap">
                {steps.map((step, index) => (
                    <React.Fragment key={step.key}>
                        <div className="d-flex flex-column align-items-center mx-2">
                            <div className={`stepper-circle ${getStepStatus(index, currentStep)}`}>
                                {index < currentStep - 1 ? (
                                    <i className="bi bi-check"></i>
                                ) : (
                                    index + 1
                                )}
                            </div>
                            <small className="mt-1 text-center" style={{ maxWidth: '80px', fontSize: '0.7rem' }}>
                                {step.title}
                            </small>
                        </div>
                        {index < steps.length - 1 && (
                            <div className={`stepper-line ${index < currentStep - 1 ? 'completed' : 'pending'}`}></div>
                        )}
                    </React.Fragment>
                ))}
            </div>
            
            <div className="text-center mt-3">
                <h6 className="mb-0">{steps[currentStep - 1]?.title}</h6>
                <small className="text-muted">
                    Passo {currentStep} de {steps.length}
                </small>
            </div>
        </div>
    );
};

export default StepperProgress;