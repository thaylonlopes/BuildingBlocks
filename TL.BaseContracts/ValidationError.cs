using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TL.BaseContracts
{
    /// <summary>
    /// Representa um erro especializado de validação que transporta falhas detalhadas agrupadas por campo/propriedade.
    /// </summary>
    /// <remarks>
    /// Integra-se nativamente com bibliotecas como FluentValidation, DataAnnotations e com o formato RFC 7807 (ProblemDetails).
    /// </remarks>
    /// <example>
    /// <code>
    /// var failures = new Dictionary&lt;string, string[]&gt;
    /// {
    ///     { "Email", new[] { "E-mail é obrigatório.", "Formato inválido." } },
    ///     { "Password", new[] { "Senha deve ter no mínimo 6 dígitos." } }
    /// };
    /// var error = ValidationError.FromFailures(failures);
    /// </code>
    /// </example>
    public record ValidationError : Error
    {
        private static readonly IReadOnlyDictionary<string, string[]> EmptyDictionary =
            new ReadOnlyDictionary<string, string[]>(new Dictionary<string, string[]>());

        /// <summary>
        /// Dicionário imutável contendo o nome do campo como chave e o array de mensagens de erro como valor.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        /// <summary>
        /// Inicializa um erro de validação a partir de um dicionário de falhas agrupadas por campo.
        /// </summary>
        /// <param name="errors">Dicionário contendo os erros de cada propriedade.</param>
        /// <param name="message">Mensagem descritiva geral (padrão: "Ocorreram um ou mais erros de validação.").</param>
        /// <param name="code">Código de erro padronizado (padrão: "Validation.General").</param>
        public ValidationError(
            IDictionary<string, string[]>? errors,
            string message = "Ocorreram um ou mais erros de validação.",
            string code = "Validation.General")
            : base(code, message, ErrorType.Validation)
        {
            Errors = errors != null
                ? new ReadOnlyDictionary<string, string[]>(errors.ToDictionary(k => k.Key, v => v.Value))
                : EmptyDictionary;
        }

        /// <summary>
        /// Cria uma nova instância de <see cref="ValidationError"/> a partir de um dicionário de falhas.
        /// </summary>
        /// <param name="errors">Dicionário de erros por campo.</param>
        /// <param name="message">Mensagem geral descritiva.</param>
        /// <param name="code">Código identificador do erro.</param>
        /// <returns>Uma nova instância de <see cref="ValidationError"/>.</returns>
        public static ValidationError FromFailures(
            IDictionary<string, string[]>? errors,
            string message = "Ocorreram um ou mais erros de validação.",
            string code = "Validation.General")
        {
            return new ValidationError(errors, message, code);
        }

        /// <summary>
        /// Cria um <see cref="ValidationError"/> para um único campo específico.
        /// </summary>
        /// <param name="field">Nome do campo com falha.</param>
        /// <param name="errorMessage">Mensagem de erro do campo.</param>
        /// <param name="code">Código do erro.</param>
        /// <returns>Uma nova instância de <see cref="ValidationError"/> contendo a falha do campo.</returns>
        public static ValidationError ForField(
            string field,
            string errorMessage,
            string code = "Validation.Field")
        {
            if (string.IsNullOrWhiteSpace(field)) throw new ArgumentNullException(nameof(field));
            if (string.IsNullOrWhiteSpace(errorMessage)) throw new ArgumentNullException(nameof(errorMessage));

            var dict = new Dictionary<string, string[]>
            {
                { field, new[] { errorMessage } }
            };

            return new ValidationError(dict, $"Erro de validação no campo '{field}'.", code);
        }
    }
}

