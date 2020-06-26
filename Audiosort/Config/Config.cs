using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Audiosort.Config
{
    class Config
    {
        AudiosortConfig config;
        string Filename;

        public Config(string filename)
        {
            Filename = filename;
            config = new AudiosortConfig();

            try
            {
                config.ReadXml(Filename);
            }
            catch (Exception)
            {
                Create();
            }
        }

        public void Save()
        {
            config.WriteXml(Filename);
        }

        public string this[string key]
        {
            get
            {
                var result = from c in config.Config
                             where c.config_name == key
                             select c.config_value;
                if (result.Count() > 0)
                    return result.ElementAt(0);
                return "";
            }
            set
            {
                var result = from c in config.Config
                             where c.config_name == key
                             select c;
                if (result.Count() > 0)
                {
                    AudiosortConfig.ConfigRow row = result.ElementAt(0);
                    row.config_value = value;
                }
                else
                {
                    config.Config.AddConfigRow(key, value);
                }
            }
        }

        void Create()
        {
            this["DefaultStyle"] = "Standard";
            this["PluginDirectories"] = "Styles;Plugins";
            this["DatabasePath"] = "data.xml";
        }
    }
}
