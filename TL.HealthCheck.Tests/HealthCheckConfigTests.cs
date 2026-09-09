using TL.HealthCheck.Config;
using FluentAssertions;

namespace TL.HealthCheck.Tests
{
    public class HealthCheckConfigTests
    {
        [Fact]
        public void Given_HealthCheckConfig_Should_Set_And_Get_Properties()
        {
            // Arrange
            var config = new HealthCheckConfig
            {
                Timeout = 30,
                Patterns = new Patterns
                {
                    Health = "/my-health",
                    Liveness = "/my-live",
                    Readiness = "/my-ready"
                }
            };

            // Assert
            config.Timeout.Should().Be(30);
            config.Patterns.Should().NotBeNull();
            config.Patterns!.Health.Should().Be("/my-health");
            config.Patterns!.Liveness.Should().Be("/my-live");
            config.Patterns!.Readiness.Should().Be("/my-ready");
        }
    }
}

