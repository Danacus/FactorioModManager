using BrightIdeasSoftware;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factorio_Mod_Manager
{
    /// <summary>
    /// Mod Management
    /// </summary>

    public partial class NewForm : Form
    {
        public static UserData userData = new UserData();
        public List<Mod> onlineModList = new List<Mod>();
        private List<Mod> finalModList = new List<Mod>();

        public NewForm()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;
            LoadUserData();
            LoadAllModsOnline(() => { LoadModList(); LoadModPacks(); });
            LoadAllModsInstalled(() => { });

        }

        private bool allowshowdisplay = false;

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        }

        public void LoadUserData()
        {
            try
            {
                if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/player-data.json"))
                    throw new FileNotFoundException();

                string s = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/player-data.json");
                s = s.Replace("-", "_");
                dynamic data = JsonConvert.DeserializeObject(s);

                if ((string)data.service_username == null || (string)data.service_username == "" || (string)data.service_token == null || (string)data.service_token == "")
                    throw new Exception();

                userData.username = (string)data.service_username;
                userData.token = (string)data.service_token;

                Console.WriteLine(string.Format("username = {0}, token = {1}", userData.username, userData.token));
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(FileNotFoundException))
                    MessageBox.Show("Unable to find player-data.json file! Please make sure you have installed Factorio and log in");
                else
                {
                    MessageBox.Show("Unable to find username or token! Please make sure you are logged in to Factorio");
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void LoadAllModsInstalled(Action callback)
        {

            var bw = new BackgroundWorker();

            // define the event handlers
            bw.DoWork += (sender, args) =>
            {
                // do your lengthy stuff here -- this will happen in a separate thread
                LoadMods();
            };
            bw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)  // if an exception occurred during DoWork,
                    MessageBox.Show(args.Error.ToString());  // do your error handling here

                callback();
            };

            bw.RunWorkerAsync();
        }

        public void LoadAllModsOnline(Action callback)
        {
            Loading loading = new Loading();
            loading.Show();
            Hide();
            loading.SetText("Downloading Modlist ...");
            //loading.WindowState = FormWindowState.Maximized;
            loading.Size = new Size(900, 600);
            loading.ShowPicture();

            var bw = new BackgroundWorker();

            // define the event handlers
            bw.DoWork += (sender, args) =>
            {
                // do your lengthy stuff here -- this will happen in a separate thread
                string info = DownloadModsInfo();
                dynamic data = JsonConvert.DeserializeObject(info);

                foreach (dynamic d in data.results)
                {
                    Mod m = new Mod((string)d.latest_release.info_json.title, (string)d.latest_release.info_json.name, null, new Version((string)d.latest_release.info_json.version), new Version((string)d.latest_release.info_json.factorio_version), (int)d.downloads_count, (string)d.latest_release.download_url, false, false, null);
                    onlineModList.Add(m);
                }
            };
            bw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)  // if an exception occurred during DoWork,
                    MessageBox.Show(args.Error.ToString());  // do your error handling here

                // Do whatever else you want to do after the work completed.
                // This happens in the main UI thread.

                loading.SetText("Finished Downloading Modlist!");
                loading.Finish();
                allowshowdisplay = true;
                SetVisibleCore(true);
                Show();
                callback();

            };

            bw.RunWorkerAsync();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private int downloadIndex = 0;
        public List<Mod> downloadArray = new List<Mod>();
        public Loading downloading;

        public void Download(Action callback)
        {
            if (downloadArray.ElementAtOrDefault(downloadIndex) == null)
            {
                callback();

                return;
            }

            downloading.progressBar.Style = ProgressBarStyle.Continuous;
            downloading.progressBar.Value = (int)((float)(((downloadIndex + 1) * 1.0f / downloadArray.Count * 1.0f) * 100.0f));
            Console.WriteLine(((float)(((downloadIndex + 1) * 1.0f / downloadArray.Count * 1.0f) * 100.0f)));

            downloading.SetText("Downloading mods ... (" + (downloadIndex + 1) + "/" + downloadArray.Count + ")");
            Download(downloadArray[downloadIndex].url, downloadArray[downloadIndex].name);
            downloadIndex++;
        }

        public void Download(string url, string modTitle)
        {
            if (url == null)
            {
                MessageBox.Show("Failed to download mod!");
                downloadIndex++;
                Download(downloadCallback);

                return;
            }

            foreach (string f in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/"))
            {
                if (f.Contains(".zip"))
                {
                    string name = f.Split('/')[f.Split('/').Length - 1];
                    name = name.Replace(".zip", "");
                    string version = name.Split('_')[name.Split('_').Length - 1];
                    string title = name.Replace("_" + version, "");

                    if (title == modTitle)
                    {
                        File.Delete(f);
                    }
                }
            }

            WebClient client = new WebClient();
            client.DownloadFileAsync(new Uri("https://mods.factorio.com" + url + "?username=" + userData.username + "&token=" + userData.token), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/" + url.Replace("%20", " ").Split('/')[url.Split('/').Length - 1]);
            client.DownloadFileCompleted += client_downloadCompleted;

        }

        private Action downloadCallback;

        private void client_downloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Download(downloadCallback);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void LoadModList()
        {
            finalModList.Clear();

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

            modList.SetObjects(finalModList);
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

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public List<Mod> installedMods = new List<Mod>();

        public void LoadMods()
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/mod-list.json"))
            {
                string s = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/mod-list.json");
                dynamic data = JsonConvert.DeserializeObject(s);

                installedMods.Clear();

                foreach (dynamic d in data.mods)
                {
                    Mod m = new Mod(null, (string)d.name, null, null, null, 0, null, true, (bool)d.enabled, null);
                    installedMods.Add(m);
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

            SaveModList();
        }

        public void SaveModList()
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

            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/mod-list.json", file);

        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void autoUpdate_Click(object sender, EventArgs e)
        {
            downloadArray.Clear();

            foreach (Mod m in finalModList)
            {
                if (m.status == "Update Available")
                {
                    downloadArray.Add(m);
                }
            }

            downloadIndex = 0;
            downloading = new Loading();
            downloading.Show();
            downloading.SetText("Downloading mods ... (1/" + downloadArray.Count + ")");

            downloadCallback = (() => LoadAllModsInstalled(() =>
            {

                downloading.SetText("Download Completed!");
                downloading.Finish();
                LoadModList();
            }));

            Download(() =>
            {
                LoadAllModsInstalled(() =>
                {
                    downloading.SetText("Download Completed!");
                    downloading.Finish();
                    LoadModList();
                });
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            downloadIndex = 0;
            downloadArray = modList.SelectedObjects.Cast<Mod>().ToList();
            downloading = new Loading();
            downloading.Show();
            downloading.SetText("Downloading mods ... (1/" + downloadArray.Count + ")");

            downloadCallback = (() => LoadAllModsInstalled(() =>
            {

                downloading.SetText("Download Completed!");
                downloading.Finish();
                LoadModList();
            }));

            Download(() =>
            {
                LoadAllModsInstalled(() =>
                {
                    downloading.SetText("Download Completed!");
                    downloading.Finish();
                    LoadModList();
                });
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            EnableMods();
        }

        public void EnableMods()
        {
            List<Mod> selection = new List<Mod>();
            selection = modList.SelectedObjects.Cast<Mod>().ToList();

            List<Mod> downloads = selection;

            foreach (Mod fm in installedMods)
            {

                foreach (Mod m in selection.ToArray())
                {

                    if (fm.name == m.name)
                    {
                        if (fm.installed)
                        {
                            fm.enabled = true;
                        }
                    }
                }
            }

            foreach (Mod fm in finalModList)
            {
                foreach (Mod m in selection.ToArray())
                {
                    if (fm.name == m.name)
                    {
                        if (m.installed)
                        {
                            downloads.Remove(m);
                        }
                    }
                }
            }

            SaveModList();
            LoadModList();

            if (downloads.Count == 0)
                return;

            DownloadPrompt prompt = new DownloadPrompt();
            prompt.Show();
            prompt.SetFinishedCallback(() =>
            {
                prompt.Close();
                downloadArray = downloads;
                downloading = new Loading();
                downloading.Show();
                downloading.SetText("Downloading mods ... (1/" + downloadArray.Count + ")");
                downloadIndex = 0;

                downloadCallback = () => LoadAllModsInstalled(() =>
                {

                    foreach (Mod fm in installedMods)
                    {

                        foreach (Mod m in selection.ToArray())
                        {

                            if (fm.name == m.name)
                            {
                                if (fm.installed)
                                {
                                    fm.enabled = true;
                                }
                            }
                        }
                    }

                    SaveModList();
                    LoadModList();
                    downloading.SetText("Download Completed!");
                    downloading.Finish();
                    Console.WriteLine("Finished!");
                });

                Download(downloadCallback);

            });

        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<Mod> selection = new List<Mod>();
            selection = modList.SelectedObjects.Cast<Mod>().ToList();

            List<Mod> downloads = selection;

            foreach (Mod fm in installedMods)
            {

                foreach (Mod m in selection.ToArray())
                {

                    if (fm.name == m.name)
                    {
                        if (fm.installed)
                        {
                            fm.enabled = false;
                        }
                    }
                }
            }

            SaveModList();
            LoadModList();

        }

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

                foreach (Mod fm in installedMods)
                {

                    foreach (Mod m in selection.ToArray())
                    {

                        if (fm.name == m.name)
                        {
                            if (fm.installed)
                            {
                                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/" + fm.name + "_" + fm.localVersion + ".zip"))
                                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/" + fm.name + "_" + fm.localVersion + ".zip");
                            }
                        }
                    }
                }

                finalModList.Clear();
                LoadAllModsInstalled(() => { LoadModList(); });
            });
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

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
        }

        public class UserData
        {
            public string username;
            public string token;
        }

        /// /////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// ModPack Management
        /// </summary>
        /// ////////////////////////////////////////////////////////////////////////////

        public ModPack selected;
        public List<ModPack> modPacks = new List<ModPack>();

        public void LoadModPacks()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/");

            modPacks.Clear();

            foreach (string file in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/"))
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

                modPacksList.SetObjects(modPacks);

                modPackContent.ClearObjects();

            }
        }

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
        }

        private void modPacksList_SelectionChanged(object sender, EventArgs e)
        {
            //List<ModPack> selection = new List<ModPack>();
            //selection = modList.SelectedObjects.Cast<ModPack>().ToList();

        }

        private void modPacksList_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void modPacksList_CellClick(object sender, BrightIdeasSoftware.CellClickEventArgs e)
        {

            List<ModPack> selection = new List<ModPack>();
            selection = modPacksList.SelectedObjects.Cast<ModPack>().ToList();
            selected = selection.FirstOrDefault();

            if (selected == null) return;

            List<Mod> mods = new List<Mod>();

            foreach (Mod m in selected.mods)
            {
                mods.Add(m);
            }

            modPackContent.SetObjects(mods);

            selectedModpack.Text = "Selected Modpack: " + selected.name;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            EnableMods(selected.mods);
        }

        public void EnableMods(List<Mod> mods)
        {
            List<Mod> selection = mods;

            List<Mod> downloads = selection;

            foreach (Mod fm in installedMods)
            {

                foreach (Mod m in selection.ToArray())
                {

                    if (fm.name == m.name)
                    {
                        if (fm.installed)
                        {
                            fm.enabled = true;
                        }
                    }
                }
            }

            foreach (Mod fm in finalModList)
            {
                foreach (Mod m in selection.ToArray())
                {
                    if (fm.name == m.name)
                    {
                        if (m.installed)
                        {
                            downloads.Remove(m);
                        }
                    }
                }
            }

            SaveModList();
            LoadModList();
            LoadModPacks();

            if (downloads.Count == 0)
                return;

            DownloadPrompt prompt = new DownloadPrompt();
            prompt.Show();
            prompt.SetFinishedCallback(() =>
            {
                prompt.Close();
                downloadArray = downloads;
                downloading = new Loading();
                downloading.Show();
                downloading.SetText("Downloading mods ... (1/" + downloadArray.Count + ")");
                downloadIndex = 0;

                downloadCallback = () => LoadAllModsInstalled(() =>
                {

                    foreach (Mod fm in installedMods)
                    {

                        foreach (Mod m in selection.ToArray())
                        {

                            if (fm.name == m.name)
                            {
                                if (fm.installed)
                                {
                                    fm.enabled = true;
                                }
                            }
                        }
                    }

                    SaveModList();
                    LoadModList();
                    LoadModPacks();

                    downloading.SetText("Download Completed!");
                    downloading.Finish();
                    Console.WriteLine("Finished!");
                });

                Download(downloadCallback);

            });
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            List<Mod> selection = selected.mods;

            List<Mod> downloads = selection;

            foreach (Mod fm in installedMods)
            {

                foreach (Mod m in selection.ToArray())
                {

                    if (fm.name == m.name)
                    {

                        Console.WriteLine("Disable Mod");
                        fm.enabled = false;

                    }
                }
            }

            SaveModList();
            LoadModList();
            LoadModPacks();

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/" + selected.name + ".json"))
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/" + selected.name + ".json");

            SaveModList();
            LoadModList();
            LoadModPacks();
        }

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
                SaveModList();
                LoadModList();
                LoadModPacks();
                nmp.Close();
            });
        }

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

                AddMods(selection, selected);
            }

            SaveModPack();
        }

        public class SimpleModPack
        {
            public string name;
            public List<SimpleMod> mods;
        }

        public class SimpleMod
        {
            public string name;

            public SimpleMod(string n)
            {
                this.name = n;
            }
        }

        public void SaveModPack()
        {
            List<SimpleMod> mods = new List<SimpleMod>();

            foreach (Mod m in selected.mods)
            {
                mods.Add(new SimpleMod(m.name));
            }

            SimpleModPack mp = new SimpleModPack();
            mp.name = selected.name;
            mp.mods = mods;

            string json = JsonConvert.SerializeObject(mp, Formatting.Indented);
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/modpacks/" + selected.name + ".json", json);
            SaveModList();
            LoadModList();
            LoadModPacks();

            if (selected == null) return;

            List<Mod> mods2 = new List<Mod>();

            foreach (Mod m in selected.mods)
            {
                mods2.Add(m);
            }

            modPackContent.SetObjects(mods2);

            selectedModpack.Text = "Selected Modpack: " + selected.name;
        }

        public void AddMods(List<Mod> mods, ModPack target)
        {
            foreach (Mod m in mods)
            {
                if (!target.mods.Contains(m))
                    target.mods.Add(m);
            }
        }

        private void modPackContent_CanDrop(object sender, BrightIdeasSoftware.OlvDropEventArgs e)
        {
            if (selected == null)
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void modList_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            modList.ModelFilter = new ModelFilter(delegate (object x)
            {
                var myFile = x as Mod;

                if (myFile.name == null || myFile.title == null) return false;

                return x != null && (myFile.title.Contains(textBox1.Text) || myFile.name.Contains(textBox1.Text));
            });

            LoadModList();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            modList.ModelFilter = new ModelFilter(delegate (object x)
            {
                var myFile = x as Mod;

                if (myFile.name == null || myFile.title == null) return false;

                return x != null && (myFile.title.ToLower().Contains(textBox1.Text.ToLower()) || myFile.name.ToLower().Contains(textBox1.Text.ToLower()));
            });

            LoadModList();
        }

    }

}