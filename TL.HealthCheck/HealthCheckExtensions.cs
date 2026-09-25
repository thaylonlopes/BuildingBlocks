using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using TL.HealthCheck.Config;
using TL.HealthCheck.Constants;
using TL.HealthCheck.Lightweight;

[assembly: InternalsVisibleTo("TL.HealthCheck.Tests")]

namespace TL.HealthCheck
{
    /// <summary>
    /// Extensões fluentes para configuração simplificada de Health Checks em ASP.NET Core e Worker Services.
    /// </summary>
    /// <remarks>
    /// Padroniza probes de orquestração (<c>/livez</c>, <c>/readyz</c> e legados) utilizando serialização nativa ultrarrápida com System.Text.Json.
    /// </remarks>
    public static class HealthCheckExtensions
    {
        private static readonly HealthCheckOptions _options = new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = LightweightHealthCheckResponseWriter.WriteResponseAsync
        };

        private static readonly HealthCheckOptions _optionsResponse = new HealthCheckOptions
        {
            Predicate = _ => true
        };

        /// <summary>
        /// Registra os Health Checks padrão no container de injeção de dependência com serializador leve System.Text.Json.
        /// </summary>
        /// <param name="services">A coleção de serviços da aplicação.</param>
        /// <param name="check">Função opcional para fornecer verificações personalizadas adicionais.</param>
        /// <returns>A coleção de serviços para encadeamento fluente.</returns>
        public static IServiceCollection AddLightweightHealthChecks(this IServiceCollection services, Func<CheckConfig[]>? check = default)
        {
            return services.AddRequiredHealthChecks(check);
        }

        /// <summary>
        /// Registra os Health Checks padrão no container de injeção de dependência.
        /// </summary>
        /// <param name="services">A coleção de serviços da aplicação.</param>
        /// <param name="check">Função opcional para fornecer verificações personalizadas adicionais.</param>
        /// <returns>A coleção de serviços para encadeamento fluente.</returns>
        public static IServiceCollection AddRequiredHealthChecks(this IServiceCollection services, Func<CheckConfig[]>? check = default)
        {
            var builder = services.AddHealthChecks();
            var checkConfigs = check?.Invoke();
            if (checkConfigs != null && checkConfigs.Length > 0)
            {
                builder.AddAllChecks(checkConfigs);
            }
            else
            {
                builder.AddAllChecks();
            }
            return services;
        }

        /// <summary>
        /// Adiciona uma lista de verificações de saúde personalizadas ao builder do Health Checks.
        /// </summary>
        /// <param name="healthChecksBuilder">O builder de Health Checks.</param>
        /// <param name="checkConfig">Lista de configurações de verificação (<see cref="CheckConfig"/>).</param>
        public static void AddAllChecks(this IHealthChecksBuilder healthChecksBuilder, params CheckConfig[]? checkConfig)
        {
            if (checkConfig is null || checkConfig.Length == 0)
            {
                healthChecksBuilder
                    .AddCheck(Check.Live, () => HealthCheckResult.Healthy(), Tags.LivenessTags)
                    .AddCheck(Check.Ready, () => HealthCheckResult.Healthy(), Tags.ReadinessTags);
                return;
            }

            foreach (var check in checkConfig)
            {
                healthChecksBuilder.AddCheck(check.Name, check.Heartbeat, check.Tags, check.Timeout);
            }
        }

        /// <summary>
        /// Acopla um endpoint web de monitoramento interno a um Worker Service (Background Service).
        /// </summary>
        /// <param name="hostBuilder">O builder do host da aplicação.</param>
        /// <param name="mapHealth">Função opcional para configurações customizadas de rota.</param>
        /// <returns>O builder do host para encadeamento fluente.</returns>
        public static IHostBuilder AddWorkerServiceHealthChecks(this IHostBuilder hostBuilder, Func<MaphealthCheckConfig[]>? mapHealth = default)
        {
            return hostBuilder.ConfigureWebHostDefaults(wBuilder =>
            {
                wBuilder.Configure(config =>
                {
                    config.UseRouting();
                    config.UseEndpoints(endpoints =>
                    {
                        endpoints.MapRequiredHealthCheck(mapHealth);
                    });
                });
            });
        }

