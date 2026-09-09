namespace TL.HealthCheck.Constants
{
    /// <summary>
    /// Tags padrão utilizadas para filtragem de probes nos endpoints de Health Check.
    /// </summary>
    public static class Tags
    {
        /// <summary>
        /// Tag para verificações de prontidão (Readiness).
        /// </summary>
        public static readonly string[] ReadinessTags = { "ready" };

        /// <summary>
        /// Tag para verificações de vivacidade (Liveness).
        /// </summary>
        public static readonly string[] LivenessTags = { "live" };
    }
}
