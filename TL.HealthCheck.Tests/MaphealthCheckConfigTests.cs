using TL.HealthCheck.Config;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace TL.HealthCheck.Tests
{
    public class MaphealthCheckConfigTests
    {
        [Fact]
        public void Given_MaphealthCheckConfig_Should_Set_And_Get_Properties()
        {
            // Arrange
            var options = new HealthCheckOptions();
            var config = new MaphealthCheckConfig("/custom-route", options);

            // Assert
            config.Pattern.Should().Be("/custom-route");
            config.Options.Should().BeSameAs(options);

            // Test setters
            var newOptions = new HealthCheckOptions();
            config.Pattern = "/new-route";
            config.Options = newOptions;

            config.Pattern.Should().Be("/new-route");
            config.Options.Should().BeSameAs(newOptions);
        }
    }
}

