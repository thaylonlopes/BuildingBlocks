using CommonHelpers.HealthCheck.Constants;
using FluentAssertions;

namespace CommonHelpers.HealthCheck.Tests
{
    public class ConstantsTests
    {
        [Fact]
        public void Given_Check_Constants_Should_Have_Expected_Values()
        {
            Check.Live.Should().Be("live");
            Check.Ready.Should().Be("ready");
            Check.Health.Should().Be("health");
        }

        [Fact]
        public void Given_MapHealthChecks_Constants_Should_Have_Expected_Values()
        {
            MapHealthChecks.Live.Should().Be("/liveness");
            MapHealthChecks.Ready.Should().Be("/ready");
            MapHealthChecks.Health.Should().Be("/health");
        }

        [Fact]
        public void Given_Tags_Constants_Should_Have_Expected_Values()
        {
            Tags.ReadinessTags.Should().ContainSingle().Which.Should().Be("ready");
            Tags.LivenessTags.Should().ContainSingle().Which.Should().Be("live");
        }
    }
}

