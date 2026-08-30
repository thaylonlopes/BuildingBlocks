using System;
using FluentAssertions;

namespace CommonHelpers.RequestResponse.Tests
{
    public class ResultTests
    {
        [Fact]
        public void Given_Success_Result_Should_Be_IsSuccess_True_And_Return_Value()
        {
            // Arrange & Act
            Result<string> result = Result.Success("Tudo certo!");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Value.Should().Be("Tudo certo!");
            result.Error.Should().Be(Error.None);
        }

        [Fact]
        public void Given_NonGeneric_Result_Success_Should_Have_Correct_Properties()
        {
            // Act
            Result result = Result.Success();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Error.Should().Be(Error.None);

            var executed = result.Match(
                onSuccess: () => "OK",
                onFailure: err => "FAIL"
            );
            executed.Should().Be("OK");
        }

        [Fact]
        public void Given_NonGeneric_Result_Failure_Should_Have_Correct_Properties()
        {
            // Arrange
            var error = Error.Validation("Val.Code", "Dado inválido");

            // Act
            Result result = Result.Failure(error);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(error);

            var executed = result.Match(
                onSuccess: () => "OK",
                onFailure: err => $"FAIL: {err.Message}"
            );
            executed.Should().Be("FAIL: Dado inválido");
        }

        [Fact]
        public void Given_Failure_Result_Should_Be_IsFailure_True_And_Throw_On_Value_Access()
        {
            // Arrange
            var error = Error.NotFound("User.NotFound", "Usuário não foi encontrado.");

            // Act
            Result<string> result = Result.Failure<string>(error);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(error);
            result.Error.Type.Should().Be(ErrorType.NotFound);

            Action accessValue = () => { var v = result.Value; };
            accessValue.Should().Throw<InvalidOperationException>()
                .WithMessage("*Usuário não foi encontrado*");
        }

        [Fact]
        public void Given_Invalid_Failure_Creation_Should_Throw_ArgumentException()
        {
            // Act & Assert
            Action actNull = () => Result.Failure(null!);
            actNull.Should().Throw<ArgumentException>();

            Action actNone = () => Result.Failure(Error.None);
            actNone.Should().Throw<ArgumentException>();

            Action actNullSuccessValue = () => Result.Success<string>(null!);
            actNullSuccessValue.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Given_Implicit_Conversion_From_Value_Should_Create_Success_Result()
        {
            // Act
            Result<int> result = 42;

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
        }

        [Fact]
        public void Given_Implicit_Conversion_From_Error_Should_Create_Failure_Result()
        {
            // Arrange
            Error error = Error.Validation("Input.Invalid", "Parâmetro inválido");

            // Act
            Result<string> result = error;

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Input.Invalid");
        }

        [Fact]
        public void Given_Match_Function_Should_Execute_Correct_Branch()
        {
            // Arrange
            Result<int> successResult = Result.Success(100);
            Result<int> failureResult = Result.Failure<int>(Error.Conflict("Id.Duplicate", "Item duplicado"));

            // Act
            var successMessage = successResult.Match(
                value => $"Valor: {value}",
                error => $"Erro: {error.Message}");

            var failureMessage = failureResult.Match(
                value => $"Valor: {value}",
                error => $"Erro: {error.Message}");

            // Assert
            successMessage.Should().Be("Valor: 100");
            failureMessage.Should().Be("Erro: Item duplicado");
        }

        [Fact]
        public void Given_Match_With_Null_Delegates_Should_Throw_ArgumentNullException()
        {
            // Arrange
            Result<int> result = 10;

            // Act & Assert
            Action act1 = () => result.Match(null!, err => "fail");
            act1.Should().Throw<ArgumentNullException>();

            Action act2 = () => result.Match(v => "ok", null!);
            act2.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Given_Map_Function_Should_Transform_Value_On_Success()
        {
            // Arrange
            Result<int> initial = 10;

            // Act
            var mapped = initial.Map(n => n * 2);

            // Assert
            mapped.IsSuccess.Should().BeTrue();
            mapped.Value.Should().Be(20);
        }

        [Fact]
        public void Given_Map_Function_On_Failure_Should_Propagate_Error()
        {
            // Arrange
            Result<int> initial = Error.NotFound("Item.404", "Item ausente");

            // Act
            var mapped = initial.Map(n => n * 2);

            // Assert
            mapped.IsFailure.Should().BeTrue();
            mapped.Error.Code.Should().Be("Item.404");
        }

        [Fact]
        public void Given_ToResponse_Extension_Should_Convert_Result_To_Response()
        {
            // Arrange
            Result<string> successResult = "Sucesso!";
            Result<string> failureResult = Error.Validation("Email.Invalid", "E-mail inválido.");

            // Act
            var successResponse = successResult.ToResponse();
            var failureResponse = failureResult.ToResponse();

            // Assert
            successResponse.IsSuccess.Should().BeTrue();
            successResponse.GetResponse().Should().Be("Sucesso!");

            failureResponse.IsSuccess.Should().BeFalse();
            failureResponse.ErrorMessages.Should().Contain("E-mail inválido.");
        }

        [Fact]
        public void Given_ToResult_Extension_Should_Convert_Response_To_Result()
        {
            // Arrange
            var responseSuccess = new Response<string>("Payload ok");
            var responseError = new Response<string>(false);
            responseError.AddErrorMessage("Falha de conexão.");

            // Act
            var resultSuccess = responseSuccess.ToResult();
            var resultError = responseError.ToResult();

            // Assert
            resultSuccess.IsSuccess.Should().BeTrue();
            resultSuccess.Value.Should().Be("Payload ok");

            resultError.IsFailure.Should().BeTrue();
            resultError.Error.Message.Should().Be("Falha de conexão.");
        }
    }
}
