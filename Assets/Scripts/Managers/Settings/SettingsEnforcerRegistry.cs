using System;
using System.Collections.Generic;
using System.Linq;

namespace Managers.Settings
{
    public static class SettingEnforcerRegistry
    {
        private static readonly List<ISettingEnforcer> Implementations = new();


        public static IReadOnlyList<ISettingEnforcer> Enforcers =>
            Implementations;


        public static void Initialize()
        {
            Implementations.Clear();


            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract ||
                        type.IsInterface)
                        continue;


                    if (!typeof(ISettingEnforcer)
                            .IsAssignableFrom(type))
                        continue;


                    if (Activator.CreateInstance(type) is ISettingEnforcer instance)
                        Implementations.Add(instance);
                }
            }
        }


        public static ISettingEnforcer Get(string key)
        {
            return Implementations
                .FirstOrDefault(x =>
                    x.GetKey() == key);
        }
    }
}