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
    public partial class ModPacks : Form
    {
        public static List<ModPack> modPacks = new List<ModPack>();
        public Main main;

        public ModPacks()
        {
            InitializeComponent();
            LoadModPacks();
            CreateList();
        }

        public void LoadModPacks()
        {
            modPacks.Clear();

            foreach (string f in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/"))
            {
                dynamic mods = JsonConvert.DeserializeObject<List<ModPackItem>>(File.ReadAllText(f));

                bool enabled = true;

                foreach (ModPackItem m in (List<ModPackItem>)mods)
                {
                    bool isEnabled = false;

                    foreach (Mod mod in Main.userData.installedMods)
                    {
                        if (mod.title == m.name && mod.enabled)
                            isEnabled = true;
                    }

                    if (isEnabled == false)
                        enabled = false;
                }

                modPacks.Add(new ModPack(f.Replace(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/", "").Replace(".json", ""), enabled, (List<ModPackItem>)mods));
            }
        }

        public void CreateList()
        {
            objectListView1.SetObjects(modPacks);
        }

        private void enableButton_Click(object sender, EventArgs e)
        {
            EnableMods();
        }

        public List<ModPack> selected = new List<ModPack>();

        public void EnableMods()
        {
            Console.WriteLine("Enable mods ...");

            List<ModPack> list = new List<ModPack>();
            list = objectListView1.SelectedObjects.Cast<ModPack>().ToList();
            List<ModPackItem> downloads = new List<ModPackItem>();

            if (list.Count > 0)
                selected = list;
            else
                list = selected;

            foreach (ModPack mp in list)
            {
                foreach (ModPackItem m in mp.mods)
                {

                    bool downloaded = false;
                    foreach (Mod mod in Main.userData.installedMods)
                    {

                        if (mod.title == m.name)
                        {
                            Console.WriteLine("Enable mod: " + mod.title);
                            mod.enabled = true;
                            downloaded = true;
                        }
                    }

                    if (!downloaded)
                        downloads.Add(m);
                }
            }

            if (downloads.Count > 0)
            {
                Form1 form = new Form1();
                //form.Show();
                form.main = main;
                form.DownloadModPack(downloads, this);
            }

            main.CreateList();
            main.SaveModList();
            LoadModPacks();
            CreateList();
        }

        public static ModPack selectedModPack;

        private void button1_Click(object sender, EventArgs e)
        {

            List<ModPack> list = new List<ModPack>();
            list = objectListView1.SelectedObjects.Cast<ModPack>().ToList();
            ModPack m = list[0];
            selectedModPack = m;
            EditModPack emp = new EditModPack();
            emp.Show();

        }

        private void disableButton_Click(object sender, EventArgs e)
        {

        }
    }

    public class ModPack
    {
        public string title { get; set; }
        public bool enabled { get; set; }
        public List<ModPackItem> mods { get; set; }

        public ModPack(string title, bool enabled, List<ModPackItem> mods)
        {
            this.title = title;
            this.enabled = enabled;
            this.mods = mods;
        }
    }

    public class ModPackItem
    {
        public string name;
    }
}