using System;
using FluentAssertions;

namespace CommonHelpers.RequestResponse.Tests
{
    public class ErrorTests
    {
        [Fact]
        public void Given_Error_Factory_Methods_Should_Create_Correct_Error_Types()
        {
            // Act & Assert
            var failure = Error.Failure("Failure.Code", "Falha geral");
            failure.Code.Should().Be("Failure.Code");
            failure.Message.Should().Be("Falha geral");
            failure.Type.Should().Be(ErrorType.Failure);

            var validation = Error.Validation("Val.Code", "Erro de validação");
            validation.Code.Should().Be("Val.Code");
            validation.Message.Should().Be("Erro de validação");
            validation.Type.Should().Be(ErrorType.Validation);

            var notFound = Error.NotFound("NF.Code", "Não encontrado");
            notFound.Code.Should().Be("NF.Code");
            notFound.Message.Should().Be("Não encontrado");
            notFound.Type.Should().Be(ErrorType.NotFound);

            var conflict = Error.Conflict("Conf.Code", "Conflito de dados");
            conflict.Code.Should().Be("Conf.Code");
            conflict.Message.Should().Be("Conflito de dados");
            conflict.Type.Should().Be(ErrorType.Conflict);

            var unauthorized = Error.Unauthorized("Auth.Code", "Não autorizado");
            unauthorized.Code.Should().Be("Auth.Code");
            unauthorized.Message.Should().Be("Não autorizado");
            unauthorized.Type.Should().Be(ErrorType.Unauthorized);

            var forbidden = Error.Forbidden("Forb.Code", "Acesso proibido");
            forbidden.Code.Should().Be("Forb.Code");
            forbidden.Message.Should().Be("Acesso proibido");
            forbidden.Type.Should().Be(ErrorType.Forbidden);
        }

        [Fact]
        public void Given_Error_None_Should_Have_Empty_Values()
        {
            // Act & Assert
            Error.None.Code.Should().BeEmpty();
            Error.None.Message.Should().BeEmpty();
            Error.None.Type.Should().Be(ErrorType.Failure);
        }

        [Fact]
        public void Given_Implicit_Conversion_From_String_Should_Create_Validation_Error()
        {
            // Act
            Error error = "O campo CPF é inválido.";

            // Assert
            error.Code.Should().Be("General.Validation");
            error.Message.Should().Be("O campo CPF é inválido.");
            error.Type.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public void Given_Two_Equal_Errors_Should_Have_Value_Equality()
        {
            // Arrange
            var error1 = Error.NotFound("User.404", "Usuário inexistente");
            var error2 = Error.NotFound("User.404", "Usuário inexistente");

            // Assert
            error1.Should().Be(error2);
            (error1 == error2).Should().BeTrue();
        }
    }
}

