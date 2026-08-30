namespace CommonHelpers.HealthCheck.Constants
{
    /// <summary>
    /// Nomes padronizados para as sondas e verificações de saúde.
    /// </summary>
    public static class Check
    {
        /// <summary>
        /// Nome da sonda de Liveness ("live").
        /// </summary>
        public static readonly string Live = "live";

        /// <summary>
        /// Nome da sonda de Readiness ("ready").
        /// </summary>
        public static readonly string Ready = "ready";

        /// <summary>
        /// Nome da sonda de saúde geral ("health").
        /// </summary>
        public static readonly string Health = "health";
    }
}
