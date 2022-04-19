using System;

namespace OpenUGD.ECS.Utilities
{
    public static class Contract
    {
        public static void True(bool condition, string message = null)
        {
            if (!condition) throw new Exception(message ?? string.Empty);
        }

        public static void True(bool condition, string formatMessage, params object[] format)
        {
            if (!condition) throw new Exception(string.Format(formatMessage ?? string.Empty, format));
        }

        public static void True(bool condition, Func<string> message)
        {
            if (!condition) throw new Exception(message?.Invoke() ?? string.Empty);
        }

        public static void False(bool condition, string message = null)
        {
            if (condition) throw new Exception(message ?? string.Empty);
        }

        public static void False(bool condition, string formatMessage, params object[] format)
        {
            if (condition) throw new Exception(string.Format(formatMessage ?? string.Empty, format));
        }

        public static void False(bool condition, Func<string> message)
        {
            if (condition) throw new Exception(message?.Invoke() ?? string.Empty);
        }

        public static void NotNull(object value, string message = null)
        {
            if (value == null) throw new Exception(message ?? string.Empty);
        }

        public static void NotNull(object value, string formatMessage, params object[] format)
        {
            if (value == null) throw new Exception(string.Format(formatMessage ?? string.Empty, format));
        }

        public static void NotNull(object value, Func<string> message)
        {
            if (value == null) throw new Exception(message?.Invoke() ?? string.Empty);
        }

        public static void IsNull(object value, string message = null)
        {
            if (value != null) throw new Exception(message ?? string.Empty);
        }

        public static void IsNull(object value, Func<string> message)
        {
            if (value != null) throw new Exception(message?.Invoke() ?? string.Empty);
        }

        public static void IsImplementInterface(Type type, Type interfaceType, string message = null)
        {
            if (!interfaceType.IsAssignableFrom(type)) throw new Exception(message ?? string.Empty);
        }

        public static void IsValueType(Type type, string message = null)
        {
            if (!type.IsValueType) throw new Exception(message ?? string.Empty);
        }

        public static void Throw(string message = null)
        {
            throw new Exception(message ?? string.Empty);
        }
    }
}