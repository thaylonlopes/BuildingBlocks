using System;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TL.HealthCheck.Config
{
    /// <summary>
    /// Representa a configuração e o delegado de execução de uma verificação de saúde customizada.
    /// </summary>
    public class CheckConfig
    {
        /// <summary>
        /// Inicializa uma nova verificação de saúde com nome e função de verificação.
        /// </summary>
        /// <param name="name">Nome exclusivo da verificação (ex: "database", "redis").</param>
        /// <param name="heartbeat">Função que executa o teste e retorna <see cref="HealthCheckResult"/>.</param>
        public CheckConfig(string name, Func<HealthCheckResult> heartbeat)
            : this(name, Array.Empty<string>(), heartbeat, null)
        {
        }

        /// <summary>
        /// Inicializa uma nova verificação de saúde completa com tags e timeout.
        /// </summary>
        /// <param name="name">Nome exclusivo da verificação.</param>
        /// <param name="tags">Tags de agrupamento (ex: "ready", "live").</param>
        /// <param name="heartbeat">Função que executa o teste.</param>
        /// <param name="timeout">Tempo limite de tolerância para a verificação.</param>
        public CheckConfig(string name, IEnumerable<string>? tags, Func<HealthCheckResult> heartbeat, TimeSpan? timeout)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Tags = tags ?? Array.Empty<string>();
            Heartbeat = heartbeat ?? throw new ArgumentNullException(nameof(heartbeat));
            Timeout = timeout;
        }

        /// <summary>
        /// Nome exclusivo da verificação de saúde.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Coleção de tags associadas (ex: "live", "ready") para filtragem de probes.
        /// </summary>
        public IEnumerable<string> Tags { get; }

        /// <summary>
        /// Delegado responsável por executar o teste de conectividade/saúde.
        /// </summary>
        public Func<HealthCheckResult> Heartbeat { get; }

        /// <summary>
        /// Tempo limite máximo de espera da verificação.
        /// </summary>
        public TimeSpan? Timeout { get; }
    }
}
