using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace Audiosort.Plugins
{
    class PluginManager
    {
        public static List<string> Directories = new List<string>();
        public static List<Style> Styles = new List<Style>();

        static System.Windows.Window Window;

        static PluginManager()
        {
            //Style empty = new Style(null);
            //empty.Name = "<kein>";
            //Styles.Add(empty);
        }

        public static void SetWindow(System.Windows.Window window)
        {
            Window = window;
        }

        public static void Search()
        {
            foreach (string Directory in Directories)
            {
                DirectoryInfo dir = new DirectoryInfo(Directory);
                if (!dir.Exists) continue;

                IEnumerable<FileInfo> files = dir.EnumerateFiles();
                foreach (FileInfo file in files)
                {
                    if (file.Extension == ".dll")
                    {
                        try
                        {
                            Assembly assembly = Assembly.LoadFrom(file.DirectoryName + "\\" + file.Name);
                            Type PluginInformation = assembly.GetType("Audiosort.Plugins.PluginInformation");
                            string type = PluginInformation.GetMethod("GetType").Invoke(null, null) as string;

                            switch (type)
                            {
                                case "Style":
                                    {
                                        Style[] styles = Style.Load(assembly);
                                        Styles.AddRange(styles);
                                        break;
                                    }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }

        public static void InvokeStyle(string name)
        {
            InvokeStyle(GetStyle(name));
        }

        public static void InvokeStyle(Style style)
        {
            if (style != null)
            {
                style.Invoke(Window);
            }
        }

        public static Style GetStyle(string name)
        {
            foreach (Style style in Styles)
            {
                if (style.Name == name)
                {
                    return style;
                }
            }
            return null;
        }
    }
}
