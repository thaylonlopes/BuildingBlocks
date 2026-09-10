using System.Collections.Generic;
using TL.BaseContracts;
using TL.MiddlewareLibrary.Models;
using Xunit;

namespace TL.MiddlewareLibrary.Tests
{
    public class ErrorContractsMappingTests
    {
        [Fact]
        public void FromError_WhenGivenNotFoundError_ShouldMapTo404AndMatchingTitle()
        {
            var error = Error.NotFound("User.NotFound", "Usuário não encontrado na base de dados.");

            var response = ProblemDetailsResponse.FromError(error, "/api/usuarios/10", "trace-not-found-1");

            Assert.NotNull(response);
            Assert.Equal(404, response.Status);
            Assert.Equal("Not Found", response.Title);
            Assert.Equal("User.NotFound", response.Code);
            Assert.Equal("Usuário não encontrado na base de dados.", response.Detail);
            Assert.Equal("/api/usuarios/10", response.Instance);
            Assert.Equal("trace-not-found-1", response.TraceId);
        }

        [Fact]
        public void FromError_WhenGivenConflictError_ShouldMapTo409AndMatchingTitle()
        {
            var error = Error.Conflict("User.EmailDuplicate", "O e-mail informado já está em uso.");

            var response = ProblemDetailsResponse.FromError(error, "/api/usuarios", "trace-conflict-1");

            Assert.Equal(409, response.Status);
            Assert.Equal("Conflict", response.Title);
            Assert.Equal("User.EmailDuplicate", response.Code);
            Assert.Equal("O e-mail informado já está em uso.", response.Detail);
        }

        [Fact]
        public void FromValidationError_ShouldMapTo400AndIncludeErrorsDictionary()
        {
            var failures = new Dictionary<string, string[]>
            {
                { "Valor", new[] { "Valor não pode ser negativo.", "Valor mínimo é 1.00." } }
            };
            var validationError = ValidationError.FromFailures(failures, "Falha de validação da transação.", "Transaction.ValidationFailed");

            var response = ProblemDetailsResponse.FromValidationError(validationError, "/api/transacoes", "trace-val-1");

            Assert.Equal(400, response.Status);
            Assert.Equal("Bad Request", response.Title);
            Assert.Equal("Transaction.ValidationFailed", response.Code);
            Assert.Equal("Falha de validação da transação.", response.Detail);
            Assert.NotNull(response.Errors);
            Assert.True(response.Errors.ContainsKey("Valor"));
            Assert.Equal(2, response.Errors["Valor"].Length);
        }

        [Fact]
        public void FromError_WhenGivenValidationErrorInstance_ShouldDelegateToFromValidationError()
        {
            Error error = ValidationError.FromFailures(new Dictionary<string, string[]>
            {
                { "Campo", new[] { "Erro de campo" } }
            });

            var response = ProblemDetailsResponse.FromError(error, "/api/teste", "trace-poly-1");

            Assert.Equal(400, response.Status);
            Assert.NotNull(response.Errors);
            Assert.True(response.Errors.ContainsKey("Campo"));
        }
    }
}
