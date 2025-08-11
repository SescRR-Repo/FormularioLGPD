// src/App.jsx
import React, { useState } from 'react';
import StepperProgress from './components/StepperProgress';
import Step1TipoCadastro from './components/Step1TipoCadastro';
import Step2DadosPessoais from './components/Step2DadosPessoais';
import Step3Dependentes from './components/Step3Dependentes';
import Step4Consentimentos from './components/Step4Consentimentos';
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
        serieSemestre: '',
        telefone: '',
        email: '',

        // Step 3
        dependentes: [],

        // Step 4
        consentimentos: {}
    });

    const handleInputChange = (field, value) => {
        setFormData(prev => ({
            ...prev,
            [field]: value
        }));
    };

    const nextStep = () => {
        setCurrentStep(prev => Math.min(prev + 1, 4));
    };

    const prevStep = () => {
        setCurrentStep(prev => Math.max(prev - 1, 1));
    };

    const handleSubmit = async () => {
        setLoading(true);
        try {
            const resultado = await ApiService.criarTermoAceite(formData);
            setTermoGerado(resultado);
            setCurrentStep(5); // Página de sucesso
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
            serieSemestre: '',
            telefone: '',
            email: '',
            dependentes: [],
            consentimentos: {}
        });
        setCurrentStep(1);
        setTermoGerado(null);
    };

    const renderStep = () => {
        switch (currentStep) {
            case 1:
                return (
                    <Step1TipoCadastro
                        formData={formData}
                        onInputChange={handleInputChange}
                        onNext={nextStep}
                    />
                );
            case 2:
                return (
                    <Step2DadosPessoais
                        formData={formData}
                        onInputChange={handleInputChange}
                        onNext={nextStep}
                        onPrev={prevStep}
                    />
                );
            case 3:
                return (
                    <Step3Dependentes
                        formData={formData}
                        onInputChange={handleInputChange}
                        onNext={nextStep}
                        onPrev={prevStep}
                    />
                );
            case 4:
                return (
                    <Step4Consentimentos
                        formData={formData}
                        onInputChange={handleInputChange}
                        onSubmit={handleSubmit}
                        onPrev={prevStep}
                        loading={loading}
                    />
                );
            case 5:
                return (
                    <DocumentoGerado
                        termoData={termoGerado}
                        onVoltar={() => setCurrentStep(4)}
                        onDownload={handleDownload}
                        onNovoTermo={handleNovoTermo}
                    />
                );
            default:
                return null;
        }
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

            {/* Progress Stepper */}
            {currentStep <= 4 && (
                <div className="bg-light py-3">
                    <div className="container">
                        <StepperProgress currentStep={currentStep} />
                    </div>
                </div>
            )}

            {/* Main Content */}
            <div className="container py-4">
                {renderStep()}
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