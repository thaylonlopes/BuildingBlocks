using System;
using System.Collections.Generic;
using System.Linq;
using TL.HealthCheck.Config;
using TL.HealthCheck.Constants;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TL.HealthCheck.Tests
{
    public class HealthChecks
    {
        [Fact]
        public void Given_AddRequiredHealthChecks_Start_Configuration_Should_Register_Default_Probes()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            const int expectedCount = 2;

            // Act
            Action action = () => serviceCollection.AddRequiredHealthChecks();

            // Assert
            action.Should().NotThrow();

            using var sp = serviceCollection.BuildServiceProvider();
            var healthCheckService = sp.GetService<HealthCheckService>();
            healthCheckService.Should().NotBeNull();

            var options = sp.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            options.Registrations.Should().HaveCount(expectedCount);
            options.Registrations.Select(r => r.Name).Should().Contain(new[] { Check.Live, Check.Ready });
        }

        [Fact]
        public void Given_AddRequiredHealthChecks_With_Empty_Array_Callback_Should_Register_Default_Probes()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            // Act
            serviceCollection.AddRequiredHealthChecks(() => Array.Empty<CheckConfig>());

            // Assert
            using var sp = serviceCollection.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            options.Registrations.Should().HaveCount(2);
            options.Registrations.Select(r => r.Name).Should().Contain(new[] { Check.Live, Check.Ready });
        }

        [Fact]
        public void Given_AddRequiredHealthChecks_StartMultipleParams_Configuration_Should_Register_All_Custom_Checks()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            const int expectedCount = 3;

            // Act
            Action action = () => serviceCollection.AddRequiredHealthChecks(() =>
            {
                return new CheckConfig[]
                {
                    new CheckConfig("item1", new[] { "tagOne" }, () => HealthCheckResult.Healthy(), TimeSpan.FromSeconds(1)),
                    new CheckConfig("item2", () => HealthCheckResult.Degraded()),
                    new CheckConfig("item3", new[] { "tagTwo" }, () => HealthCheckResult.Healthy(), TimeSpan.FromSeconds(10))
                };
            });

            // Assert
            action.Should().NotThrow();

            using var sp = serviceCollection.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            options.Registrations.Should().HaveCount(expectedCount);
            options.Registrations.Select(r => r.Name).Should().Contain(new[] { "item1", "item2", "item3" });
        }

        [Fact]
        public void Given_AddConfiguredHealthChecks_With_Valid_Patterns_Should_Add_Configured_Routes()
        {
            // Arrange
            var options = new HealthCheckConfig
            {
                Patterns = new Patterns
                {
                    Health = "/custom-health",
                    Liveness = "/custom-liveness",
                    Readiness = "/custom-ready"
                }
            };

            // Act
            var result = HealthCheckExtensions.AddConfiguredHealthChecks(options, new List<MaphealthCheckConfig>());

            // Assert
            result.Should().NotBeNull();
            result!.Should().HaveCount(3);
            result!.Select(r => r.Pattern).Should().Contain(new[] { "/custom-health", "/custom-liveness", "/custom-ready" });
        }

        [Fact]
        public void Given_AddConfiguredHealthChecks_With_Null_Or_Empty_Patterns_Should_Not_Add_Blank_Routes()
        {
            // Arrange
            var options = new HealthCheckConfig
            {
                Patterns = new Patterns
                {
                    Health = "",
                    Liveness = "   ",
                    Readiness = null
                }
            };

            // Act
            var result = HealthCheckExtensions.AddConfiguredHealthChecks(options, new List<MaphealthCheckConfig>());

            // Assert
            result.Should().NotBeNull();
            result!.Should().BeEmpty();
        }

        [Fact]
        public void Given_AddConfiguredHealthChecks_With_Null_Options_Should_Return_Configs_Unchanged()
        {
            // Arrange
            var initialConfigs = new List<MaphealthCheckConfig>();

            // Act
            var result = HealthCheckExtensions.AddConfiguredHealthChecks(null, initialConfigs);

            // Assert
            result.Should().BeSameAs(initialConfigs);
        }

        [Fact]
        public void Given_AddWorkerServiceHealthChecks_Should_Configure_Host_Without_Exception()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder();

            // Act
            Action action = () => hostBuilder.AddWorkerServiceHealthChecks();

            // Assert
            action.Should().NotThrow();
        }
    }
}