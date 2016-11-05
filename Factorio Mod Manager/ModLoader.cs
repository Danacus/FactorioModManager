using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factorio_Mod_Manager
{
    public class ModLoader
    {
        public List<Mod> LoadMods()
        {
            List<Mod> installedMods = new List<Mod>();

            if (File.Exists(StaticVar.gameFolder + "mods/mod-list.json"))
            {
                string s = File.ReadAllText(StaticVar.gameFolder + "mods/mod-list.json");
                dynamic data = JsonConvert.DeserializeObject(s);

                installedMods.Clear();

                foreach (dynamic d in data.mods)
                {
                    Mod m = new Mod(null, (string)d.name, null, null, null, 0, null, true, (bool)d.enabled, null);
                    installedMods.Add(m);
                }
            }

            return LoadModZips(installedMods);
        }

        public List<Mod> LoadModZips(List<Mod> installedMods)
        {
            foreach (string f in Directory.GetFiles(StaticVar.gameFolder + "mods/"))
            {
                if (f.Contains(".zip"))
                {
                    string name = f.Split('/')[f.Split('/').Length - 1];
                    name = name.Replace(".zip", "");
                    Mod m = new Mod(null, name.Replace("_" + name.Split('_')[name.Split('_').Length - 1], ""), new Version(name.Split('_')[name.Split('_').Length - 1]), null, null, 0, null, true, false, null);

                    bool ok = false;

                    foreach (Mod mod in installedMods)
                    {
                        if (mod.name == m.name)
                            ok = true;
                    }

                    if (!ok)
                        installedMods.Add(m);
                    else
                    {
                        foreach (Mod mod in installedMods)
                        {
                            if (mod.name == m.name)
                                mod.localVersion = m.localVersion;
                        }
                    }
                }
            }

            foreach (Mod mod in installedMods.ToArray())
            {
                if (mod.localVersion == null)
                {
                    installedMods.Remove(mod);
                }
            }
            SaveModList(installedMods);

            return installedMods;

        }

        public void SaveModList(List<Mod> installedMods)
        {
            List<Mod> mods = new List<Mod>();

            bool baseok = false;

            foreach (Mod m in installedMods)
            {
                if (m.name == "base")
                {
                    baseok = true;
                }
            }

            if (!baseok)
            {
                Mod b = new Mod(null, "base", null, null, null, 0, null, true, true, null);
                installedMods.Add(b);
            }

            List<ModListItem> items = new List<ModListItem>();

            foreach (Mod m in installedMods)
            {
                ModListItem i = new ModListItem();
                i.name = m.name;
                i.enabled = m.enabled.ToString().ToLower();
                items.Add(i);
            }

            ModList modList = new ModList();
            modList.mods = items.ToArray();

            string file = JsonConvert.SerializeObject(modList, Formatting.Indented);

            File.WriteAllText(StaticVar.gameFolder + "mods/mod-list.json", file);

        }

        public List<Mod> LoadOnlineMods(bool online)
        {
            List<Mod> onlineModList = new List<Mod>();

            string info;

            if (!online)
                info = File.ReadAllText(StaticVar.gameFolder + "ModManagerCache.json");
            else
                info = DownloadModsInfo();

            dynamic data = JsonConvert.DeserializeObject(info);

            foreach (dynamic d in data.results)
            {
                Mod m = new Mod((string)d.latest_release.info_json.title, (string)d.latest_release.info_json.name, null, new Version((string)d.latest_release.info_json.version), new Version((string)d.latest_release.info_json.factorio_version), (int)d.downloads_count, (string)d.latest_release.download_url, false, false, null);
                onlineModList.Add(m);
            }

            File.WriteAllText(StaticVar.gameFolder + "ModManagerCache.json", info);

            return onlineModList;
        }

        public string DownloadModsInfo()
        {
            try
            {
                int size = 10000;

                string s = new WebClient().DownloadString("https://mods.factorio.com/api/mods?order=top&page_size=" + size);
                //Console.WriteLine(s);
                return s;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.GetType().ToString());
                return null;
            }
        }

        public List<Mod> CreateFinalModList(List<Mod> installedMods, List<Mod> onlineModList)
        {
            List<Mod> finalModList = new List<Mod>();

            foreach (Mod im in installedMods)
            {
                foreach (Mod om in onlineModList)
                {
                    if (om.name == im.name)
                    {
                        Mod m = new Mod(om.title, om.name, im.localVersion, om.onlineVersion, om.factorioVersion, om.downloads, om.url, im.installed, im.enabled, null);
                        m.status = (im.localVersion.CompareTo(om.onlineVersion) >= 0) ? "Up-To-Date" : "Update Available";
                        finalModList.Add(m);
                    }
                }
            }
            foreach (Mod im in installedMods)
            {
                bool inList = false;

                foreach (Mod m in finalModList)
                {
                    if (m.name == im.name)
                        inList = true;
                }

                if (!inList)
                    finalModList.Add(im);
            }

            foreach (Mod om in onlineModList)
            {
                bool inList = false;

                foreach (Mod m in finalModList)
                {

                    if (m.name == om.name)
                        inList = true;
                }

                if (!inList)
                    finalModList.Add(om);
            }

            return finalModList;
        }
    }
}