using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Mod_Manager
{
    public class Mod
    {
        public string title { get; set; }
        public string name { get; set; }
        public Version localVersion { get; set; }
        public Version onlineVersion { get; set; }
        public Version factorioVersion { get; set; }
        public int downloads { get; set; }
        public string url;
        public bool installed { get; set; }
        public bool enabled { get; set; }
        public string status { get; set; }

        public Mod(string title, string name, Version localVersion, Version onlineVersion, Version factorioVersion, int downloads, string url, bool installed, bool enabled, string status)
        {
            this.title = title;
            this.name = name;
            this.localVersion = localVersion;
            this.onlineVersion = onlineVersion;
            this.factorioVersion = factorioVersion;
            this.downloads = downloads;
            this.url = url;
            this.enabled = enabled;
            this.installed = installed;
            this.status = status;
        }

        public string FilePath()
        {
            return String.Format(
                "{0}mods/{1}_{2}.zip",
                StaticVar.gameFolder,
                name,
                localVersion
            );
        }

        public bool Exists()
        {
            return File.Exists(FilePath());
        }

        public void Delete()
        {
            File.Delete(FilePath());
        }

        public void Uninstall()
        {
            if (installed)
            {
                Delete();
                installed = false;
            }
        }

        public void Enable()
        {
            if (!enabled)
                enabled = true;
        }

        public void Disable()
        {
            if (enabled)
                enabled = false;
        }
    }

    public class SimpleMod
    {
        public string name;

        public SimpleMod(string n)
        {
            this.name = n;
        }
    }
}