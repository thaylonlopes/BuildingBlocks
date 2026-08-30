using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace CommonHelpers.HealthCheck.Config
{
    public class MaphealthCheckConfig
    {
        public MaphealthCheckConfig(string pattern, HealthCheckOptions options)
        {
            Pattern = pattern;
            Options = options;
        }

        public string Pattern { get; set; }
        public HealthCheckOptions Options { get; set; }
    }
}
