// src/components/StepperProgress.jsx
import React from 'react';

const StepperProgress = ({ currentStep, totalSteps = 4 }) => {
    const steps = [
        { number: 1, title: 'Tipo de Cadastro', completed: currentStep > 1, active: currentStep === 1 },
        { number: 2, title: 'Dados Pessoais', completed: currentStep > 2, active: currentStep === 2 },
        { number: 3, title: 'Dependentes', completed: currentStep > 3, active: currentStep === 3 },
        { number: 4, title: 'Consentimentos', completed: currentStep > 4, active: currentStep === 4 }
    ];

    return (
        <div className="stepper-container mb-4">
            <div className="d-flex justify-content-between align-items-center">
                {steps.map((step, index) => (
                    <React.Fragment key={step.number}>
                        <div className="d-flex flex-column align-items-center">
                            <div
                                className={`stepper-circle ${step.completed ? 'completed' :
                                        step.active ? 'active' :
                                            'pending'
                                    }`}
                            >
                                {step.completed ? (
                                    <i className="bi bi-check"></i>
                                ) : (
                                    step.number
                                )}
                            </div>
                            <small className="mt-1 text-muted">{step.title}</small>
                        </div>
                        {index < steps.length - 1 && (
                            <div
                                className={`stepper-line ${currentStep > step.number ? 'completed' : 'pending'
                                    }`}
                            />
                        )}
                    </React.Fragment>
                ))}
            </div>
            <div className="mt-2">
                <small className="text-muted">
                    Etapa {currentStep} de {totalSteps}: {steps[currentStep - 1]?.title}
                </small>
            </div>
        </div>
    );
};

export default StepperProgress;