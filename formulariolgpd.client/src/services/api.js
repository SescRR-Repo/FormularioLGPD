// DETECÇÃO MELHORADA DE AMBIENTE
const getApiBaseUrl = () => {
    const hostname = window.location.hostname;
    const port = window.location.port;
    const protocol = window.location.protocol;
    
    // DESENVOLVIMENTO: Visual Studio
    if (port === '56200' && hostname === 'localhost') {
        return 'https://localhost:7102/api';
    }
    
    // DESENVOLVIMENTO: IIS Express 
    if (port === '44342' && hostname === 'localhost') {
        return 'https://localhost:44342/api';
    }
    
    // PRODUÇÃO/IIS: Mesma origem
    return `${protocol}//${hostname}${port ? `:${port}` : ''}/api`;
};

// Detectar se está em ambiente de desenvolvimento
const isDevelopment = () => {
    return window.location.hostname === 'localhost' || 
           window.location.hostname === '127.0.0.1' ||
           window.location.hostname.includes('dev') ||
           window.location.port === '56200'; // Porta do Vite dev
};

// Função de log condicional
const debugLog = (...args) => {
    if (isDevelopment()) {
        console.log(...args);
    }
};

const debugWarn = (...args) => {
    if (isDevelopment()) {
        console.warn(...args);
    }
};

const debugError = (...args) => {
    if (isDevelopment()) {
        console.error(...args);
    }
};

const API_BASE_URL = getApiBaseUrl();

class ApiService {
    constructor() {
        // Só testar conectividade em desenvolvimento
        if (isDevelopment()) {
            this.testConnection().then(isConnected => {
                if (isConnected) {
                    debugLog('✅ API conectada:', API_BASE_URL);
                } else {
                    debugWarn('⚠️ API não conectada. Tentativas alternativas...');
                    this.tryAlternativeUrls();
                }
            });
        }
    }

    async tryAlternativeUrls() {
        const alternativeUrls = [
            window.location.origin + '/api',
            window.location.protocol + '//' + window.location.hostname + '/api',
            'https://' + window.location.hostname + '/api',
            'http://' + window.location.hostname + '/api',
            'https://localhost:7102/api',
            'http://localhost:5230/api'
        ];

        debugLog('🔍 Testando URLs alternativas...');
        
        for (const url of alternativeUrls) {
            try {
                debugLog('🔍 Testando:', url);
                const response = await fetch(`${url}/TermoAceite/conteudo-termo`, {
                    method: 'GET',
                    mode: 'cors'
                });
                
                if (response.ok) {
                    debugLog('✅ URL funcionando encontrada:', url);
                    window.API_BASE_URL_OVERRIDE = url;
                    return url;
                }
            } catch (error) {
                debugLog('❌ Falhou:', url, error.message);
            }
        }
        
        debugError('💥 Nenhuma URL da API funcionando encontrada!');
        return null;
    }

    getCurrentApiUrl() {
        return window.API_BASE_URL_OVERRIDE || API_BASE_URL;
    }

    async criarTermoAceite(dadosFormulario) {
        try {
            const currentUrl = this.getCurrentApiUrl();
            
            // Log seguro - sem dados sensíveis
            debugLog('🚀 Processando termo...');
            
            const payload = {
                tipoCadastro: dadosFormulario.tipoCadastro, // ADICIONAR ESTA LINHA
                titular: {
                    nome: dadosFormulario.nome,
                    cpf: dadosFormulario.cpf?.replace(/\D/g, ''),
                    rg: dadosFormulario.rg || '',
                    dataNascimento: dadosFormulario.dataNascimento || '1990-01-01',
                    estadoCivil: dadosFormulario.estadoCivil,
                    naturalidade: dadosFormulario.naturalidade,
                    endereco: dadosFormulario.endereco || '',
                    telefone: dadosFormulario.telefone,
                    email: dadosFormulario.email,
                    escolaridade: dadosFormulario.escolaridade,
                    serieSemestre: dadosFormulario.situacao || dadosFormulario.serieSemestre || '',
                    qualificacaoLegal: this.mapQualificacao(dadosFormulario.qualificacao)
                },
                dependentes: (dadosFormulario.dependentes || []).map(dep => ({
                    nome: dep.nome,
                    cpf: dep.cpf?.replace(/\D/g, ''),
                    dataNascimento: dep.dataNascimento || '2010-01-01',
                    grauParentesco: dep.grauParentesco
                })),
                aceiteConfirmado: true
            };

            const response = await fetch(`${currentUrl}/TermoAceite`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(payload),
                mode: 'cors'
            });

            if (!response.ok) {
                const errorText = await response.text();
                
                let errorMessage = 'Erro ao processar termo';
                try {
                    const errorJson = JSON.parse(errorText);
                    errorMessage = errorJson.message || errorMessage;
                } catch {
                    errorMessage = 'Erro interno do servidor';
                }
                
                throw new Error(errorMessage);
            }

            const result = await response.json();
            debugLog('✅ Termo processado com sucesso');
            
            return result;
            
        } catch (error) {
            // Em produção, log mínimo
            if (isDevelopment()) {
                debugError('💥 Erro:', error.message);
            }
            
            // Tentar URLs alternativas só em desenvolvimento
            if (isDevelopment() && (error.message.includes('Failed to fetch') || error.message.includes('ERR_CONNECTION_REFUSED'))) {
                const newUrl = await this.tryAlternativeUrls();
                if (newUrl) {
                    return this.criarTermoAceite(dadosFormulario);
                }
            }
            
            throw error;
        }
    }

    async downloadPdf(termoId) {
        try {
            const currentUrl = this.getCurrentApiUrl();
            const response = await fetch(`${currentUrl}/TermoAceite/${termoId}/pdf`);

            if (!response.ok) {
                throw new Error('Erro ao baixar documento');
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `termo_lgpd_${termoId}.html`;
            a.click();
            window.URL.revokeObjectURL(url);
            
            debugLog('✅ Download iniciado para termo:', termoId);
        } catch (error) {
            debugError('Erro no download:', error.message);
            throw error;
        }
    }

    async validarCpf(cpf) {
        try {
            const currentUrl = this.getCurrentApiUrl();
            const cpfLimpo = cpf.replace(/\D/g, '');
            
            debugLog('🔍 Validando CPF:', `${cpfLimpo.substring(0, 3)}***`);
            
            const response = await fetch(`${currentUrl}/TermoAceite/validar-cpf/${cpfLimpo}`);

            if (!response.ok) {
                throw new Error('Erro ao validar CPF');
            }

            return await response.json();
        } catch (error) {
            debugError('Erro na validação CPF:', error.message);
            throw error;
        }
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

    async testConnection() {
        try {
            const currentUrl = this.getCurrentApiUrl();
            debugLog('🔍 Testando conexão com API...');
            
            const response = await fetch(`${currentUrl}/TermoAceite/conteudo-termo`, {
                method: 'GET',
                mode: 'cors'
            });
            
            return response.ok;
        } catch (error) {
            debugError('❌ Falha no teste de conexão:', error.message);
            return false;
        }
    }
}

export default new ApiService();