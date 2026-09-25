using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using TL.HealthCheck.Lightweight;
using Xunit;

namespace TL.HealthCheck.Tests;

public class LightweightHealthCheckTests
{
    [Fact]
    public async Task Given_Healthy_Report_Should_Write_Status_200_And_Valid_Json()
    {
        var httpContext = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        httpContext.Response.Body = responseBodyStream;

        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["database"] = new(
                HealthStatus.Healthy,
                "Conexao OK",
                TimeSpan.FromMilliseconds(5),
                null,
                null,
                new[] { "ready" })
        };
        var report = new HealthReport(entries, TimeSpan.FromMilliseconds(10));

        await LightweightHealthCheckResponseWriter.WriteResponseAsync(httpContext, report);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");

        responseBodyStream.Seek(0, SeekOrigin.Begin);
        using var jsonDocument = await JsonDocument.ParseAsync(responseBodyStream);
        var root = jsonDocument.RootElement;

        root.GetProperty("status").GetString().Should().Be("Healthy");
        root.GetProperty("entries").GetProperty("database").GetProperty("status").GetString().Should().Be("Healthy");
        root.GetProperty("entries").GetProperty("database").GetProperty("description").GetString().Should().Be("Conexao OK");
    }

    [Fact]
    public async Task Given_Unhealthy_Report_Should_Write_Status_503()
    {
        var httpContext = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        httpContext.Response.Body = responseBodyStream;

        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["external_service"] = new(
                HealthStatus.Unhealthy,
                "Servico indisponivel",
                TimeSpan.FromMilliseconds(120),
                new InvalidOperationException("Falha de rede"),
                null,
                new[] { "live" })
        };
        var report = new HealthReport(entries, TimeSpan.FromMilliseconds(150));

        await LightweightHealthCheckResponseWriter.WriteResponseAsync(httpContext, report);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);

        responseBodyStream.Seek(0, SeekOrigin.Begin);
        using var jsonDocument = await JsonDocument.ParseAsync(responseBodyStream);
        jsonDocument.RootElement.GetProperty("status").GetString().Should().Be("Unhealthy");
    }

    [Fact]
    public void Given_Services_When_AddLightweightHealthChecks_Should_Register_HealthChecks()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddLightweightHealthChecks();

        var serviceProvider = services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetService<HealthCheckService>();

        healthCheckService.Should().NotBeNull();
    }
}
