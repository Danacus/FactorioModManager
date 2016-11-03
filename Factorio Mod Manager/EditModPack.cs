using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factorio_Mod_Manager
{
    public partial class EditModPack : Form
    {

        public EditModPack()
        {
            InitializeComponent();
            LoadInstalledMods();
            LoadModPackMods();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddToModPack();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RemoveFromModpack();
        }

        public void LoadInstalledMods()
        {
            List<ModObjectListItem> list = new List<ModObjectListItem>();
            List<ModPackItem> mods = ModPacks.selectedModPack.mods;
            foreach (Mod m in Main.userData.installedMods)
            {
                bool inModPack = false;

                foreach (ModPackItem i in mods)
                {
                    if (i.name == m.title)
                        inModPack = true;
                }

                if (!inModPack)
                    list.Add(new ModObjectListItem(m.title, m.version, m.enabled));
            }

            objectListView1.SetObjects(list);
        }

        public void LoadModPackMods()
        {
            List<ModObjectListItem> list = new List<ModObjectListItem>();
            List<ModPackItem> mods = ModPacks.selectedModPack.mods;
            foreach (Mod m in Main.userData.installedMods)
            {
                bool inModPack = false;

                foreach (ModPackItem i in mods)
                {
                    if (i.name == m.title)
                        inModPack = true;
                }

                if (inModPack)
                    list.Add(new ModObjectListItem(m.title, m.version, m.enabled));
            }

            objectListView2.SetObjects(list);
        }

        public void AddToModPack()
        {
            List<ModObjectListItem> list = new List<ModObjectListItem>();
            list = objectListView1.SelectedObjects.Cast<ModObjectListItem>().ToList();

            List<ModPackItem> mods = ModPacks.selectedModPack.mods;

            foreach (ModObjectListItem i in list)
            {

                ModPackItem mpi = new ModPackItem();
                mpi.name = i.title;
                ModPacks.selectedModPack.mods.Add(mpi);

            }

            SaveModPack();
            LoadInstalledMods();
            LoadModPackMods();
        }

        public void RemoveFromModpack()
        {
            List<ModObjectListItem> list = new List<ModObjectListItem>();
            list = objectListView2.SelectedObjects.Cast<ModObjectListItem>().ToList();

            List<ModPackItem> mods = ModPacks.selectedModPack.mods;

            foreach (ModObjectListItem i in list)
            {

                foreach (ModPackItem mpi in ModPacks.selectedModPack.mods.ToArray())
                {
                    if (mpi.name == i.title)
                        ModPacks.selectedModPack.mods.Remove(mpi);
                }

            }

            SaveModPack();
            LoadInstalledMods();
            LoadModPackMods();
        }

        public void SaveModPack()
        {
            string file = JsonConvert.SerializeObject(ModPacks.selectedModPack.mods, Formatting.Indented);

            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/" + ModPacks.selectedModPack.title + ".json", file);
        }
    }
}