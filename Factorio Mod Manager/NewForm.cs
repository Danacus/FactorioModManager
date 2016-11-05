using BrightIdeasSoftware;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factorio_Mod_Manager
{

    public partial class NewForm : Form
    {
        public static UserData userData = new UserData();
        public List<Mod> onlineModList = new List<Mod>();
        public List<Mod> finalModList = new List<Mod>();
        public List<Mod> installedMods = new List<Mod>();
        public ModLoader modReader = new ModLoader();
        public ModPackLoader modPackLoader = new ModPackLoader();
        public DownloadManager downloadManager;
        public ExecutableManager executableManager = new ExecutableManager();

        public NewForm()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;

            //Load UserData
            string err = null;
            userData.Load(StaticVar.gameFolder + "player-data.json", out err);
            if (err != null)
            {
                MessageBox.Show(err);
            }

            //Load mods in mod portal
            LoadAllModsOnline(() => { modList.RefreshMods(this); LoadAllModPacks(() => { }); executableManager.LoadExecutables(); SetExecutables(); });
            //LoadAllModsInstalled will finish before LoadAllModsOnline, so the final modlist can be created after loading the online mods
            LoadAllModsInstalled(() => { });
            downloadManager = new DownloadManager(userData);
        }

        public void RefreshMods()
        {
            modReader.SaveModList(installedMods);
            modList.RefreshMods(this);
            LoadAllModPacks(() => { });
        }

        private bool allowshowdisplay = false;

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        }

        public void SetExecutables()
        {
            comboBox1.Items.Clear();

            foreach (Executable e in executableManager.executables)
            {
                comboBox1.Items.Add("Factorio " + e.version);
            }

            comboBox1.Items.Add("Add Executable ...");
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Add Executable ...")
            {
                executableManager.AddExecutable();
                SetExecutables();
            }
        }

        private void launchButton_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null) return;

            if (comboBox1.SelectedItem.ToString() != "Add Executable ...")
            {
                executableManager.RunExecutable(executableManager.executables[comboBox1.Items.IndexOf(comboBox1.SelectedItem)]);
            }
        }

        /// <summary>
        /// Mod Management
        /// </summary>

        public void LoadAllModsInstalled(Action callback)
        {

            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                installedMods = modReader.LoadMods();
            };
            bw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    MessageBox.Show(args.Error.ToString());

                callback();
            };

            bw.RunWorkerAsync();
        }

        public void LoadAllModsOnline(Action callback)
        {
            //Show Loading form and hide NewForm

            BackgroundWorker bw = new BackgroundWorker();

            Loading loading = loading = new Loading("Downloading Modlist ...", true, new Size(900, 600));
            bool online = false;

            if (File.Exists(StaticVar.gameFolder + "ModManagerCache.json"))
            {
                DownloadPrompt prompt = new DownloadPrompt();
                prompt.Show();
                prompt.SetText("Do you want to update the mod list?");
                prompt.SetTitle("Update mod list");
                prompt.SetFinishedCallback(() =>
                {

                    loading.Show();
                    Hide();
                    online = true;
                    bw.RunWorkerAsync();
                });
                prompt.SetDeniedCallback(() =>
                {
                    online = false;
                    bw.RunWorkerAsync();
                });
            }
            else
            {
                loading.Show();
                Hide();
                online = true;
                bw.RunWorkerAsync();
            }

            bw.DoWork += (sender, args) =>
            {
                onlineModList = modReader.LoadOnlineMods(online);
            };
            bw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    MessageBox.Show(args.Error.ToString());

                //Show NewForm and hide Loading form
                loading.SetText("Finished Downloading Modlist!");
                loading.Finish();
                allowshowdisplay = true;
                SetVisibleCore(true);
                Show();
                callback();
            };

        }

        private void autoUpdate_Click(object sender, EventArgs e)
        {
            List<Mod> downloads = new List<Mod>();

            //I like System.Linq
            downloads = finalModList.Where(mod => mod.status == "Update Available").ToList();

            downloadManager.Initialize(downloads);

            downloadManager.downloadCallback = (() => LoadAllModsInstalled(() =>
            {
                downloadManager.downloading.SetText("Download Completed!");
                downloadManager.downloading.Finish();
                RefreshMods();
            }));

            downloadManager.Download();
        }

        //Download selected mods
        private void button1_Click(object sender, EventArgs e)
        {
            downloadManager.Initialize(modList.SelectedObjects.Cast<Mod>().ToList());

            downloadManager.downloadCallback = (() => LoadAllModsInstalled(() =>
            {
                downloadManager.downloading.SetText("Download Completed!");
                downloadManager.downloading.Finish();
                RefreshMods();
            }));

            downloadManager.Download();
        }

        //Enable selected mods
        private void button2_Click(object sender, EventArgs e)
        {
            finalModList.EnableMods(modList.GetSelection(), this);
        }

        //Disable selected mods
        private void button3_Click(object sender, EventArgs e)
        {
            finalModList.DisableMods(modList.GetSelection(), this);
        }

        //Delete selected mods
        private void button4_Click(object sender, EventArgs e)
        {
            DownloadPrompt prompt = new DownloadPrompt();
            prompt.Show();
            prompt.SetText("Are you sure you want to remove the selected mods?");
            prompt.SetTitle("Delete Mods");
            prompt.SetFinishedCallback(() =>
            {
                prompt.Close();
                List<Mod> selection = new List<Mod>();
                selection = modList.SelectedObjects.Cast<Mod>().ToList();

                foreach (Mod m in selection.ToArray())
                {
                    m.Uninstall();
                }

                finalModList.Clear();
                LoadAllModsInstalled(() => { modList.RefreshMods(this); });
            });
        }

        //Search mods
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            modList.ModelFilter = new ModelFilter(delegate (object x)
            {
                var myFile = x as Mod;

                if (myFile.name == null || myFile.title == null) return false;

                return x != null && (myFile.title.ToLower().Contains(textBox1.Text.ToLower()) || myFile.name.ToLower().Contains(textBox1.Text.ToLower()));
            });

            modList.RefreshMods(this);
        }

        /// /////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// ModPack Management
        /// </summary>
        /// ////////////////////////////////////////////////////////////////////////////

        public ModPack selected;
        public List<ModPack> modPacks = new List<ModPack>();

        public void LoadAllModPacks(Action callback)
        {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                modPacks = modPackLoader.LoadModPacks(finalModList);
                modPacksList.SetObjects(modPacks);
                modPackContent.ClearObjects();
            };
            bw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    MessageBox.Show(args.Error.ToString());

                callback();
            };

            bw.RunWorkerAsync();
        }

        //Open/Edit Modpack
        private void modPacksList_CellClick(object sender, BrightIdeasSoftware.CellClickEventArgs e)
        {
            selected = modPacksList.GetModPackSelection().FirstOrDefault();
            if (selected == null) return;
            modPackContent.SetObjects(selected.mods);
            selectedModpack.Text = "Selected Modpack: " + selected.name;
        }

        //Enable Modpack
        private void button1_Click_1(object sender, EventArgs e)
        {
            finalModList.EnableMods(selected.mods, this);
            RefreshMods();
        }

        //Disable Modpack
        private void button2_Click_1(object sender, EventArgs e)
        {
            finalModList.EnableMods(selected.mods, this);
            RefreshMods();
        }

        //Delete Modpack
        private void button3_Click_1(object sender, EventArgs e)
        {
            selected.Uninstall();
            RefreshMods();
        }

        //Add Modpack
        private void button7_Click(object sender, EventArgs e)
        {
            NewModPack nmp = new NewModPack();
            nmp.Show();
            nmp.SetCallBack((string name) =>
            {
                SimpleModPack mp = new SimpleModPack();
                mp.name = name;
                mp.mods = new List<SimpleMod>();
                string json = JsonConvert.SerializeObject(mp, Formatting.Indented);
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/" + name + ".json", json);
                RefreshMods();
                nmp.Close();
            });
        }

        //Add mods to modpack
        private void modPackContent_ModelCanDrop(object sender, BrightIdeasSoftware.ModelDropEventArgs e)
        {
            if (selected == null)
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                e.Effect = DragDropEffects.Copy;

                List<Mod> selection = new List<Mod>();
                selection = modList.SelectedObjects.Cast<Mod>().ToList();

                if (selection.Count == 0) return;

                Console.WriteLine("Add mods " + selection.Count);

                selected.AddMods(selection);
            }

            selected.Save();
            modPackContent.SetObjects(selected.mods);
            selectedModpack.Text = "Selected Modpack: " + name;

            RefreshMods();
        }

        private void modList_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            if (selected == null)
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                e.Effect = DragDropEffects.Copy;

                List<Mod> selection = new List<Mod>();
                selection = modPackContent.SelectedObjects.Cast<Mod>().ToList();

                if (selection.Count == 0) return;

                Console.WriteLine("Remove mods " + selection.Count);

                selected.RemoveMods(selection);
            }

            selected.Save();
            modPackContent.SetObjects(selected.mods);
            selectedModpack.Text = "Selected Modpack: " + name;

            RefreshMods();
        }

        private void modPackContent_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void modList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}