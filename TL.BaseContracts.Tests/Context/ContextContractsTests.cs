using System.Collections.Generic;
using FluentAssertions;
using TL.BaseContracts.Context;
using Xunit;

namespace TL.BaseContracts.Tests.Context;

public class ContextContractsTests
{
    [Fact]
    public void Given_CustomCurrentUserImplementation_When_Instantiated_Should_Expose_Properties()
    {
        var roles = new List<string> { "Administrator", "Auditor" };
        ICurrentUser user = new TestCurrentUser("user-123", "admin@empresa.com", roles, true);

        user.Id.Should().Be("user-123");
        user.Email.Should().Be("admin@empresa.com");
        user.Roles.Should().ContainInOrder("Administrator", "Auditor");
        user.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void Given_AnonymousUser_When_Instantiated_Should_Have_Null_Id_And_False_IsAuthenticated()
    {
        ICurrentUser user = new TestCurrentUser(null, null, new List<string>(), false);

        user.Id.Should().BeNull();
        user.Email.Should().BeNull();
        user.Roles.Should().BeEmpty();
        user.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void Given_CurrentTenant_With_Valid_TenantId_When_Queried_Should_Return_HasTenant_True()
    {
        ICurrentTenant tenant = new TestCurrentTenant("tenant-corp-42");

        tenant.TenantId.Should().Be("tenant-corp-42");
        tenant.HasTenant.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_CurrentTenant_Without_TenantId_When_Queried_Should_Return_HasTenant_False(string? tenantId)
    {
        ICurrentTenant tenant = new TestCurrentTenant(tenantId);

        tenant.TenantId.Should().Be(tenantId);
        tenant.HasTenant.Should().BeFalse();
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public TestCurrentUser(string? id, string? email, IReadOnlyList<string> roles, bool isAuthenticated)
        {
            Id = id;
            Email = email;
            Roles = roles;
            IsAuthenticated = isAuthenticated;
        }

        public string? Id { get; }
        public string? Email { get; }
        public IReadOnlyList<string> Roles { get; }
        public bool IsAuthenticated { get; }
    }

    private sealed class TestCurrentTenant : ICurrentTenant
    {
        public TestCurrentTenant(string? tenantId)
        {
            TenantId = tenantId;
        }

        public string? TenantId { get; }

#if !NETCOREAPP3_0_OR_GREATER && !NET8_0_OR_GREATER
        public bool HasTenant => !string.IsNullOrWhiteSpace(TenantId);
#endif
    }
}
