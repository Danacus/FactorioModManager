using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Mod_Manager
{
    public class ModPack
    {
        public string name { get; set; }
        public bool enabled { get; set; }
        public List<Mod> mods;

        public ModPack()
        {
        }

        public ModPack(string name)
        {
            this.name = name;
        }

        public ModPack(string name, List<Mod> mods)
        {
            this.name = name; this.mods = mods;
        }

        public void Uninstall()
        {
            if (File.Exists(StaticVar.gameFolder + "modpacks/" + name + ".json"))
                File.Delete(StaticVar.gameFolder + "modpacks/" + name + ".json");
        }

        public void Save()
        {
            List<SimpleMod> mods2 = new List<SimpleMod>();

            foreach (Mod m in mods)
            {
                mods2.Add(new SimpleMod(m.name));
            }

            SimpleModPack mp = new SimpleModPack();
            mp.name = name;
            mp.mods = mods2;

            string json = JsonConvert.SerializeObject(mp, Formatting.Indented);
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/" + name + ".json", json);
        }

        public void AddMods(List<Mod> newMods)
        {
            foreach (Mod m in newMods)
            {
                if (!mods.Contains(m))
                    mods.Add(m);
            }
        }
    }

    public class SimpleModPack
    {
        public string name;
        public List<SimpleMod> mods;
    }
}