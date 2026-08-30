namespace CommonHelpers.HealthCheck.Config
{
    /// <summary>
    /// Padrões de URL relativos para mapeamento dos endpoints de Health Check.
    /// </summary>
    public class Patterns
    {
        /// <summary>
        /// Rota customizada para a verificação de saúde geral com interface UI (padrão: <c>/health</c>).
        /// </summary>
        public string? Health { get; set; }

        /// <summary>
        /// Rota customizada para a sonda de Liveness (padrão: <c>/liveness</c>).
        /// </summary>
        public string? Liveness { get; set; }

        /// <summary>
        /// Rota customizada para a sonda de Readiness (padrão: <c>/ready</c>).
        /// </summary>
        public string? Readiness { get; set; }
    }
}