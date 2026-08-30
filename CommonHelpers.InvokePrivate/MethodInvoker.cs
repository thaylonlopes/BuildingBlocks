using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace CommonHelpers.InvokePrivate
{
    /// <summary>
    /// Utilitário baseado em Reflection para execução dinâmica de membros privados.
    /// </summary>
    /// <remarks>
    /// 💡 **Nota de Boas Práticas:**
    /// Esta classe destina-se primariamente a testes de regressão em bibliotecas legadas e diagnósticos pontuais onde o código-fonte original não pode ser refatorado.
    /// Para novas implementações, prefira sempre o modificador <c>internal</c> combinado com <c>[InternalsVisibleTo]</c> para preservar o encapsulamento.
    /// </remarks>
    [Obsolete("Utilize apenas para suporte e compatibilidade com testes legados. Evite a quebra de encapsulamento em código de produção novo.")]
    public static class MethodInvoker
    {
        /// <summary>
        /// Invoca um método privado void síncrono de uma instância.
        /// </summary>
        /// <param name="obj">Instância do objeto que contém o método privado.</param>
        /// <param name="methodName">Nome do método privado a ser executado.</param>
        /// <param name="parameters">Parâmetros passados para o método.</param>
        /// <exception cref="ArgumentNullException">Lançada caso <paramref name="obj"/> ou <paramref name="methodName"/> seja nulo.</exception>
        /// <exception cref="MissingMethodException">Lançada se o método não for encontrado na classe.</exception>
        public static void InvokePrivateMethod(object obj, string methodName, params object[] parameters)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("O nome do método não pode ser vazio.", nameof(methodName));

            Type type = obj.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new MissingMethodException($"Método '{methodName}' não encontrado na classe '{type.FullName}'.");

            try
            {
                methodInfo.Invoke(obj, parameters);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }

        /// <summary>
        /// Invoca um método privado síncrono que retorna um valor tipado <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo de retorno esperado.</typeparam>
        /// <param name="obj">Instância do objeto que contém o método privado.</param>
        /// <param name="methodName">Nome do método privado a ser executado.</param>
        /// <param name="parameters">Parâmetros passados para o método.</param>
        /// <returns>O valor retornado pelo método privado ou o valor padrão do tipo.</returns>
        public static T? InvokePrivateMethod<T>(object obj, string methodName, params object[] parameters)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("O nome do método não pode ser vazio.", nameof(methodName));

            Type type = obj.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new MissingMethodException($"Método '{methodName}' não encontrado na classe '{type.FullName}'.");

            try
            {
                object? result = methodInfo.Invoke(obj, parameters);
                return result is null ? default : (T)result;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return default;
            }
        }

        /// <summary>
        /// Invoca um método privado assíncrono (<see cref="Task"/>) aguardando sua conclusão.
        /// </summary>
        /// <param name="obj">Instância do objeto que contém o método privado.</param>
        /// <param name="methodName">Nome do método privado a ser executado.</param>
        /// <param name="parameters">Parâmetros passados para o método.</param>
        public static async Task InvokePrivateMethodAsync(object obj, string methodName, params object[] parameters)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("O nome do método não pode ser vazio.", nameof(methodName));

            Type type = obj.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new MissingMethodException($"Método '{methodName}' não encontrado na classe '{type.FullName}'.");

            object? result;
            try
            {
                result = methodInfo.Invoke(obj, parameters);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return;
            }

            if (result is Task task)
            {
                await task;
            }
        }

        /// <summary>
        /// Invoca um método privado assíncrono que retorna <see cref="Task{T}"/> aguardando seu resultado tipado.
        /// </summary>
        /// <typeparam name="T">Tipo de retorno esperado.</typeparam>
        /// <param name="obj">Instância do objeto que contém o método privado.</param>
        /// <param name="methodName">Nome do método privado a ser executado.</param>
        /// <param name="parameters">Parâmetros passados para o método.</param>
        /// <returns>O valor retornado pela tarefa assíncrona.</returns>
        public static async Task<T?> InvokePrivateMethodAsync<T>(object obj, string methodName, params object[] parameters)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("O nome do método não pode ser vazio.", nameof(methodName));

            Type type = obj.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new MissingMethodException($"Método '{methodName}' não encontrado na classe '{type.FullName}'.");

            object? result;
            try
            {
                result = methodInfo.Invoke(obj, parameters);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return default;
            }

            if (result is Task<T> taskOfT)
            {
                return await taskOfT;
            }

            if (result is Task task)
            {
                await task;
                return default;
            }

            return result is null ? default : (T)result;
        }
    }
}
