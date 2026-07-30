using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Utility
{
    public static class Locator
    {
        private static readonly Dictionary<Type, object> services = new();
        
        public static void Register<T>(T service)
        {
            services[typeof(T)] = service;
        }

        public static T Get<T>()
        {
            return (T)services[typeof(T)];
        }
    }
}
