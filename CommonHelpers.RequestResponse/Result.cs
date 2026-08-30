using System;

namespace CommonHelpers.RequestResponse
{
    /// <summary>
    /// Representa o resultado de uma operação sem retorno de valor, indicando sucesso ou falha com erro tipado.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Indica se a operação foi executada com sucesso.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Indica se a operação resultou em falha.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Objeto de erro tipado associado à falha (retorna <see cref="Error.None"/> se a operação foi bem-sucedida).
        /// </summary>
        public Error Error { get; }

        /// <summary>
        /// Inicializa o resultado com status de sucesso e sem erro.
        /// </summary>
        protected Result()
        {
            IsSuccess = true;
            Error = Error.None;
        }

        /// <summary>
        /// Inicializa o resultado indicando falha e associando o erro tipado correspondente.
        /// </summary>
        /// <param name="error">O erro ocorrido.</param>
        protected Result(Error error)
        {
            if (error == null || error == Error.None)
            {
                throw new ArgumentException("Um resultado com falha deve conter um erro válido e não nulo.", nameof(error));
            }
            IsSuccess = false;
            Error = error;
        }

        /// <summary>
        /// Cria um resultado bem-sucedido.
        /// </summary>
        public static Result Success() => new Result();

        /// <summary>
        /// Cria um resultado bem-sucedido com valor tipado de retorno.
        /// </summary>
        /// <typeparam name="TValue">Tipo do valor retornado.</typeparam>
        /// <param name="value">Valor da operação.</param>
        public static Result<TValue> Success<TValue>(TValue value) => new Result<TValue>(value);

        /// <summary>
        /// Cria um resultado com falha e erro tipado.
        /// </summary>
        /// <param name="error">Detalhes do erro.</param>
        public static Result Failure(Error error) => new Result(error);

        /// <summary>
        /// Cria um resultado com falha para um retorno tipado.
        /// </summary>
        /// <typeparam name="TValue">Tipo esperado em caso de sucesso.</typeparam>
        /// <param name="error">Detalhes do erro.</param>
        public static Result<TValue> Failure<TValue>(Error error) => new Result<TValue>(error);

        /// <summary>
        /// Executa uma das funções com base no sucesso ou na falha do resultado (Pattern Matching funcional).
        /// </summary>
        public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        {
            if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));

            return IsSuccess ? onSuccess() : onFailure(Error);
        }
    }

    /// <summary>
    /// Representa o resultado de uma operação que retorna um valor tipado <typeparamref name="TValue"/> em caso de sucesso.
    /// </summary>
    /// <typeparam name="TValue">Tipo do valor de sucesso.</typeparam>
    public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        /// <summary>
        /// Obtém o valor retornado pela operação bem-sucedida.
        /// </summary>
        /// <exception cref="InvalidOperationException">Lançada caso tente acessar o valor em um resultado com falha.</exception>
        public TValue Value
        {
            get
            {
                if (IsFailure)
                {
                    throw new InvalidOperationException($"Não é possível acessar o valor de um resultado com falha: '{Error.Message}'.");
                }
                return _value!;
            }
        }

        internal Result(TValue value) : base()
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "O valor de um resultado bem-sucedido não pode ser nulo.");
            }
            _value = value;
        }

        internal Result(Error error) : base(error)
        {
            _value = default;
        }

        /// <summary>
        /// Executa uma das funções com base no sucesso ou na falha do resultado (Pattern Matching funcional).
        /// </summary>
        public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure)
        {
            if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));

            return IsSuccess ? onSuccess(Value) : onFailure(Error);
        }

        /// <summary>
        /// Transforma o valor de sucesso aplicando uma função mapeadora.
        /// </summary>
        public Result<TResult> Map<TResult>(Func<TValue, TResult> mapper)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            if (IsFailure)
            {
                return Result.Failure<TResult>(Error);
            }

            return Result.Success(mapper(Value));
        }

        /// <summary>
        /// Conversão implícita de um valor <typeparamref name="TValue"/> para <see cref="Result{TValue}"/> com sucesso.
        /// </summary>
        public static implicit operator Result<TValue>(TValue value) => Result.Success(value);

        /// <summary>
        /// Conversão implícita de um <see cref="Error"/> para <see cref="Result{TValue}"/> com falha.
        /// </summary>
        public static implicit operator Result<TValue>(Error error) => Result.Failure<TValue>(error);
    }

    /// <summary>
    /// Métodos de extensão para interoperabilidade entre <see cref="Result{T}"/> e o envelope tradicional <see cref="Response{T}"/>.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Converte um <see cref="Result{T}"/> no formato tradicional <see cref="Response{T}"/>.
        /// </summary>
        public static Response<T> ToResponse<T>(this Result<T> result) where T : notnull
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            if (result.IsSuccess)
            {
                return new Response<T>(result.Value);
            }

            var response = new Response<T>(false);
            response.AddErrorMessage(result.Error.Message);
            return response;
        }

        /// <summary>
        /// Converte um <see cref="Response{T}"/> tradicional para o padrão moderno <see cref="Result{T}"/>.
        /// </summary>
        public static Result<T> ToResult<T>(this Response<T> response) where T : notnull
        {
            if (response == null) throw new ArgumentNullException(nameof(response));

            if (response.IsSuccess && response.GetResponse() != null)
            {
                return Result.Success(response.GetResponse());
            }

            var errorMessage = response.ErrorMessages != null && response.ErrorMessages.Count > 0
                ? string.Join("; ", response.ErrorMessages)
                : "Erro na operação.";

            return Result.Failure<T>(Error.Failure("Response.Error", errorMessage));
        }
    }
}

