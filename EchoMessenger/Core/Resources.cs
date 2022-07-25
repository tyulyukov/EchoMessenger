using System;
using System.Windows;

namespace EchoMessenger.Core
{
    public static class Resources
    {
        public static T Find<T>(String resourceName)
        {
            var resource = Application.Current.Resources[resourceName];

            if (resource == null)
                throw new ArgumentException($"{resourceName} is not present in the resources dictionary.", nameof(resourceName));

            if (resource is T)
                return (T)resource;

            throw new InvalidCastException($"Can not cast a {nameof(resource)} to {nameof(T)}");
        }
    }
}
