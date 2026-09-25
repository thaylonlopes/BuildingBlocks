using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TL.HealthCheck.Lightweight;

/// <summary>
/// Serializador de respostas de Health Check baseado em Utf8JsonWriter nativo de alta performance e compatível com Native AOT.
/// </summary>
public static class LightweightHealthCheckResponseWriter
{
    private static readonly JsonWriterOptions WriterOptions = new JsonWriterOptions
    {
        Indented = false
    };

    /// <summary>
    /// Escreve o relatório de saúde no corpo da resposta HTTP em formato JSON nativo padronizado.
    /// </summary>
    /// <param name="context">Contexto HTTP da requisição atual.</param>
    /// <param name="report">Relatório de integridade gerado pelos executores de health check.</param>
    /// <returns>Task assíncrona representando a conclusão da escrita.</returns>
    public static async Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(report);

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = report.Status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;

        await using var writer = new Utf8JsonWriter(context.Response.Body, WriterOptions);

        writer.WriteStartObject();
        writer.WriteString("status", report.Status.ToString());
        writer.WriteString("totalDuration", report.TotalDuration.ToString());

        writer.WriteStartObject("entries");
        foreach (var entry in report.Entries)
        {
            writer.WriteStartObject(entry.Key);
            writer.WriteString("status", entry.Value.Status.ToString());
            writer.WriteString("description", entry.Value.Description);
            writer.WriteString("duration", entry.Value.Duration.ToString());

            writer.WriteStartArray("tags");
            foreach (var tag in entry.Value.Tags)
            {
                writer.WriteStringValue(tag);
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
        writer.WriteEndObject();

        writer.WriteEndObject();

        await writer.FlushAsync(context.RequestAborted);
    }
}
