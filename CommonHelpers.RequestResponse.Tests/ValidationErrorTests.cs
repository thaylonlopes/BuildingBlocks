using System;
using System.Collections.Generic;
using FluentAssertions;

namespace CommonHelpers.RequestResponse.Tests
{
    public class ValidationErrorTests
    {
        [Fact]
        public void Given_FromFailures_With_Multiple_Field_Errors_Should_Populate_Errors_Dictionary()
        {
            // Arrange
            var failures = new Dictionary<string, string[]>
            {
                { "Email", new[] { "E-mail inválido.", "Domínio corporativo obrigatório." } },
                { "Password", new[] { "Mínimo de 8 caracteres." } }
            };

            // Act
            var validationError = ValidationError.FromFailures(failures);

            // Assert
            validationError.Type.Should().Be(ErrorType.Validation);
            validationError.Code.Should().Be("Validation.General");
            validationError.Message.Should().Be("Ocorreram um ou mais erros de validação.");
            validationError.Errors.Should().HaveCount(2);
            validationError.Errors["Email"].Should().HaveCount(2);
            validationError.Errors["Password"].Should().ContainSingle().Which.Should().Be("Mínimo de 8 caracteres.");
        }

        [Fact]
        public void Given_ForField_Should_Create_ValidationError_For_Single_Field()
        {
            // Act
            var error = ValidationError.ForField("Cpf", "CPF deve conter 11 dígitos.", "Customer.InvalidCpf");

            // Assert
            error.Type.Should().Be(ErrorType.Validation);
            error.Code.Should().Be("Customer.InvalidCpf");
            error.Errors.Should().ContainKey("Cpf");
            error.Errors["Cpf"].Should().ContainSingle().Which.Should().Be("CPF deve conter 11 dígitos.");
        }

        [Fact]
        public void Given_Null_Failures_Dictionary_Should_Default_To_Empty_Dictionary()
        {
            // Act
            var error = ValidationError.FromFailures(null);

            // Assert
            error.Errors.Should().NotBeNull();
            error.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Given_ValidationError_Returned_In_Result_Should_Expose_Errors()
        {
            // Arrange
            var valError = ValidationError.ForField("Age", "Idade mínima é 18 anos.");

            // Act
            Result<int> result = Result.Failure<int>(valError);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ValidationError>();

            var typedError = (ValidationError)result.Error;
            typedError.Errors["Age"].Should().Contain("Idade mínima é 18 anos.");
        }

        [Fact]
        public void Given_ForField_With_Null_Field_Or_Message_Should_Throw_ArgumentNullException()
        {
            // Act & Assert
            Action act1 = () => ValidationError.ForField(null!, "Erro");
            act1.Should().Throw<ArgumentNullException>();

            Action act2 = () => ValidationError.ForField("Field", null!);
            act2.Should().Throw<ArgumentNullException>();
        }
    }
}

