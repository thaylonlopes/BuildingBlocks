using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TL.BaseContracts;
using TL.MiddlewareLibrary.Exceptions;
using TL.MiddlewareLibrary.Extensions;
using TL.MiddlewareLibrary.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace TL.MiddlewareLibrary.Tests
{
    public class ProblemDetailsIntegrationTests
    {
        [Fact]
        public async Task Pipeline_WhenUnhandledExceptionOccurs_ShouldReturnRfc7807JsonWithCorrelationId()
        {
            using var host = await CreateTestHostAsync();
            var client = host.GetTestClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/test/crash");
            request.Headers.Add("X-Correlation-Id", "corr-test-pipeline-123");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.True(response.Headers.Contains("X-Correlation-Id"));

            var json = await response.Content.ReadAsStringAsync();
            var problem = JsonSerializer.Deserialize<ProblemDetailsResponse>(json);

            Assert.NotNull(problem);
            Assert.Equal(500, problem.Status);
            Assert.Equal("Internal Server Error", problem.Title);
            Assert.Equal("corr-test-pipeline-123", problem.TraceId);
            Assert.Equal("/test/crash", problem.Instance);
        }

        [Fact]
        public async Task Pipeline_WhenValidationExceptionThrown_ShouldReturn400WithErrorsMap()
        {
            using var host = await CreateTestHostAsync();
            var client = host.GetTestClient();

            var response = await client.GetAsync("/test/validation-error");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var json = await response.Content.ReadAsStringAsync();
            var problem = JsonSerializer.Deserialize<ProblemDetailsResponse>(json);

            Assert.NotNull(problem);
            Assert.Equal(400, problem.Status);
            Assert.Equal("Bad Request", problem.Title);
            Assert.Equal("Input.Invalid", problem.Code);
            Assert.NotNull(problem.Errors);
            Assert.True(problem.Errors.ContainsKey("Nome"));
        }

        private static async Task<IHost> CreateTestHostAsync()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddMemoryCache();
                    });
                    webHost.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseRequestTiming();
                        app.UseExceptionHandling();
                        app.UseStatusCodeMiddleware();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/test/crash", (Func<string>)(() => throw new ApplicationException("Falha forçada")));
                            endpoints.MapGet("/test/validation-error", (Func<string>)(() =>
                            {
                                var failures = new Dictionary<string, string[]>
                                {
                                    { "Nome", new[] { "Nome é obrigatório." } }
                                };
                                throw new ValidationException(ValidationError.FromFailures(failures, "Erro de validação", "Input.Invalid"));
                            }));
                        });
                    });
                });

            return await hostBuilder.StartAsync();
        }
    }
}
