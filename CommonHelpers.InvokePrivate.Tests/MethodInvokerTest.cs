using System;
using System.Threading.Tasks;
using FluentAssertions;

namespace CommonHelpers.InvokePrivate.Tests
{
    public partial class MethodInvokerTests
    {
        [Fact]
        public void InvokePrivateMethod_ShouldReturnExpectedResult()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            var result = MethodInvoker.InvokePrivateMethod<string>(testObj, "PrivateMethod", "World");

            // Assert
            result.Should().Be("Hello, World");
        }

        [Fact]
        public void InvokePrivateMethod_WhenReturningNull_ShouldReturnDefault()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            var result = MethodInvoker.InvokePrivateMethod<string>(testObj, "PrivateNullReturningMethod");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task InvokePrivateMethodAsync_ShouldReturnExpectedResult()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            var result = await MethodInvoker.InvokePrivateMethodAsync<string>(testObj, "PrivateMethodAsync", "Async World");

            // Assert
            result.Should().Be("Hello, Async World");
        }

        [Fact]
        public void InvokePrivateVoidMethod_ShouldNotThrowException()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            Action action = () => MethodInvoker.InvokePrivateMethod(testObj, "PrivateVoidMethod");

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public async Task InvokePrivateVoidMethodAsync_ShouldNotThrowException()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            Func<Task> action = async () => await MethodInvoker.InvokePrivateMethodAsync(testObj, "PrivateVoidMethodAsync");

            // Assert
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public void InvokePrivateMethod_WhenMethodThrows_ShouldUnwrapAndThrowInnerException()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            Action action = () => MethodInvoker.InvokePrivateMethod(testObj, "PrivateMethodThrowingException");

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Erro de negócio disparado dentro do método privado.");
        }

        [Fact]
        public void InvokePrivateMethodGeneric_WhenMethodThrows_ShouldUnwrapAndThrowInnerException()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            Action action = () => MethodInvoker.InvokePrivateMethod<string>(testObj, "PrivateGenericMethodThrowingException");

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Erro de negócio em método genérico.");
        }

        [Fact]
        public async Task InvokePrivateMethodAsync_WhenMethodThrows_ShouldUnwrapAndThrowInnerException()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            Func<Task> action = async () => await MethodInvoker.InvokePrivateMethodAsync(testObj, "PrivateAsyncMethodThrowingException");

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Erro de negócio em método assíncrono.");
        }

        [Fact]
        public async Task InvokePrivateMethodGenericAsync_WhenMethodThrows_ShouldUnwrapAndThrowInnerException()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            Func<Task> action = async () => await MethodInvoker.InvokePrivateMethodAsync<string>(testObj, "PrivateGenericAsyncMethodThrowingException");

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Erro de negócio em método genérico assíncrono.");
        }

        [Fact]
        public void InvokePrivateMethod_WhenMethodNotFound_ShouldThrowMissingMethodException()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            Action action = () => MethodInvoker.InvokePrivateMethod(testObj, "NonExistentMethod");

            // Assert
            action.Should().Throw<MissingMethodException>()
                .WithMessage("*NonExistentMethod*");
        }

        [Fact]
        public void InvokePrivateMethod_WhenNullObject_ShouldThrowArgumentNullException()
        {
            // Act
            Action action = () => MethodInvoker.InvokePrivateMethod(null!, "PrivateMethod");

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void InvokePrivateMethod_WhenEmptyMethodName_ShouldThrowArgumentException()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            Action action = () => MethodInvoker.InvokePrivateMethod(testObj, string.Empty);

            // Assert
            action.Should().Throw<ArgumentException>();
        }
    }
}
