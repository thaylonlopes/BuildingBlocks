using System;

namespace CommonHelpers.RequestResponse
{
    /// <summary>
    /// Categorização semântica de erros para simplificar o mapeamento em status codes HTTP e logs.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// Erro genérico ou inesperado no processamento (mapeado tipicamente para 500 Internal Server Error).
        /// </summary>
        Failure = 0,

        /// <summary>
        /// Erro de validação de dados de entrada ou parâmetros (mapeado tipicamente para 400 Bad Request).
        /// </summary>
        Validation = 1,

        /// <summary>
        /// Recurso solicitado não foi encontrado (mapeado tipicamente para 404 Not Found).
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// Conflito de estado ou duplicidade de recurso (mapeado tipicamente para 409 Conflict).
        /// </summary>
        Conflict = 3,

        /// <summary>
        /// Falha de autenticação ou credenciais inválidas (mapeado tipicamente para 401 Unauthorized).
        /// </summary>
        Unauthorized = 4,

        /// <summary>
        /// Acesso negado por falta de permissão ou privilégios (mapeado tipicamente para 403 Forbidden).
        /// </summary>
        Forbidden = 5
    }

    /// <summary>
    /// Representa um erro tipado de negócio ou infraestrutura de forma imutável e expressiva.
    /// </summary>
    /// <remarks>
    /// Substitui o uso de exceções para fluxo de controle, fornecendo código de erro estável, mensagem clara e categoria semântica.
    /// </remarks>
    public record Error
    {
        /// <summary>
        /// Representa a ausência de erro (operação com sucesso).
        /// </summary>
        public static readonly Error None = new Error(string.Empty, string.Empty, ErrorType.Failure);

        /// <summary>
        /// Código textual padronizado do erro (ex: "User.NotFound", "Order.InvalidAmount").
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Descrição legível e humanizada do motivo da falha.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Categoria semântica do erro para roteamento HTTP e observabilidade.
        /// </summary>
        public ErrorType Type { get; }

        /// <summary>
        /// Inicializa uma nova instância de <see cref="Error"/>.
        /// </summary>
        /// <param name="code">Código único do erro.</param>
        /// <param name="message">Mensagem humanizada explicando o erro.</param>
        /// <param name="type">Categoria semântica do erro.</param>
        public Error(string code, string message, ErrorType type = ErrorType.Failure)
        {
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            Type = type;
        }

        /// <summary>
        /// Cria um erro de falha genérica.
        /// </summary>
        public static Error Failure(string code, string message) => new Error(code, message, ErrorType.Failure);

        /// <summary>
        /// Cria um erro de validação de entrada (400).
        /// </summary>
        public static Error Validation(string code, string message) => new Error(code, message, ErrorType.Validation);

        /// <summary>
        /// Cria um erro de recurso não encontrado (404).
        /// </summary>
        public static Error NotFound(string code, string message) => new Error(code, message, ErrorType.NotFound);

        /// <summary>
        /// Cria um erro de conflito ou duplicidade (409).
        /// </summary>
        public static Error Conflict(string code, string message) => new Error(code, message, ErrorType.Conflict);

        /// <summary>
        /// Cria um erro de não autenticado (401).
        /// </summary>
        public static Error Unauthorized(string code, string message) => new Error(code, message, ErrorType.Unauthorized);

        /// <summary>
        /// Cria um erro de acesso proibido (403).
        /// </summary>
        public static Error Forbidden(string code, string message) => new Error(code, message, ErrorType.Forbidden);

        /// <summary>
        /// Conversão implícita de string para um <see cref="Error"/> de validação.
        /// </summary>
        public static implicit operator Error(string message) => Validation("General.Validation", message);
    }
}

