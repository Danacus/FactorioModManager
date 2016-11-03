using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public partial class Form1 : Form
    {
        public dynamic data;
        public string search = "";

        public string currentdownload = "";
        public bool checkUpdates = false;

        public Main main;

        public Form1()
        {
            InitializeComponent();

            Deserialize(DownloadModsInfo());

        }

        public string DownloadModsInfo()
        {
            try
            {
                int size = 0;

                if (checkUpdates)
                    size = 5000;
                else
                    size = 50;

                search = searchBox.Text;
                string s = new WebClient().DownloadString("https://mods.factorio.com/api/mods?q=" + search + "&order=top&page_size=" + size);
                //Console.WriteLine(s);
                return s;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.GetType().ToString());
                return null;
            }
        }

        public void DownloadModPack(string json, Mod[] mods)
        {
            if (json == null)
            {
                MessageBox.Show("Error Deserializing Json File! https://mods.factorio.com/api/mods?q=" + searchBox.Text + " is null");
                return;
            }

            try
            {

                json = json.Replace("{}", "null");
                data = JsonConvert.DeserializeObject(json);
                //Console.WriteLine("Title: " + data.results[0].title);
                //Console.WriteLine("Url: " + data.results[0].latest_release.download_url);

                List<ModDownloadListItem> masterList = new List<ModDownloadListItem>();

                foreach (dynamic d in data.results)
                {
                    //ListViewItem i = new ListViewItem(new[] { (string)d.title, (string)d.latest_release.version, "Not Installed" });
                    //i.SubItems.Add("column3");
                    //listView1.Items.Add(i);

                    //masterList.Add(new ModObjectListItem("a", "b", true));
                    bool installed = false;

                    foreach (Mod m in mods)
                    {

                        if (m.title == (string)d.latest_release.info_json.name)
                        {
                            installed = true;
                            var version2 = new Version(m.version);
                            var version1 = new Version((string)d.latest_release.version);

                            var result = version1.CompareTo(version2);
                            if (result > 0)
                            {
                                currentdownload = (string)d.latest_release.download_url;
                                counter = 0;
                                downloadSize = 1;
                                Download(currentdownload);
                            }

                            //masterList.Add(new ModDownloadListItem((string)d.title, (string)d.latest_release.version, "Update Available", (string)d.downloads_count, (string)d.latest_release.download_url));

                        }
                        else
                        {

                        }

                    }

                    //Adding the object list to the Objectlistview

                    if (!installed && !checkUpdates)
                        masterList.Add(new ModDownloadListItem((string)d.title, (string)d.latest_release.version, "Not Installed", (string)d.downloads_count, (string)d.latest_release.download_url));
                }

                objectListView1.SetObjects(masterList);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Deserializing Json File! " + e.Message);
            }
        }

        public void Deserialize(string json)
        {
            if (json == null)
            {
                MessageBox.Show("Error Deserializing Json File! https://mods.factorio.com/api/mods?q=" + searchBox.Text + " is null");
                return;
            }

            try
            {

                json = json.Replace("{}", "null");
                data = JsonConvert.DeserializeObject(json);
                //Console.WriteLine("Title: " + data.results[0].title);
                //Console.WriteLine("Url: " + data.results[0].latest_release.download_url);

                List<ModDownloadListItem> masterList = new List<ModDownloadListItem>();

                foreach (dynamic d in data.results)
                {
                    //ListViewItem i = new ListViewItem(new[] { (string)d.title, (string)d.latest_release.version, "Not Installed" });
                    //i.SubItems.Add("column3");
                    //listView1.Items.Add(i);

                    //masterList.Add(new ModObjectListItem("a", "b", true));
                    bool installed = false;

                    foreach (Mod m in Main.userData.installedMods)
                    {

                        if (m.title == (string)d.latest_release.info_json.name)
                        {
                            installed = true;
                            var version2 = new Version(m.version);
                            var version1 = new Version((string)d.latest_release.version);

                            var result = version1.CompareTo(version2);
                            if (result > 0)
                                masterList.Add(new ModDownloadListItem((string)d.title, (string)d.latest_release.version, "Update Available", (string)d.downloads_count, (string)d.latest_release.download_url));
                            else if (!checkUpdates)
                                masterList.Add(new ModDownloadListItem((string)d.title, (string)d.latest_release.version, "Up-To-Date", (string)d.downloads_count, (string)d.latest_release.download_url));

                        }
                        else
                        {

                        }

                    }

                    //Adding the object list to the Objectlistview

                    if (!installed && !checkUpdates)
                        masterList.Add(new ModDownloadListItem((string)d.title, (string)d.latest_release.version, "Not Installed", (string)d.downloads_count, (string)d.latest_release.download_url));
                }

                objectListView1.SetObjects(masterList);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Deserializing Json File! " + e.Message);
            }

            //Download(data.results[0].latest_release.download_url);

        }

        public void Download(string dl)
        {
            foreach (string f in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/"))
            {
                if (f.Contains(".zip"))
                {
                    string name = f.Split('/')[f.Split('/').Length - 1];
                    name = name.Replace(".zip", "");
                    string version = name.Split('_')[name.Split('_').Length - 1];
                    string title = name.Replace("_" + version, "");

                    if (title == (string)data.results[objectListView1.SelectedItems[0].Index].title)
                    {
                        File.Delete(f);
                    }
                }
            }

            WebClient client = new WebClient();
            client.DownloadFileAsync(new Uri("https://mods.factorio.com" + dl + "?username=" + Main.userData.username + "&token=" + Main.userData.token), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/" + dl.Replace("%20", " ").Split('/')[dl.Split('/').Length - 1]);
            client.DownloadFileCompleted += client_downloadCompleted;
            client.DownloadProgressChanged += client_downloadProgress;

        }

        private void client_downloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            d.Progress((int)((float)(counter / downloadSize) * 100f + (counter / downloadSize) * e.ProgressPercentage));
        }

        private int counter = 0;
        private int downloadSize = 0;

        private void client_downloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //listView1.Items[currentdownload].SubItems[2].Text = "Up-To-Date";

            counter++;
            if (counter >= downloadSize)
            {
                d.Progress(100);
                main.LoadMods();
                main.CreateList();
                Deserialize(DownloadModsInfo());
                d.Close();
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            checkUpdates = false;
            Deserialize(DownloadModsInfo());
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        public class ModDownloadListItem
        {

            public string title { get; set; }
            public string version { get; set; }
            public string status { get; set; }
            public string downloads { get; set; }
            public string url { get; set; }

            public ModDownloadListItem(string title, string version, string status, string downloads, string url)
            {
                this.title = title;
                this.version = version;
                this.status = status;
                this.downloads = downloads;
                this.url = url;
            }
        }

        private Downloading d;

        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            currentdownload = ((ModDownloadListItem)objectListView1.SelectedObject).url;
            counter = 0;
            downloadSize = 1;
            d = new Downloading();
            d.Show();
            Download(currentdownload);

        }

        private void checkForUpdatesButton_Click(object sender, EventArgs e)
        {
            checkUpdates = true;
            Deserialize(DownloadModsInfo());
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            counter = 0;

            List<ModDownloadListItem> list = new List<ModDownloadListItem>();
            list = objectListView1.SelectedObjects.Cast<ModDownloadListItem>().ToList();

            downloadSize = list.Count;
            Console.WriteLine("Download Size: " + downloadSize);

            d = new Downloading();
            d.Show();

            foreach (ModDownloadListItem i in list)
            {
                currentdownload = i.url;

                Download(currentdownload);
            }
        }

        public ModPacks modPacksForm;

        public void DownloadModPack(List<ModPackItem> mods, ModPacks sender)
        {
            modPacksForm = sender;
            List<string> downloadUrls = new List<string>();

            foreach (ModPackItem m in mods)
            {
                downloadUrls.Add(m.name);
            }

            foreach (string s in downloadUrls)
            {
                if (GetUrl(s) == null)
                {
                    MessageBox.Show("Mod Not Found! " + s);
                    downloadUrls.Remove(s);
                }
            }

            downloadSize = downloadUrls.Count;

            d = new Downloading();
            d.Show();

            foreach (string s in downloadUrls)
            {

                WebClient client = new WebClient();
                client.DownloadFileAsync(new Uri("https://mods.factorio.com" + GetUrl(s) + "?username=" + Main.userData.username + "&token=" + Main.userData.token), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/mods/" + GetUrl(s).Replace("%20", " ").Split('/')[GetUrl(s).Split('/').Length - 1]);
                client.DownloadFileCompleted += client_downloadModPackCompleted;
            }

        }

        private void client_downloadModPackCompleted(object sender, AsyncCompletedEventArgs e)
        {
            counter++;
            if (counter >= downloadSize)
            {
                Console.WriteLine("Download Completed!");

                d.Progress(100);
                main.LoadMods();
                main.CreateList();
                modPacksForm.EnableMods();

                d.Close();
            }
        }

        public string GetUrl(string name)
        {

            try
            {
                int size = 50;

                search = name;
                string s = new WebClient().DownloadString("https://mods.factorio.com/api/mods?q=" + search + "&order=top&page_size=" + size);
                //Console.WriteLine(s);
                dynamic data = JsonConvert.DeserializeObject(s);

                string url = null;

                foreach (dynamic d in data.results)
                {
                    string u = (string)d.latest_release.download_url;

                    if (u.Contains(name))
                    {
                        url = u;
                    }
                }

                return url;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.GetType().ToString());
                return null;
            }

        }
    }
}