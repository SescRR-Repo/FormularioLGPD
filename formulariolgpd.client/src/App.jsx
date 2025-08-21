// src/App.jsx
import React, { useState } from 'react';
import StepperProgress from './components/StepperProgress';
import Step1TipoCadastro from './components/Step1TipoCadastro';
import Step2DadosPessoais from './components/Step2DadosPessoais';
import Step2_5DadosTitular from './components/Step2_5DadosTitular';
import Step3Dependentes from './components/Step3Dependentes';
import Step4Consentimentos from './components/Step4Consentimentos';
import DocumentoPrevia from './components/DocumentoPrevia';
import DocumentoGerado from './components/DocumentoGerado';
import ApiService from './services/api';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';
import './App.css';

function App() {
    const [currentStep, setCurrentStep] = useState(1);
    const [loading, setLoading] = useState(false);
    const [termoGerado, setTermoGerado] = useState(null);
    const [formData, setFormData] = useState({
        // Step 1
        tipoCadastro: '',
        qualificacao: '',

        // Step 2
        nome: '',
        cpf: '',
        estadoCivil: '',
        naturalidade: '',
        escolaridade: '',
        situacao: '',
        telefone: '',
        email: '',

        // Step 2.5 (Para dependentes maiores)
        dadosTitular: null,

        // Step 3
        dependentes: [],

        // Step 4
        consentimentos: {}
    });

    // ===== LÓGICA DE FLUXOS CONDICIONAIS =====
    const getFlowSteps = (qualificacao) => {
        switch (qualificacao) {
            case 'titular':
                return [
                    { key: 'tipo', title: 'Tipo de Cadastro', component: Step1TipoCadastro },
                    { key: 'dados', title: 'Dados Pessoais', component: Step2DadosPessoais },
                    { key: 'dependentes', title: 'Dependentes', component: Step3Dependentes },
                    { key: 'consentimentos', title: 'Consentimentos', component: Step4Consentimentos },
                    { key: 'previa', title: 'Prévia do Documento', component: DocumentoPrevia }
                ];

            case 'dependenteMaior':
                return [
                    { key: 'tipo', title: 'Tipo de Cadastro', component: Step1TipoCadastro },
                    { key: 'dados', title: 'Seus Dados', component: Step2DadosPessoais },
                    { key: 'titular', title: 'Dados do Titular', component: Step2_5DadosTitular },
                    { key: 'consentimentos', title: 'Consentimentos', component: Step4Consentimentos },
                    { key: 'previa', title: 'Prévia do Documento', component: DocumentoPrevia }
                ];

            case 'responsavelMenor':
            case 'responsavelOrfao':
            case 'curadorTutor':
                return [
                    { key: 'tipo', title: 'Tipo de Cadastro', component: Step1TipoCadastro },
                    { key: 'dados', title: 'Dados Pessoais', component: Step2DadosPessoais },
                    { key: 'dependentes', title: 'Dependentes', component: Step3Dependentes },
                    { key: 'consentimentos', title: 'Consentimentos', component: Step4Consentimentos },
                    { key: 'previa', title: 'Prévia do Documento', component: DocumentoPrevia }
                ];

            default:
                return [
                    { key: 'tipo', title: 'Tipo de Cadastro', component: Step1TipoCadastro },
                    { key: 'dados', title: 'Dados Pessoais', component: Step2DadosPessoais },
                    { key: 'consentimentos', title: 'Consentimentos', component: Step4Consentimentos },
                    { key: 'previa', title: 'Prévia do Documento', component: DocumentoPrevia }
                ];
        }
    };

    // Obter steps baseado na qualificação atual
    const currentFlow = getFlowSteps(formData.qualificacao);
    const maxSteps = currentFlow.length;

    const handleInputChange = (field, value) => {
        setFormData(prev => {
            const newData = { ...prev, [field]: value };
            
            // Se mudou a qualificação, resetar o step para 1
            if (field === 'qualificacao' && value !== prev.qualificacao) {
                setCurrentStep(1);
            }
            
            return newData;
        });
    };

    const nextStep = () => {
        setCurrentStep(prev => Math.min(prev + 1, maxSteps));
    };

    const prevStep = () => {
        setCurrentStep(prev => Math.max(prev - 1, 1));
    };

    const handlePreviewConfirm = async () => {
        setLoading(true);
        try {
            const resultado = await ApiService.criarTermoAceite(formData);
            setTermoGerado(resultado);
            setCurrentStep(maxSteps + 1); // Ir para DocumentoGerado
        } catch (error) {
            alert('Erro ao processar termo: ' + error.message);
        } finally {
            setLoading(false);
        }
    };

    const handleDownload = async () => {
        if (termoGerado?.id) {
            try {
                await ApiService.downloadPdf(termoGerado.id);
            } catch (error) {
                alert('Erro ao baixar PDF: ' + error.message);
            }
        }
    };

    const handleNovoTermo = () => {
        setFormData({
            tipoCadastro: '',
            qualificacao: '',
            nome: '',
            cpf: '',
            estadoCivil: '',
            naturalidade: '',
            escolaridade: '',
            situacao: '',
            telefone: '',
            email: '',
            dadosTitular: null,
            dependentes: [],
            consentimentos: {}
        });
        setCurrentStep(1);
        setTermoGerado(null);
    };

    const renderCurrentStep = () => {
        // Se está na página final (DocumentoGerado)
        if (currentStep > maxSteps) {
            return (
                <DocumentoGerado
                    termoData={termoGerado}
                    onVoltar={() => setCurrentStep(maxSteps)}
                    onDownload={handleDownload}
                    onNovoTermo={handleNovoTermo}
                />
            );
        }

        // Renderizar step atual baseado no fluxo
        const currentStepConfig = currentFlow[currentStep - 1];
        if (!currentStepConfig) return null;

        const StepComponent = currentStepConfig.component;
        const commonProps = {
            formData,
            onInputChange: handleInputChange,
            onNext: nextStep,
            onPrev: prevStep
        };

        // Props específicas por componente
        if (StepComponent === Step4Consentimentos) {
            return (
                <StepComponent
                    {...commonProps}
                    onSubmit={nextStep} // Vai para prévia
                    loading={loading}
                />
            );
        }

        if (StepComponent === DocumentoPrevia) {
            return (
                <StepComponent
                    {...commonProps}
                    onConfirm={handlePreviewConfirm}
                    loading={loading}
                />
            );
        }

        return <StepComponent {...commonProps} />;
    };

    return (
        <div className="app-container">
            {/* Header */}
            <div className="bg-primary text-white py-4">
                <div className="container">
                    <h1 className="h3 mb-1">Termo LGPD - SESC Roraima</h1>
                    <p className="mb-0">Preencha os dados para gerar seu termo de consentimento digital</p>
                </div>
            </div>

            {/* Progress Stepper - Só mostrar se não estiver na página final */}
            {currentStep <= maxSteps && (
                <div className="bg-light py-3">
                    <div className="container">
                        <StepperProgress 
                            currentStep={currentStep} 
                            steps={currentFlow}
                            qualificacao={formData.qualificacao}
                        />
                    </div>
                </div>
            )}

            {/* Main Content */}
            <div className="container py-4">
                {renderCurrentStep()}
            </div>

            {/* Footer */}
            <footer className="bg-dark text-white text-center py-3 mt-5">
                <div className="container">
                    <small>
                        © 2025 SESC Roraima - Sistema de Termo de Consentimento LGPD
                    </small>
                </div>
            </footer>
        </div>
    );
}

export default App;