export const validateCPF = (cpf) => {
    if (!cpf) return false;
    
    // Remove caracteres não numéricos
    cpf = cpf.replace(/\D/g, '');
    
    // Verifica se tem 11 dígitos
    if (cpf.length !== 11) return false;
    
    // Verifica se todos os dígitos são iguais
    if (/^(\d)\1{10}$/.test(cpf)) return false;
    
    // Calcula o primeiro dígito verificador
    let soma = 0;
    for (let i = 0; i < 9; i++) {
        soma += parseInt(cpf[i]) * (10 - i);
    }
    
    let resto = soma % 11;
    let primeiroDigito = resto < 2 ? 0 : 11 - resto;
    
    if (parseInt(cpf[9]) !== primeiroDigito) return false;
    
    // Calcula o segundo dígito verificador
    soma = 0;
    for (let i = 0; i < 10; i++) {
        soma += parseInt(cpf[i]) * (11 - i);
    }
    
    resto = soma % 11;
    let segundoDigito = resto < 2 ? 0 : 11 - resto;
    
    return parseInt(cpf[10]) === segundoDigito;
};

export const formatCPF = (cpf) => {
    if (!cpf) return cpf;
    
    const numbers = cpf.replace(/\D/g, '');
    
    if (numbers.length <= 11) {
        return numbers.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    }
    
    return cpf;
};

export const applyCPFMask = (value) => {
    // Remove tudo que não é dígito
    const numbers = value.replace(/\D/g, '');
    
    // Aplica a máscara progressivamente
    if (numbers.length <= 3) {
        return numbers;
    } else if (numbers.length <= 6) {
        return numbers.replace(/(\d{3})(\d+)/, '$1.$2');
    } else if (numbers.length <= 9) {
        return numbers.replace(/(\d{3})(\d{3})(\d+)/, '$1.$2.$3');
    } else {
        return numbers.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    }
};