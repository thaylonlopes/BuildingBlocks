using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TL.Resilience;
using Xunit;

namespace TL.Resilience.Tests
{
    public class ResilienceHelperTests
    {
        [Fact]
        public async Task ExecuteWithRetryAsync_WhenOperationSucceedsFirstTime_ShouldReturnResultImmediately()
        {
            int executionCount = 0;

            var result = await ResilienceHelper.ExecuteWithRetryAsync(ct =>
            {
                executionCount++;
                return Task.FromResult("resultado-sucesso");
            }, maxRetryAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(10));

            result.Should().Be("resultado-sucesso");
            executionCount.Should().Be(1);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_WhenTransientFailuresOccurBeforeMaxAttempts_ShouldRetryAndSucceed()
        {
            int executionCount = 0;

            var result = await ResilienceHelper.ExecuteWithRetryAsync(ct =>
            {
                executionCount++;
                if (executionCount < 3)
                {
                    throw new HttpRequestException("Falha de rede transitória simulada.");
                }

                return Task.FromResult(42);
            }, maxRetryAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(10));

            result.Should().Be(42);
            executionCount.Should().Be(3);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_WhenFailuresExceedMaxRetries_ShouldThrowOriginalException()
        {
            int executionCount = 0;

            Func<Task> action = async () =>
            {
                await ResilienceHelper.ExecuteWithRetryAsync<string>(ct =>
                {
                    executionCount++;
                    throw new InvalidOperationException("Falha persistente de banco.");
                }, maxRetryAttempts: 2, initialDelay: TimeSpan.FromMilliseconds(10));
            };

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Falha persistente de banco.");

            executionCount.Should().Be(3);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_WhenCancellationTokenIsCancelled_ShouldThrowOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            int executionCount = 0;

            Func<Task> action = async () =>
            {
                await ResilienceHelper.ExecuteWithRetryAsync(ct =>
                {
                    executionCount++;
                    ct.ThrowIfCancellationRequested();
                    return Task.FromResult(100);
                }, maxRetryAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(10), cancellationToken: cts.Token);
            };

            await action.Should().ThrowAsync<OperationCanceledException>();
            executionCount.Should().Be(0);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_VoidOverload_WhenTransientFailureOccurs_ShouldRetryAndComplete()
        {
            int executionCount = 0;

            await ResilienceHelper.ExecuteWithRetryAsync(ct =>
            {
                executionCount++;
                if (executionCount == 1)
                {
                    throw new TimeoutException("Timeout transitório");
                }

                return Task.CompletedTask;
            }, maxRetryAttempts: 2, initialDelay: TimeSpan.FromMilliseconds(10));

            executionCount.Should().Be(2);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_WithLogger_ShouldLogWarningOnRetry()
        {
            var loggerMock = new Mock<ILogger>();
            int executionCount = 0;

            await ResilienceHelper.ExecuteWithRetryAsync(ct =>
            {
                executionCount++;
                if (executionCount == 1)
                {
                    throw new ApplicationException("Erro transiente");
                }

                return Task.FromResult("ok");
            }, maxRetryAttempts: 2, initialDelay: TimeSpan.FromMilliseconds(10), logger: loggerMock.Object);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddStandardResilience_WhenConfiguredOnHttpClient_ShouldExecuteThroughPipeline()
        {
            var services = new ServiceCollection();
            services.AddHttpClient("TestClient")
                .AddStandardResilience()
                .ConfigurePrimaryHttpMessageHandler(() => new TestHttpMessageHandler(HttpStatusCode.OK, "{\"status\":\"ok\"}"));

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("TestClient");

            var response = await client.GetAsync("https://api.empresa.local/health");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("{\"status\":\"ok\"}");
        }

        private sealed class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _content;

            public TestHttpMessageHandler(HttpStatusCode statusCode, string content)
            {
                _statusCode = statusCode;
                _content = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_content)
                };

                return Task.FromResult(response);
            }
        }
    }
}