        /// <summary>
        /// Mapeia os endpoints padronizados de probes Kubernetes (<c>/livez</c> e <c>/readyz</c>) com serialização JSON nativa.
        /// </summary>
        /// <param name="endpointRouteBuilder">O roteador de endpoints ASP.NET Core.</param>
        /// <returns>O roteador para encadeamento fluente.</returns>
        public static IEndpointRouteBuilder MapLightweightHealthChecks(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapHealthChecks("/livez", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live") || registration.Tags.Contains("liveness"),
                ResponseWriter = LightweightHealthCheckResponseWriter.WriteResponseAsync
            });

            endpointRouteBuilder.MapHealthChecks("/readyz", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("ready") || registration.Tags.Contains("readiness"),
                ResponseWriter = LightweightHealthCheckResponseWriter.WriteResponseAsync
            });

            endpointRouteBuilder.MapHealthChecks("/healthz", _options);

            return endpointRouteBuilder;
        }

        /// <summary>
        /// Mapeia os endpoints HTTP padrão de Health Check (<c>/health</c>, <c>/ready</c> e <c>/liveness</c>) nas rotas da aplicação.
        /// </summary>
        /// <param name="endpointRouteBuilder">O roteador de endpoints ASP.NET Core.</param>
        /// <param name="mapHealth">Função opcional para sobrescrever rotas.</param>
        /// <returns>O roteador para encadeamento fluente.</returns>
        public static IEndpointRouteBuilder MapRequiredHealthCheck(this IEndpointRouteBuilder endpointRouteBuilder, Func<MaphealthCheckConfig[]>? mapHealth = default)
        {
            var configs = mapHealth?.Invoke()?.ToList();
            var configService = endpointRouteBuilder.ServiceProvider?.GetService<IConfiguration>();

            if (configService != null)
            {
                var section = configService.GetSection(nameof(HealthCheckConfig));
                var options = section?.Get<HealthCheckConfig>();
                configs = AddConfiguredHealthChecks(options, configs);
            }

            endpointRouteBuilder.AddAllMapHealthChecks(configs);
            return endpointRouteBuilder;
        }

        internal static List<MaphealthCheckConfig>? AddConfiguredHealthChecks(HealthCheckConfig? options, List<MaphealthCheckConfig>? configs)
        {
            if (options?.Patterns is not null)
            {
                configs ??= new List<MaphealthCheckConfig>();

                if (!string.IsNullOrWhiteSpace(options.Patterns.Health))
                    configs.Add(new MaphealthCheckConfig(options.Patterns.Health!, _options));

                if (!string.IsNullOrWhiteSpace(options.Patterns.Liveness))
                    configs.Add(new MaphealthCheckConfig(options.Patterns.Liveness!, _optionsResponse));

                if (!string.IsNullOrWhiteSpace(options.Patterns.Readiness))
                    configs.Add(new MaphealthCheckConfig(options.Patterns.Readiness!, _optionsResponse));
            }
            return configs;
        }

        private static void AddAllMapHealthChecks(this IEndpointRouteBuilder endpointRouteBuilder, List<MaphealthCheckConfig>? configs)
        {
            if (configs is null || configs.Count == 0)
            {
                endpointRouteBuilder.MapHealthChecks(MapHealthChecks.Health, _options);
                endpointRouteBuilder.MapHealthChecks(MapHealthChecks.Ready, _optionsResponse);
                endpointRouteBuilder.MapHealthChecks(MapHealthChecks.Live, _optionsResponse);
                return;
            }

            foreach (var config in configs)
            {
                endpointRouteBuilder.MapHealthChecks(config.Pattern, config.Options);
            }
        }
    }
}