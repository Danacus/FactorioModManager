using BrightIdeasSoftware;
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

    public partial class Main : Form
    {
        public static UserData userData = new UserData();

        public Main()
        {
            InitializeComponent();
            userData.installedMods = new List<Mod>();
            LoadUserData();
            LoadMods();
            CreateList();
            LoadModPacks();
        }

        public void LoadModPacks()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/");

        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            form.Show();
            form.main = this;
        }

        public void CreateList()
        {
            Console.WriteLine("Creating list");/*
            OLVColumn titleColumn = new OLVColumn("Title", "title");
            OLVColumn versionColumn = new OLVColumn("Version", "version");
            OLVColumn enabledColumn = new OLVColumn("Enabled", "enabled");

            objectListView1.AllColumns.Add(titleColumn);
            objectListView1.AllColumns.Add(versionColumn);
            objectListView1.AllColumns.Add(enabledColumn);

            titleColumn.FillsFreeSpace = true;
            versionColumn.FillsFreeSpace = true;
            enabledColumn.FillsFreeSpace = true;
            */
            List<ModObjectListItem> masterList = new List<ModObjectListItem>();
            //masterList.Add(new ModObjectListItem("a", "b", true));

            foreach (Mod m in userData.installedMods)
            {
                masterList.Add(new ModObjectListItem(m.title, m.version, m.enabled));
            }

            //Adding the object list to the Objectlistview
            objectListView1.SetObjects(masterList);
        }

        public void LoadUserData()
        {
            string s = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/player-data.json");
            s = s.Replace("-", "_");
            dynamic data = JsonConvert.DeserializeObject(s);
            userData.username = (string)data.service_username;
            userData.token = (string)data.service_token;

            Console.WriteLine(string.Format("username = {0}, token = {1}", userData.username, userData.token));
        }

        public void LoadMods()
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/mod-list.json"))
            {
                string s = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/mod-list.json");
                dynamic data = JsonConvert.DeserializeObject(s);

                userData.installedMods.Clear();

                foreach (dynamic d in data.mods)
                {
                    Mod m = new Mod();
                    m.title = (string)d.name;
                    m.enabled = (bool)d.enabled;
                    userData.installedMods.Add(m);
                }
            }

            LoadModZips();
        }

        public void LoadModZips()
        {
            foreach (string f in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/"))
            {
                if (f.Contains(".zip"))
                {
                    string name = f.Split('/')[f.Split('/').Length - 1];
                    name = name.Replace(".zip", "");
                    Mod m = new Mod();
                    m.version = name.Split('_')[name.Split('_').Length - 1];
                    m.title = name.Replace("_" + m.version, "");

                    m.enabled = false;

                    bool ok = false;

                    foreach (Mod mod in userData.installedMods)
                    {
                        if (mod.title == m.title)
                            ok = true;
                    }

                    if (!ok)
                        userData.installedMods.Add(m);
                    else
                    {
                        foreach (Mod mod in userData.installedMods)
                        {
                            if (mod.title == m.title)
                                mod.version = m.version;
                        }
                    }
                }
            }

            foreach (Mod mod in userData.installedMods.ToArray())
            {
                if (mod.version == null)
                {
                    userData.installedMods.Remove(mod);
                }
            }

            SaveModList();
        }

        public void SaveModList()
        {
            List<ModListItem> mods = new List<ModListItem>();

            bool baseok = false;

            foreach (Mod m in userData.installedMods)
            {
                if (m.title == "base")
                {
                    baseok = true;
                }
            }

            if (!baseok)
            {
                ModListItem b = new ModListItem();
                b.name = "base";
                b.enabled = "true";
                mods.Add(b);
            }

            foreach (Mod m in userData.installedMods)
            {
                ModListItem i = new ModListItem();
                i.name = m.title;
                i.enabled = m.enabled.ToString().ToLower();
                mods.Add(i);
            }

            ModList modList = new ModList();
            modList.mods = mods.ToArray();

            string file = JsonConvert.SerializeObject(modList, Formatting.Indented);

            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/mod-list.json", file);

        }

        private void enableButton_Click(object sender, EventArgs e)
        {
            foreach (ModObjectListItem i in objectListView1.SelectedObjects)
            {
                foreach (Mod m in userData.installedMods)
                {
                    if (m.title == i.title)
                    {
                        m.enabled = true;
                    }
                }

            }

            CreateList();
            SaveModList();
        }

        private void disableButton_Click(object sender, EventArgs e)
        {
            foreach (ModObjectListItem i in objectListView1.SelectedObjects)
            {
                foreach (Mod m in userData.installedMods)
                {
                    if (m.title == i.title)
                    {
                        m.enabled = false;
                    }
                }

            }

            CreateList();
            SaveModList();
        }

        private void modPacksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModPacks mp = new ModPacks();
            mp.Show();
            mp.main = this;
        }
    }

    public class ModObjectListItem
    {

        public string title { get; set; }
        public string version { get; set; }
        public bool enabled { get; set; }

        public ModObjectListItem(string title, string version, bool enabled)
        {
            this.title = title;
            this.version = version;
            this.enabled = enabled;
        }
    }

    public class UserData
    {
        public string username;
        public string token;
        public List<Mod> installedMods;
    }

    public class Mod
    {
        public string title;
        public string version;
        public bool enabled;
    }

    public class ModList
    {
        public ModListItem[] mods;
    }

    public class ModListItem
    {
        public string name;
        public string enabled;
    }
}