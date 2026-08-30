using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CommonHelpers.RequestResponse
{
    /// <summary>
    /// Envelope de transporte padronizado para respostas de operações em microsserviços.
    /// </summary>
    /// <typeparam name="T">O tipo do payload encapsulado na resposta.</typeparam>
    /// <remarks>
    /// Garante integridade de estado: a propriedade <see cref="IsSuccess"/> é estritamente <c>false</c>
    /// sempre que houver qualquer erro registrado em <see cref="ErrorMessages"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var response = new Response&lt;UserDto&gt;(user);
    /// response.AddMessage("Usuário autenticado com sucesso.");
    /// </code>
    /// </example>
    public class Response<T> where T : notnull
    {
        /// <summary>
        /// Indica se a operação foi concluída com sucesso e sem mensagens de erro.
        /// </summary>
        public bool IsSuccess { get; protected set; }

        private readonly List<string> _messages = new List<string>();
        private readonly List<string> _errorMessages = new List<string>();

        /// <summary>
        /// Payload de dados armazenado internamente.
        /// </summary>
        protected T _response = default!;

        /// <summary>
        /// Inicializa uma nova resposta definindo explicitamente seu status inicial de sucesso.
        /// </summary>
        /// <param name="isSuccess">Status inicial da resposta.</param>
        public Response(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        /// <summary>
        /// Inicializa uma resposta bem-sucedida contendo o payload de dados informado.
        /// </summary>
        /// <param name="response">Payload não nulo retornado pela operação.</param>
        /// <exception cref="ArgumentNullException">Lançada caso <paramref name="response"/> seja nulo.</exception>
        public Response(T response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response), "O payload de resposta não pode ser nulo.");
            IsSuccess = true;
            AddResponse(response);
        }

        /// <summary>
        /// Adiciona uma mensagem informativa à resposta (não altera o status de sucesso).
        /// </summary>
        /// <param name="message">Texto da mensagem a ser registrada.</param>
        public void AddMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _messages.Add(message);
            }
        }

        /// <summary>
        /// Adiciona uma mensagem de erro à resposta e automaticamente define <see cref="IsSuccess"/> como <c>false</c>.
        /// </summary>
        /// <param name="message">Descrição do erro ocorrido.</param>
        public void AddErrorMessage(string message) => AddErrorMessages(message);

        /// <summary>
        /// Define o payload de resposta associado.
        /// </summary>
        /// <param name="response">Payload não nulo.</param>
        /// <exception cref="ArgumentNullException">Lançada caso <paramref name="response"/> seja nulo.</exception>
        public void AddResponse(T response)
        {
            _response = response ?? throw new ArgumentNullException(nameof(response));
        }

        /// <summary>
        /// Obtém o payload encapsulado na resposta.
        /// </summary>
        /// <returns>O objeto de resposta <typeparamref name="T"/>.</returns>
        public T GetResponse() => _response;

        /// <summary>
        /// Coleção somente-leitura de mensagens de erro associadas à resposta.
        /// </summary>
        public IReadOnlyCollection<string> ErrorMessages => GetMessages(_errorMessages);

        /// <summary>
        /// Coleção somente-leitura de mensagens informativas da resposta.
        /// </summary>
        public IReadOnlyCollection<string> Messages => GetMessages(_messages);

        private static IReadOnlyCollection<string> GetMessages(List<string> messages) => messages.AsReadOnly();

        /// <summary>
        /// Atualiza o status de sucesso com base na presença de mensagens de erro.
        /// </summary>
        protected virtual void VerifySuccess()
        {
            IsSuccess = _errorMessages == null || _errorMessages.Count == 0;
        }

        private void AddErrorMessages(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _errorMessages.Add(message);
            }
            VerifySuccess();
        }
    }

    /// <summary>
    /// Envelope de transporte não-genérico baseado em <see cref="object"/>.
    /// </summary>
    public class Response : Response<object>
    {
        /// <summary>
        /// Inicializa a resposta com um payload de tipo <see cref="object"/>.
        /// </summary>
        public Response(object r) : base(r) { }

        /// <summary>
        /// Inicializa a resposta com um status booleano.
        /// </summary>
        public Response(bool isSuccess) : base(isSuccess) { }
    }
}
