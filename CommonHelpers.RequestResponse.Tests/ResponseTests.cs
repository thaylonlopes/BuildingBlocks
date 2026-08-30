using System;
using FluentAssertions;

namespace CommonHelpers.RequestResponse.Tests
{
    public class ResponseTests
    {
        private class SampleRequest : IRequest
        {
            public Guid IdRequest { get; } = Guid.NewGuid();
        }

        private class SampleGenericRequest : IRequest<string>
        {
            public Guid IdRequest { get; } = Guid.NewGuid();
        }

        [Fact]
        public void Given_Valid_Payload_Constructor_Should_Set_IsSuccess_True_And_Return_Response()
        {
            // Arrange
            const string expectedPayload = "Payload de sucesso";

            // Act
            var response = new Response<string>(expectedPayload);

            // Assert
            response.IsSuccess.Should().BeTrue();
            response.GetResponse().Should().Be(expectedPayload);
            response.ErrorMessages.Should().BeEmpty();
            response.Messages.Should().BeEmpty();
        }

        [Fact]
        public void Given_AddMessage_Should_Keep_IsSuccess_True_And_Add_Message()
        {
            // Arrange
            var response = new Response<string>("OK");

            // Act
            response.AddMessage("Operação executada");
            response.AddMessage(""); // Mensagem vazia não deve ser inserida

            // Assert
            response.IsSuccess.Should().BeTrue();
            response.Messages.Should().HaveCount(1);
            response.Messages.Should().Contain("Operação executada");
        }

        [Fact]
        public void Given_AddErrorMessage_Should_Set_IsSuccess_To_False_And_Add_Error()
        {
            // Arrange
            var response = new Response<string>("OK");

            // Act
            response.AddErrorMessage("Falha ao salvar no banco");
            response.AddErrorMessage("   "); // Espaços em branco não devem ser inseridos

            // Assert
            response.IsSuccess.Should().BeFalse();
            response.ErrorMessages.Should().HaveCount(1);
            response.ErrorMessages.Should().Contain("Falha ao salvar no banco");
        }

        [Fact]
        public void Given_Null_Payload_Constructor_Should_Throw_ArgumentNullException()
        {
            // Act
            Action action = () => new Response<string>(null!);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Given_AddResponse_With_Null_Should_Throw_ArgumentNullException()
        {
            // Arrange
            var response = new Response<string>(true);

            // Act
            Action action = () => response.AddResponse(null!);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Given_IRequest_Implementation_Should_Provide_Valid_Guid()
        {
            // Arrange & Act
            var request = new SampleRequest();
            var genericRequest = new SampleGenericRequest();

            // Assert
            request.IdRequest.Should().NotBeEmpty();
            genericRequest.IdRequest.Should().NotBeEmpty();
        }

        [Fact]
        public void Given_NonGeneric_Response_Should_Handle_Object_And_Errors()
        {
            // Arrange
            var response = new Response(new { Code = 200 });

            // Act
            response.AddMessage("Processado");
            response.AddErrorMessage("Erro não fatal");

            // Assert
            response.IsSuccess.Should().BeFalse();
            response.GetResponse().Should().NotBeNull();
            response.Messages.Should().Contain("Processado");
            response.ErrorMessages.Should().Contain("Erro não fatal");
        }

        [Fact]
        public void Given_NonGeneric_Response_Boolean_Constructor_Should_Set_Status()
        {
            // Act
            var responseSuccess = new Response(true);
            var responseFail = new Response(false);

            // Assert
            responseSuccess.IsSuccess.Should().BeTrue();
            responseFail.IsSuccess.Should().BeFalse();
        }
    }
}
