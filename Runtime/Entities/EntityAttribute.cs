using System;
using OpenUGD.ECS.Components;
using OpenUGD.ECS.Utilities;

namespace OpenUGD.ECS.Entities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExcludeAttribute : Attribute
    {
        public readonly Type Type;

        public ExcludeAttribute(Type type)
        {
            Contract.IsImplementInterface(type, typeof(IComponent));
            Contract.IsValueType(type);
            Type = type;
        }
    }
}