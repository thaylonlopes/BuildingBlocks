using System;
using CommonHelpers.HealthCheck.Config;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CommonHelpers.HealthCheck.Tests
{
    public class CheckConfigTests
    {
        [Fact]
        public void Given_Valid_2Param_Constructor_Should_Initialize_Properties()
        {
            // Arrange & Act
            var check = new CheckConfig("sql_db", () => HealthCheckResult.Healthy());

            // Assert
            check.Name.Should().Be("sql_db");
            check.Heartbeat().Status.Should().Be(HealthStatus.Healthy);
            check.Tags.Should().BeEmpty();
            check.Timeout.Should().BeNull();
        }

        [Fact]
        public void Given_Valid_4Param_Constructor_Should_Initialize_All_Properties()
        {
            // Arrange
            var tags = new[] { "ready", "db" };
            var timeout = TimeSpan.FromSeconds(5);

            // Act
            var check = new CheckConfig("cache", tags, () => HealthCheckResult.Degraded(), timeout);

            // Assert
            check.Name.Should().Be("cache");
            check.Tags.Should().Contain(new[] { "ready", "db" });
            check.Heartbeat().Status.Should().Be(HealthStatus.Degraded);
            check.Timeout.Should().Be(timeout);
        }

        [Fact]
        public void Given_Null_Name_Should_Throw_ArgumentNullException()
        {
            // Act
            Action action = () => new CheckConfig(null!, () => HealthCheckResult.Healthy());

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Given_Null_Heartbeat_Should_Throw_ArgumentNullException()
        {
            // Act
            Action action = () => new CheckConfig("test", null!);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Given_Null_Tags_Should_Default_To_Empty_Enumerable()
        {
            // Act
            var check = new CheckConfig("service", null, () => HealthCheckResult.Healthy(), null);

            // Assert
            check.Tags.Should().NotBeNull();
            check.Tags.Should().BeEmpty();
        }
    }
}

