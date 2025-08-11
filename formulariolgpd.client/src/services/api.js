// src/services/api.js
const API_BASE_URL = 'https://localhost:7102/api';

class ApiService {
    async criarTermoAceite(dadosFormulario) {
        const response = await fetch(`${API_BASE_URL}/TermoAceite`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                titular: {
                    nome: dadosFormulario.nome,
                    cpf: dadosFormulario.cpf?.replace(/\D/g, ''),
                    rg: dadosFormulario.rg,
                    dataNascimento: dadosFormulario.dataNascimento || '1990-01-01',
                    estadoCivil: dadosFormulario.estadoCivil,
                    naturalidade: dadosFormulario.naturalidade,
                    endereco: dadosFormulario.endereco,
                    telefone: dadosFormulario.telefone,
                    email: dadosFormulario.email,
                    escolaridade: dadosFormulario.escolaridade,
                    serieSemestre: dadosFormulario.serieSemestre,
                    qualificacaoLegal: this.mapQualificacao(dadosFormulario.qualificacao)
                },
                dependentes: (dadosFormulario.dependentes || []).map(dep => ({
                    nome: dep.nome,
                    cpf: dep.cpf?.replace(/\D/g, ''),
                    dataNascimento: '2010-01-01', // Assumindo menor de idade
                    grauParentesco: dep.grauParentesco
                })),
                aceiteConfirmado: true
            })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Erro ao processar termo');
        }

        return await response.json();
    }

    async downloadPdf(termoId) {
        const response = await fetch(`${API_BASE_URL}/TermoAceite/${termoId}/pdf`);

        if (!response.ok) {
            throw new Error('Erro ao baixar PDF');
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `termo_lgpd_${termoId}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
    }

    async validarCpf(cpf) {
        const cpfLimpo = cpf.replace(/\D/g, '');
        const response = await fetch(`${API_BASE_URL}/TermoAceite/validar-cpf/${cpfLimpo}`);

        if (!response.ok) {
            throw new Error('Erro ao validar CPF');
        }

        return await response.json();
    }

    mapQualificacao(qualificacao) {
        const map = {
            'titular': 1,
            'dependenteMaior': 2,
            'responsavelMenor': 3,
            'responsavelOrfao': 4,
            'curadorTutor': 5
        };
        return map[qualificacao] || 1;
    }
}

export default new ApiService();