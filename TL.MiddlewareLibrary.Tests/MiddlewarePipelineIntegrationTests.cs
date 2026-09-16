using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TL.MiddlewareLibrary.Models;
using Xunit;

namespace TL.MiddlewareLibrary.Tests
{
    public class MiddlewarePipelineIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public MiddlewarePipelineIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Pipeline_WhenRequestContainsCorrelationId_ShouldPropagateCorrelationIdHeader()
        {
            var client = CreateIsolatedClient();
            var expectedCorrelationId = $"corr-test-{Guid.NewGuid():N}";

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/middlewares/timing");
            request.Headers.Add("X-Correlation-Id", expectedCorrelationId);

            using var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Headers.Contains("X-Correlation-Id").Should().BeTrue();
            response.Headers.GetValues("X-Correlation-Id").First().Should().Be(expectedCorrelationId);
        }

        [Fact]
        public async Task Pipeline_WhenExecutingValidRequest_ShouldIncludeResponseTimeHeader()
        {
            var client = CreateIsolatedClient();

            using var response = await client.GetAsync("/api/middlewares/timing");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Headers.Contains("X-Response-Time-Ms").Should().BeTrue();

            var headerValue = response.Headers.GetValues("X-Response-Time-Ms").First();
            long.TryParse(headerValue, out long elapsedMs).Should().BeTrue();
            elapsedMs.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task Pipeline_WhenUnhandledExceptionThrown_ShouldReturnProblemDetails500WithTraceId()
        {
            var client = CreateIsolatedClient();
            var customTraceId = $"fatal-error-{Guid.NewGuid():N}";

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/middlewares/unhandled-crash");
            request.Headers.Add("X-Correlation-Id", customTraceId);

            using var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
            response.Headers.Contains("X-Correlation-Id").Should().BeTrue();
            response.Headers.GetValues("X-Correlation-Id").First().Should().Be(customTraceId);

            var jsonContent = await response.Content.ReadAsStringAsync();
            var problem = JsonSerializer.Deserialize<ProblemDetailsResponse>(jsonContent);

            problem.Should().NotBeNull();
            problem!.Status.Should().Be(500);
            problem.Title.Should().Be("Internal Server Error");
            problem.TraceId.Should().Be(customTraceId);
            problem.Instance.Should().Be("/api/middlewares/unhandled-crash");
        }

        [Fact]
        public async Task Pipeline_WhenValidationExceptionThrown_ShouldReturnProblemDetails400WithErrorsMap()
        {
            var client = CreateIsolatedClient();

            using var response = await client.GetAsync("/api/middlewares/validation-error");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

            var jsonContent = await response.Content.ReadAsStringAsync();
            var problem = JsonSerializer.Deserialize<ProblemDetailsResponse>(jsonContent);

            problem.Should().NotBeNull();
            problem!.Status.Should().Be(400);
            problem.Title.Should().Be("Bad Request");
            problem.Code.Should().Be("Customer.InvalidData");
            problem.Errors.Should().NotBeNull();
            problem.Errors!.Should().ContainKey("Documento");
            problem.Errors!.Should().ContainKey("LimiteCredito");
        }

        [Fact]
        public async Task Pipeline_UnderConcurrentLoad_ShouldSafelyReuseArrayPoolBuffersWithoutCorruption()
        {
            const int concurrentRequests = 50;

            var tasks = Enumerable.Range(1, concurrentRequests).Select(async index =>
            {
                var category = $"category-{index % 5}";
                var client = CreateIsolatedClient($"192.168.100.{index}");

                using var response = await client.GetAsync($"/api/middlewares/cache?category={category}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var body = await response.Content.ReadAsStringAsync();
                body.Should().NotBeNullOrWhiteSpace();
                body.Should().Contain(category);

                return response.Headers.Contains("X-Cache");
            });

            var results = await Task.WhenAll(tasks);

            results.Should().HaveCount(concurrentRequests);
            results.All(headerFound => headerFound).Should().BeTrue();
        }

        private HttpClient CreateIsolatedClient(string? customIp = null)
        {
            var client = _factory.CreateClient();
            var ip = customIp ?? $"10.0.{Random.Shared.Next(1, 200)}.{Random.Shared.Next(1, 250)}";
            client.DefaultRequestHeaders.Add("X-Forwarded-For", ip);
            return client;
        }
    }
}
