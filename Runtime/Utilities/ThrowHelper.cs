using System;

namespace OpenUGD.ECS.Utilities
{
    public static class ThrowHelper
    {
        public static void ThrowArgumentOutOfRangeException(string paramName, ExceptionResource message)
        {
            throw new ArgumentOutOfRangeException(paramName, message.ToString());
        }

        public static void ThrowArgumentNullException(string arg)
        {
            throw new ArgumentNullException(arg);
        }

        public static void ThrowArgumentOutOfRangeException()
        {
            ThrowArgumentOutOfRangeException("index", ExceptionResource.ArgumentOutOfRangeIndex);
        }

        public static void IfNullAndNullsAreIllegalThenThrow<T>(object? value, string argName)
        {
            if (value == null && !(default(T) == null))
                ThrowHelper.ThrowArgumentNullException(argName);
        }

        public static void ThrowWrongValueTypeArgumentException(object value, Type type)
        {
            throw new ArgumentException($"Arg_WrongType, value:{value}:{value?.GetType()}, expectedType:{type}");
        }

        public static void ThrowArgumentException(ExceptionResource resource)
        {
            throw new ArgumentException(resource.ToString());
        }

        public static void ThrowInvalidOperationException(ExceptionResource resource)
        {
            throw new InvalidOperationException(resource.ToString());
        }

        public static object ThrowNotImplemented()
        {
            throw new NotImplementedException();
        }

        public static void ThrowValueExistException()
        {
            throw new InvalidOperationException("value already in the list");
        }
    }
}