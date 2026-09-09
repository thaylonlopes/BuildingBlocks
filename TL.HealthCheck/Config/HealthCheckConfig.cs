namespace TL.HealthCheck.Config
{
    /// <summary>
    /// Seção de configuração de Health Checks mapeada a partir do <c>appsettings.json</c>.
    /// </summary>
    public class HealthCheckConfig
    {
        /// <summary>
        /// Tempo limite global em segundos para as verificações de saúde.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Padrões de rotas customizadas para os endpoints de Health Check.
        /// </summary>
        public Patterns? Patterns { get; set; }
    }
}
