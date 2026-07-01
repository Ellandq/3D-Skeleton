using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Settings;

namespace Editor.CommandCenter.Screens.SettingsEnforcement
{
    public class SettingsEnforcerInfo
    {
        public Type type;
        public string key;
        public Type valueType;
        public bool hasImplementation;
    }


    public static class SettingsEnforcerScanner
    {
        private static readonly List<SettingsEnforcerInfo> Cache = new();

        public static IReadOnlyList<SettingsEnforcerInfo> Enforcers => Cache;


        public static void Refresh()
        {
            Cache.Clear();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }


                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;


                    var interfaceType =
                        type.GetInterfaces()
                            .FirstOrDefault(x =>
                                x.IsGenericType &&
                                x.GetGenericTypeDefinition() ==
                                typeof(ISettingEnforcer<>));


                    if (interfaceType == null)
                        continue;


                    var keyMethod =
                        type.GetMethod(
                            "GetKey",
                            BindingFlags.Public |
                            BindingFlags.Instance);


                    if (keyMethod == null)
                        continue;


                    var key =
                        keyMethod.Invoke(
                            Activator.CreateInstance(type),
                            null)
                        as string;


                    if (string.IsNullOrEmpty(key))
                        continue;


                    Cache.Add(
                        new SettingsEnforcerInfo
                        {
                            type = type,
                            key = key,
                            valueType =
                                interfaceType
                                .GetGenericArguments()[0],

                            hasImplementation =
                                EnforceMethodScanner.HasImplementation(type)
                        });
                }
            }
        }
    }
}