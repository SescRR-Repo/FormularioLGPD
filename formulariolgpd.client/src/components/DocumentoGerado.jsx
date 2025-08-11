// src/components/DocumentoGerado.jsx
import React from 'react';

const DocumentoGerado = ({ termoData, onVoltar, onDownload, onNovoTermo }) => {
    return (
        <div className="documento-gerado-container">
            <div className="row justify-content-center">
                <div className="col-md-8">
                    <div className="card border-0 shadow-sm">
                        <div className="card-body p-4 text-center">
                            <div className="mb-4">
                                <i className="bi bi-check-circle-fill text-success" style={{ fontSize: '4rem' }}></i>
                            </div>

                            <h2 className="text-success mb-3">Documento Gerado</h2>

                            <div className="alert alert-success">
                                <h5>Termo de Consentimento processado com sucesso!</h5>
                                <p className="mb-0">
                                    <strong>Número do Termo:</strong> {termoData.numeroTermo}<br />
                                    <strong>Data/Hora:</strong> {new Date(termoData.dataHoraAceite).toLocaleString()}<br />
                                    <strong>Hash de Integridade:</strong> {termoData.hashIntegridade?.substring(0, 20)}...
                                </p>
                            </div>

                            <div className="d-flex justify-content-center gap-3 mt-4">
                                <button
                                    className="btn btn-outline-secondary"
                                    onClick={onVoltar}
                                >
                                    Voltar ao Formulário
                                </button>

                                <button
                                    className="btn btn-primary"
                                    onClick={onDownload}
                                >
                                    <i className="bi bi-download me-2"></i>
                                    Imprimir/Salvar PDF
                                </button>
                            </div>

                            <div className="mt-4">
                                <button
                                    className="btn btn-link"
                                    onClick={onNovoTermo}
                                >
                                    Preencher novo termo
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default DocumentoGerado;