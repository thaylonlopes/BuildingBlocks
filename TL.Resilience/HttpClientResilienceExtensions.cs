using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TL.Resilience
{
    /// <summary>
    /// Métodos de extensão para configuração de resiliência padronizada com Polly v8 em instâncias de <see cref="IHttpClientBuilder"/>.
    /// </summary>
    public static class HttpClientResilienceExtensions
    {
        /// <summary>
        /// Adiciona a estratégia de resiliência padronizada ao cliente HTTP configurando retentativas para erros 5xx/408, circuit breaker e timeout.
        /// </summary>
        /// <param name="builder">Construtor do cliente HTTP configurado no contêiner de injeção de dependência.</param>
        /// <returns>A mesma instância de <paramref name="builder"/> para encadeamento fluente.</returns>
        public static IHttpClientBuilder AddStandardResilience(this IHttpClientBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.AddTransient<StandardResilienceHandler>();
            return builder.AddHttpMessageHandler<StandardResilienceHandler>();
        }
    }

    /// <summary>
    /// Manipulador de mensagens HTTP delegante que executa cada requisição através de um pipeline de resiliência corporativo baseado em Polly v8.
    /// </summary>
    public class StandardResilienceHandler : DelegatingHandler
    {
        private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="StandardResilienceHandler"/> com o pipeline padrão de resiliência.
        /// </summary>
        public StandardResilienceHandler()
            : this(BuildDefaultPipeline())
        {
        }

        /// <summary>
        /// Inicializa uma nova instância de <see cref="StandardResilienceHandler"/> com um pipeline customizado.
        /// </summary>
        /// <param name="pipeline">Pipeline de resiliência a ser aplicado nas requisições HTTP.</param>
        public StandardResilienceHandler(ResiliencePipeline<HttpResponseMessage> pipeline)
        {
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }

        /// <summary>
        /// Envia a requisição HTTP aplicando as políticas de timeout, circuit breaker e retentativa configuradas no pipeline.
        /// </summary>
        /// <param name="request">Mensagem de requisição HTTP a ser despachada.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        /// <returns>Resposta HTTP obtida da execução resiliente.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await _pipeline.ExecuteAsync(
                async ct => await base.SendAsync(request, ct),
                cancellationToken);
        }

        private static ResiliencePipeline<HttpResponseMessage> BuildDefaultPipeline()
        {
            return new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(ConfigureRetryOptions())
                .AddCircuitBreaker(ConfigureCircuitBreakerOptions())
                .AddTimeout(new TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(30)
                })
                .Build();
        }

        private static RetryStrategyOptions<HttpResponseMessage> ConfigureRetryOptions()
        {
            return new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = CreateTransientFailurePredicate()
            };
        }

        private static CircuitBreakerStrategyOptions<HttpResponseMessage> ConfigureCircuitBreakerOptions()
        {
            return new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(15),
                ShouldHandle = CreateTransientFailurePredicate()
            };
        }

        private static PredicateBuilder<HttpResponseMessage> CreateTransientFailurePredicate()
        {
            return new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TimeoutRejectedException>()
                .HandleResult(IsTransientHttpFailure);
        }

        private static bool IsTransientHttpFailure(HttpResponseMessage response)
        {
            int statusCode = (int)response.StatusCode;
            return statusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout;
        }
    }
}
