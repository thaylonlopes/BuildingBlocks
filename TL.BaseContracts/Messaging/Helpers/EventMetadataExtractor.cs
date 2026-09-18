using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using TL.BaseContracts.Messaging.Attributes;

namespace TL.BaseContracts.Messaging.Helpers
{
    /// <summary>
    /// Utilitário de alta performance com cache thread-safe O(1) para extração de metadados declarativos
    /// de eventos (chaves de partição, tópicos e identificadores de mensagem).
    /// </summary>
    public static class EventMetadataExtractor
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> PartitionKeyCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> MessageIdCache = new();
        private static readonly ConcurrentDictionary<Type, string> TopicNameCache = new();

        /// <summary>
        /// Extrai o valor textual da chave de partição da propriedade anotada com <see cref="PartitionKeyAttribute"/>.
        /// </summary>
        /// <param name="message">A instância da mensagem de evento.</param>
        /// <returns>O valor da partition key ou <c>null</c> se ausente.</returns>
        public static string? ExtractPartitionKey(object? message)
        {
            if (message == null)
            {
                return null;
            }

            var prop = PartitionKeyCache.GetOrAdd(message.GetType(), ResolvePartitionKeyProperty);
            if (prop == null)
            {
                return null;
            }

            var value = prop.GetValue(message);
            return value?.ToString();
        }

        /// <summary>
        /// Extrai o identificador determinístico da mensagem a partir da interface <see cref="IIntegrationEvent"/>
        /// ou da propriedade anotada com <see cref="MessageIdAttribute"/>.
        /// </summary>
        /// <param name="message">A instância da mensagem de evento.</param>
        /// <returns>O identificador da mensagem formatado em string ou <c>null</c> se ausente.</returns>
        public static string? ExtractMessageId(object? message)
        {
            if (message == null)
            {
                return null;
            }

            if (message is IIntegrationEvent integrationEvent)
            {
                return integrationEvent.EventId.ToString();
            }

            var prop = MessageIdCache.GetOrAdd(message.GetType(), ResolveMessageIdProperty);
            if (prop == null)
            {
                return null;
            }

            var value = prop.GetValue(message);
            return value?.ToString();
        }

        /// <summary>
        /// Obtém o nome do tópico, exchange ou destino para o tipo de evento fornecido,
        /// inspecionando o atributo <see cref="TopicAttribute"/> ou aplicando convenção kebab-case padrão.
        /// </summary>
        /// <typeparam name="T">O tipo do evento.</typeparam>
        /// <returns>O nome do tópico/exchange de destino.</returns>
        public static string GetTopicName<T>() where T : class
        {
            return GetTopicName(typeof(T));
        }

        /// <summary>
        /// Obtém o nome do tópico, exchange ou destino para o tipo fornecido.
        /// </summary>
        /// <param name="type">O tipo do evento.</param>
        /// <returns>O nome do tópico/exchange de destino.</returns>
        public static string GetTopicName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return TopicNameCache.GetOrAdd(type, ResolveTopicName);
        }

        /// <summary>
        /// Converte uma cadeia de caracteres em formato PascalCase para kebab-case.
        /// </summary>
        /// <param name="value">Texto original.</param>
        /// <returns>Texto em kebab-case.</returns>
        public static string ToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var builder = new StringBuilder(value.Length + 8);
            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];
                if (char.IsUpper(current))
                {
                    AppendHyphenIfNeeded(builder, value, index);
                    builder.Append(char.ToLowerInvariant(current));
                }
                else
                {
                    builder.Append(current);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Limpa os caches de reflexão em memória (útil para cenários de testes).
        /// </summary>
        public static void ClearCache()
        {
            PartitionKeyCache.Clear();
            MessageIdCache.Clear();
            TopicNameCache.Clear();
        }

        private static PropertyInfo? ResolvePartitionKeyProperty(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo? found = null;

            foreach (var prop in properties)
            {
                if (prop.IsDefined(typeof(PartitionKeyAttribute), inherit: true))
                {
                    if (found != null)
                    {
                        throw new InvalidOperationException(
                            $"O tipo '{type.FullName}' possui múltiplas propriedades decoradas com [PartitionKey]: '{found.Name}' e '{prop.Name}'. Apenas uma é permitida.");
                    }

                    found = prop;
                }
            }

            return found;
        }

        private static PropertyInfo? ResolveMessageIdProperty(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo? found = null;

            foreach (var prop in properties)
            {
                if (prop.IsDefined(typeof(MessageIdAttribute), inherit: true))
                {
                    if (found != null)
                    {
                        throw new InvalidOperationException(
                            $"O tipo '{type.FullName}' possui múltiplas propriedades decoradas com [MessageId]: '{found.Name}' e '{prop.Name}'. Apenas uma é permitida.");
                    }

                    found = prop;
                }
            }

            return found;
        }

        private static string ResolveTopicName(Type type)
        {
            var topicAttr = type.GetCustomAttribute<TopicAttribute>(inherit: true);
            if (topicAttr != null && !string.IsNullOrWhiteSpace(topicAttr.Name))
            {
                return topicAttr.Name;
            }

            string typeName = RemoveEventSuffix(type.Name);
            return ToKebabCase(typeName);
        }

        private static string RemoveEventSuffix(string name)
        {
            if (name.EndsWith("Event", StringComparison.Ordinal) && name.Length > 5)
            {
                return name.Substring(0, name.Length - 5);
            }

            if (name.EndsWith("Message", StringComparison.Ordinal) && name.Length > 7)
            {
                return name.Substring(0, name.Length - 7);
            }

            return name;
        }

        private static void AppendHyphenIfNeeded(StringBuilder builder, string value, int index)
        {
            if (index <= 0)
            {
                return;
            }

            char previous = value[index - 1];
            bool precededByLowerOrDigit = char.IsLower(previous) || char.IsDigit(previous);
            bool followedByLower = (index + 1 < value.Length) && char.IsLower(value[index + 1]);

            if (precededByLowerOrDigit || followedByLower)
            {
                builder.Append('-');
            }
        }
    }
}
