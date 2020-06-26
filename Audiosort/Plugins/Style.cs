using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Audiosort.Plugins
{
    public class Style
    {
        const string Namespace = "Audiosort.Plugins";

        public string Name;
        public string Author;
        public string Homepage;
        public string Version;

        public string XamlDirectory = "";
        public string[] XamlPaths;

        protected Assembly Assembly;
        protected string AssemblyName;

        public Style(Assembly a)
        {
            Assembly = a;
        }

        public void Invoke(System.Windows.Window Window)
        {
            Window.Resources.Clear();
            if (XamlPaths == null)
                return;

            foreach (string XamlPath in XamlPaths)
            {
                string path;
                if (XamlDirectory != "")
                    path = Namespace + "." + XamlDirectory.Replace('/', '.') + "." + XamlPath;
                else
                    path = Namespace + "." + XamlPath;
                Stream s = Assembly.GetManifestResourceStream(path);
                if (s == null)
                    continue;

                System.Windows.ResourceDictionary resourceDictionary =
                    (System.Windows.ResourceDictionary)System.Windows.Markup.XamlReader.Load(s);
                Window.Resources.MergedDictionaries.Add(resourceDictionary);
                s.Close();
            }
        }

        public static Style[] Load(string AssemblyName)
        {
            if (!File.Exists(AssemblyName))
                return null;

            Assembly assembly = Assembly.LoadFrom(AssemblyName);

            return Load(assembly);
        }

        public static Style[] Load(Assembly assembly)
        {
            Type PluginInformation = assembly.GetType(Namespace + ".PluginInformation");
            string type = PluginInformation.GetMethod("GetType").Invoke(null, null) as string;
            if (type != "Style")
                return null;

            Type StyleInformation = assembly.GetType(Namespace + ".StyleInformation");
            string[] styles = StyleInformation.GetMethod("GetStyles").Invoke(null, null) as string[];

            Style[] Result = new Style[styles.Length];
            for (int i = 0; i < styles.Length; i++)
            {
                Type style = assembly.GetType(Namespace + "." + styles[i]);
                Result[i] = style.GetConstructor(new Type[] { typeof(Assembly) }).Invoke(new object[] { assembly }) as Style;
            }

            return Result;
        }
    }
}
