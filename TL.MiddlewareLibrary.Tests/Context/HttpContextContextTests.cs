using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TL.BaseContracts.Context;
using TL.MiddlewareLibrary.Context;
using TL.MiddlewareLibrary.Extensions;
using Xunit;

namespace TL.MiddlewareLibrary.Tests.Context;

public class HttpContextContextTests
{
    [Fact]
    public void Given_Authenticated_User_Claims_Should_Populate_ICurrentUser_Properties()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "usr_999"),
            new(ClaimTypes.Email, "usuario@corporativo.com"),
            new(ClaimTypes.Role, "Admin"),
            new(ClaimTypes.Role, "Manager")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        ICurrentUser currentUser = new HttpContextCurrentUser(accessorMock.Object);

        currentUser.IsAuthenticated.Should().BeTrue();
        currentUser.Id.Should().Be("usr_999");
        currentUser.Email.Should().Be("usuario@corporativo.com");
        currentUser.Roles.Should().ContainInOrder("Admin", "Manager");
    }

    [Fact]
    public void Given_Sub_Claim_When_NameIdentifier_Missing_Should_Resolve_UserId()
    {
        var claims = new List<Claim>
        {
            new("sub", "jwt-sub-123"),
            new("email", "sub@teste.com")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        ICurrentUser currentUser = new HttpContextCurrentUser(accessorMock.Object);

        currentUser.Id.Should().Be("jwt-sub-123");
        currentUser.Email.Should().Be("sub@teste.com");
        currentUser.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void Given_Anonymous_Request_Should_Return_Null_Identity_And_False()
    {
        var httpContext = new DefaultHttpContext();
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        ICurrentUser currentUser = new HttpContextCurrentUser(accessorMock.Object);

        currentUser.IsAuthenticated.Should().BeFalse();
        currentUser.Id.Should().BeNull();
        currentUser.Email.Should().BeNull();
        currentUser.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Given_Null_HttpContext_Should_Safely_Return_Default_Values()
    {
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        ICurrentUser currentUser = new HttpContextCurrentUser(accessorMock.Object);
        ICurrentTenant currentTenant = new HttpContextCurrentTenant(accessorMock.Object);

        currentUser.IsAuthenticated.Should().BeFalse();
        currentUser.Id.Should().BeNull();
        currentUser.Email.Should().BeNull();
        currentUser.Roles.Should().BeEmpty();

        currentTenant.TenantId.Should().BeNull();
        currentTenant.HasTenant.Should().BeFalse();
    }

    [Fact]
    public void Given_Tenant_Header_Should_Populate_ICurrentTenant()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "tenant-matriz-01";

        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        ICurrentTenant currentTenant = new HttpContextCurrentTenant(accessorMock.Object, "X-Tenant-Id");

        currentTenant.TenantId.Should().Be("tenant-matriz-01");
        currentTenant.HasTenant.Should().BeTrue();
    }

    [Fact]
    public void Given_Tenant_Claim_When_Header_Missing_Should_Resolve_From_Claim()
    {
        var claims = new List<Claim> { new("tenant_id", "tenant-claim-42") };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        ICurrentTenant currentTenant = new HttpContextCurrentTenant(accessorMock.Object);

        currentTenant.TenantId.Should().Be("tenant-claim-42");
        currentTenant.HasTenant.Should().BeTrue();
    }

    [Fact]
    public void Given_ServiceCollection_Extensions_Should_Resolve_Services()
    {
        var services = new ServiceCollection();
        services.AddCurrentUser();
        services.AddCurrentTenant("X-Custom-Tenant");

        var provider = services.BuildServiceProvider();

        var currentUser = provider.GetService<ICurrentUser>();
        var currentTenant = provider.GetService<ICurrentTenant>();

        currentUser.Should().NotBeNull();
        currentTenant.Should().NotBeNull();
    }
}
