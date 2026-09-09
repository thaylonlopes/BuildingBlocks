namespace TL.HealthCheck.Constants
{
    /// <summary>
    /// Rotas HTTP padrão expostas para orquestradores e ferramentas de monitoramento.
    /// </summary>
    public static class MapHealthChecks
    {
        /// <summary>
        /// Rota padrão de Liveness ("/liveness").
        /// </summary>
        public static readonly string Live = "/liveness";

        /// <summary>
        /// Rota padrão de Readiness ("/ready").
        /// </summary>
        public static readonly string Ready = "/ready";

        /// <summary>
        /// Rota padrão de Health geral ("/health").
        /// </summary>
        public static readonly string Health = "/health";
    }
}
