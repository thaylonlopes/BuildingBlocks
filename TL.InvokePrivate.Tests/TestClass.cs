using System;
using System.Threading.Tasks;

namespace TL.InvokePrivate.Tests
{
    public partial class MethodInvokerTests
    {
        private class TestClass
        {
            private string PrivateMethod(string input)
            {
                return $"Hello, {input}";
            }

            private string? PrivateNullReturningMethod()
            {
                return null;
            }

            private async Task<string> PrivateMethodAsync(string input)
            {
                await Task.Delay(5);
                return $"Hello, {input}";
            }

            private void PrivateVoidMethod()
            {
            }

            private async Task PrivateVoidMethodAsync()
            {
                await Task.Delay(5);
            }

            private void PrivateMethodThrowingException()
            {
                throw new InvalidOperationException("Erro de negócio disparado dentro do método privado.");
            }

            private string PrivateGenericMethodThrowingException()
            {
                throw new InvalidOperationException("Erro de negócio em método genérico.");
            }

            private async Task PrivateAsyncMethodThrowingException()
            {
                await Task.Delay(5);
                throw new InvalidOperationException("Erro de negócio em método assíncrono.");
            }

            private async Task<string> PrivateGenericAsyncMethodThrowingException()
            {
                await Task.Delay(5);
                throw new InvalidOperationException("Erro de negócio em método genérico assíncrono.");
            }
        }
    }
}
