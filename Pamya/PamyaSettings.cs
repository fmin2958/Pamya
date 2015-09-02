using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

namespace Pamya
{
    public sealed class PamyaSettings
    {
        private static readonly Lazy<PamyaSettings> lazy =
            new Lazy<PamyaSettings>(() => new PamyaSettings());

        private Dictionary<String, String> settings = new Dictionary<String, String>();
        public string settings_file;

        public static PamyaSettings Instance { get { return lazy.Value; } }

        private PamyaSettings()
        {
            //Load resource settings (defaults)
            Stream resource = GetType().Assembly.GetManifestResourceStream("Pamya.Resources.settings.xml");
            GetSettingsFromText((new StreamReader(resource)).ReadToEnd());
        }

        public void SetSettingsFile(string settings_file)
        {
            this.settings_file = settings_file;
        }

        public string GetSetting(string setting)
        {
            if (settings.ContainsKey(setting))
            {
                string value = settings[setting];
                return value;
            }
            else
            {
                return "";
            }
        }

        public void SaveSettings()
        {
            XElement el = new XElement("settings",
                settings.Select(kv => new XElement(kv.Key, kv.Value)));
            File.WriteAllText(settings_file, el.ToString());
        }

        public void GetSettings()
        {
            if (File.Exists(settings_file))
            {
                string xml = File.ReadAllText(settings_file);
                GetSettingsFromText(xml);
            }
        }

        private void GetSettingsFromText(string xml)
        {
            XElement rootElement = XElement.Parse(xml);
            foreach (var el in rootElement.Elements())
            {
                var setting_name = el.Name.LocalName;
                var setting_value = el.Value;
                if (settings.ContainsKey(setting_name))
                    settings[setting_name] = setting_value;
                else
                    settings.Add(setting_name, setting_value);
            }
        }
    }
}
