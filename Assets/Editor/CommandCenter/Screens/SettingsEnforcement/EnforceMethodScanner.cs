using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Editor.CommandCenter.Screens.SettingsEnforcement
{
    public static class EnforceMethodScanner
    {
        public static bool HasImplementation(Type type)
        {
            var method =
                type.GetMethod(
                    "Enforce",
                    BindingFlags.Public |
                    BindingFlags.Instance);


            if (method == null)
                return false;


            var file =
                FindSourceFile(type);


            if (string.IsNullOrEmpty(file))
                return true;


            var source =
                File.ReadAllText(file);


            var methodName =
                "void " + method.Name;


            var start =
                source.IndexOf(methodName,
                    StringComparison.Ordinal);


            if (start < 0)
                return true;


            var open =
                source.IndexOf(
                    "{",
                    start, StringComparison.Ordinal);


            var close =
                source.IndexOf(
                    "}",
                    open, StringComparison.Ordinal);


            if (open < 0 || close < 0)
                return true;


            var body =
                source.Substring(
                    open + 1,
                    close - open - 1)
                .Trim();


            return !string.IsNullOrEmpty(body);
        }


        private static string FindSourceFile(Type type)
        {
            if (type == null)
                return null;


            var scriptName = type.Name + ".cs";


            var assetsPath =
                Path.GetFullPath("Assets");


            if (!Directory.Exists(assetsPath))
                return null;


            var files =
                Directory.GetFiles(
                    assetsPath,
                    scriptName,
                    SearchOption.AllDirectories);


            return files.FirstOrDefault();
        }
    }
}