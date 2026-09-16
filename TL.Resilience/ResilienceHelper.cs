using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TL.Resilience
{
    /// <summary>
    /// Fornece métodos utilitários de resiliência corporativa com Polly v8 para execução de operações assíncronas com retentativas, backoff exponencial e jitter.
    /// </summary>
    public static class ResilienceHelper
    {
        private const int DefaultMaxRetryAttempts = 3;
        private static readonly TimeSpan DefaultInitialDelay = TimeSpan.FromMilliseconds(200);

        /// <summary>
        /// Executa uma operação assíncrona com retorno aplicando política de retentativas com backoff exponencial e jitter decorrelacionado.
        /// </summary>
        /// <typeparam name="TResult">Tipo do resultado retornado pela operação.</typeparam>
        /// <param name="operation">Operação assíncrona a ser executada com suporte a cancelamento.</param>
        /// <param name="maxRetryAttempts">Número máximo de tentativas de repetição em caso de falha transitória.</param>
        /// <param name="initialDelay">Intervalo de atraso inicial antes da primeira repetição.</param>
        /// <param name="logger">Instância opcional de logger estruturado para registrar eventos de retentativa.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        /// <returns>Resultado da execução da operação.</returns>
        public static async Task<TResult> ExecuteWithRetryAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            int maxRetryAttempts = DefaultMaxRetryAttempts,
            TimeSpan? initialDelay = null,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            var pipeline = BuildRetryPipeline<TResult>(maxRetryAttempts, initialDelay ?? DefaultInitialDelay, logger);

            return await pipeline.ExecuteAsync(async ct => await operation(ct), cancellationToken);
        }

        /// <summary>
        /// Executa uma operação assíncrona sem retorno aplicando política de retentativas com backoff exponencial e jitter decorrelacionado.
        /// </summary>
        /// <param name="operation">Operação assíncrona a ser executada com suporte a cancelamento.</param>
        /// <param name="maxRetryAttempts">Número máximo de tentativas de repetição em caso de falha transitória.</param>
        /// <param name="initialDelay">Intervalo de atraso inicial antes da primeira repetição.</param>
        /// <param name="logger">Instância opcional de logger estruturado para registrar eventos de retentativa.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        public static async Task ExecuteWithRetryAsync(
            Func<CancellationToken, Task> operation,
            int maxRetryAttempts = DefaultMaxRetryAttempts,
            TimeSpan? initialDelay = null,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            var pipeline = BuildRetryPipeline(maxRetryAttempts, initialDelay ?? DefaultInitialDelay, logger);

            await pipeline.ExecuteAsync(async ct => await operation(ct), cancellationToken);
        }

        private static ResiliencePipeline<TResult> BuildRetryPipeline<TResult>(
            int maxRetryAttempts,
            TimeSpan delay,
            ILogger? logger)
        {
            return new ResiliencePipelineBuilder<TResult>()
                .AddRetry(new RetryStrategyOptions<TResult>
                {
                    MaxRetryAttempts = maxRetryAttempts,
                    Delay = delay,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder<TResult>()
                        .Handle<Exception>(ex => ex is not OperationCanceledException),
                    OnRetry = args =>
                    {
                        LogStructuredRetry(logger, args.AttemptNumber, args.RetryDelay, args.Outcome.Exception);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        private static ResiliencePipeline BuildRetryPipeline(
            int maxRetryAttempts,
            TimeSpan delay,
            ILogger? logger)
        {
            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = maxRetryAttempts,
                    Delay = delay,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder()
                        .Handle<Exception>(ex => ex is not OperationCanceledException),
                    OnRetry = args =>
                    {
                        LogStructuredRetry(logger, args.AttemptNumber, args.RetryDelay, args.Outcome.Exception);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        private static void LogStructuredRetry(ILogger? logger, int attemptNumber, TimeSpan delay, Exception? exception)
        {
            if (logger is null)
            {
                return;
            }

            logger.LogWarning(
                exception,
                "Tentativa de retentativa #{AttemptNumber} acionada. Próxima execução em {DelayMs} ms devido a falha transitória: {ErrorMessage}",
                attemptNumber,
                delay.TotalMilliseconds,
                exception?.Message ?? "Resultado de execução inválido");
        }
    }
}
