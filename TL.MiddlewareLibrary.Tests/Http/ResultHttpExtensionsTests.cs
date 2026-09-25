using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using TL.BaseContracts;
using TL.BaseContracts.Http;
using Xunit;

namespace TL.MiddlewareLibrary.Tests.Http;

public class ResultHttpExtensionsTests
{
    [Fact]
    public void Given_GenericSuccessResult_When_ToHttpResult_Should_Return_Ok_With_Value()
    {
        var result = Result<string>.Success("Payload de teste");

        var httpResult = result.ToHttpResult();

        var okResult = httpResult.Should().BeOfType<Ok<string>>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be("Payload de teste");
    }

    [Fact]
    public void Given_GenericSuccessResult_With_Custom_OnSuccess_Should_Return_Custom_IResult()
    {
        var result = Result<int>.Success(100);

        var httpResult = result.ToHttpResult(val => Results.Created($"/api/items/{val}", val));

        var createdResult = httpResult.Should().BeOfType<Created<int>>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Location.Should().Be("/api/items/100");
        createdResult.Value.Should().Be(100);
    }

    [Fact]
    public void Given_NonGenericSuccessResult_When_ToHttpResult_Should_Return_Ok_Without_Value()
    {
        var result = Result.Success();

        var httpResult = result.ToHttpResult();

        var okResult = httpResult.Should().BeOfType<Ok>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public void Given_NonGenericSuccessResult_With_Custom_OnSuccess_Should_Return_Custom_IResult()
    {
        var result = Result.Success();

        var httpResult = result.ToHttpResult(() => Results.NoContent());

        var noContentResult = httpResult.Should().BeOfType<NoContent>().Subject;
        noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public void Given_NotFound_Error_When_ToHttpResult_Should_Return_ProblemDetails_404()
    {
        var error = Error.NotFound("Customer.NotFound", "Cliente não encontrado.");
        var result = Result<string>.Failure(error);

        var httpResult = result.ToHttpResult();

        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problemResult.ProblemDetails.Title.Should().Be("Customer.NotFound");
        problemResult.ProblemDetails.Detail.Should().Be("Cliente não encontrado.");
        problemResult.ProblemDetails.Extensions.Should().ContainKey("errorCode").WhoseValue.Should().Be("Customer.NotFound");
    }

    [Fact]
    public void Given_Validation_Error_When_ToHttpResult_Should_Return_ProblemDetails_400()
    {
        var error = Error.Validation("Form.Invalid", "Formulário contém campos inválidos.");
        var result = Result.Failure(error);

        var httpResult = result.ToHttpResult();

        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problemResult.ProblemDetails.Title.Should().Be("Form.Invalid");
        problemResult.ProblemDetails.Detail.Should().Be("Formulário contém campos inválidos.");
        problemResult.ProblemDetails.Extensions.Should().ContainKey("errorCode").WhoseValue.Should().Be("Form.Invalid");
    }

    [Fact]
    public void Given_Conflict_Error_When_ToHttpResult_Should_Return_ProblemDetails_409()
    {
        var error = Error.Conflict("Order.AlreadyExists", "Pedido com este identificador já existe.");
        var result = Result<int>.Failure(error);

        var httpResult = result.ToHttpResult();

        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        problemResult.ProblemDetails.Title.Should().Be("Order.AlreadyExists");
        problemResult.ProblemDetails.Detail.Should().Be("Pedido com este identificador já existe.");
    }

    [Fact]
    public void Given_Unauthorized_Error_When_ToHttpResult_Should_Return_ProblemDetails_401()
    {
        var error = Error.Unauthorized("Auth.ExpiredToken", "Token de sessão expirado.");
        var result = Result.Failure(error);

        var httpResult = result.ToHttpResult();

        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        problemResult.ProblemDetails.Title.Should().Be("Auth.ExpiredToken");
    }

    [Fact]
    public void Given_Forbidden_Error_When_ToHttpResult_Should_Return_ProblemDetails_403()
    {
        var error = Error.Forbidden("Auth.ForbiddenScope", "Escopo insuficiente para esta operação.");
        var result = Result.Failure(error);

        var httpResult = result.ToHttpResult();

        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ProblemDetails.Title.Should().Be("Auth.ForbiddenScope");
    }

    [Fact]
    public void Given_Failure_Error_When_ToHttpResult_Should_Return_ProblemDetails_500()
    {
        var error = Error.Failure("Database.Timeout", "Tempo limite de conexão com banco excedido.");
        var result = Result.Failure(error);

        var httpResult = result.ToHttpResult();

        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        problemResult.ProblemDetails.Title.Should().Be("Database.Timeout");
    }

    [Fact]
    public void Given_Rich_ValidationError_When_ToHttpResult_Should_Return_ValidationProblem()
    {
        var failures = new Dictionary<string, string[]>
        {
            { "Email", new[] { "E-mail inválido." } },
            { "Age", new[] { "Idade deve ser maior que 18." } }
        };
        var validationError = ValidationError.FromFailures(failures, "Falha de validação dos dados.", "User.Invalid");
        var result = Result<bool>.Failure(validationError);

        var httpResult = result.ToHttpResult();

        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problemResult.ProblemDetails.Title.Should().Be("User.Invalid");
        problemResult.ProblemDetails.Detail.Should().Be("Falha de validação dos dados.");
        problemResult.ProblemDetails.Extensions.Should().ContainKey("errorCode").WhoseValue.Should().Be("User.Invalid");
    }

    [Fact]
    public void Given_Null_Result_When_ToHttpResult_Should_Throw_ArgumentNullException()
    {
        Result<string> nullGenericResult = null!;
        Result nullVoidResult = null!;

        var genericAction = () => nullGenericResult.ToHttpResult();
        var voidAction = () => nullVoidResult.ToHttpResult();

        genericAction.Should().Throw<ArgumentNullException>();
        voidAction.Should().Throw<ArgumentNullException>();
    }
}
