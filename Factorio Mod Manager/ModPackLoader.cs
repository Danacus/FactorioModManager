using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Mod_Manager
{
    public class ModPackLoader
    {
        public List<ModPack> LoadModPacks(List<Mod> finalModList)
        {
            List<ModPack> modPacks = new List<ModPack>();

            if (!Directory.Exists(StaticVar.gameFolder + "modpacks/"))
                Directory.CreateDirectory(StaticVar.gameFolder + "modpacks/");

            foreach (string file in Directory.GetFiles(StaticVar.gameFolder + "modpacks/"))
            {

                if (!file.Contains(".json")) break;

                Console.WriteLine("Deserializing Modpack");
                dynamic d = JsonConvert.DeserializeObject(File.ReadAllText(file));

                List<Mod> m = new List<Mod>();

                bool enabled = true;

                foreach (dynamic item in d.mods)
                {
                    if (item == null) break;

                    bool ok = false;

                    foreach (Mod mod in finalModList)
                    {

                        if (mod.name == (string)item.name)
                        {
                            Console.WriteLine("Adding Mod");
                            m.Add(mod);
                            ok = true;
                            if (!mod.enabled)
                                enabled = false;
                        }
                    }

                    if (!ok)
                    {
                        m.Add(new Mod(null, (string)item.name, null, null, null, 0, null, false, false, null));
                        enabled = false;
                    }

                }

                ModPack mp = new ModPack((string)d.name, m);
                mp.enabled = enabled;
                modPacks.Add(mp);

            }

            return modPacks;
        }
    }
}