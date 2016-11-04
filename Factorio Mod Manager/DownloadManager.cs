using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factorio_Mod_Manager
{
    public class DownloadManager
    {

        public int downloadIndex = 0;
        public List<Mod> downloadArray = new List<Mod>();
        public Loading downloading;

        public UserData userData;

        public void Initialize(List<Mod> downloads)
        {
            downloadArray = downloads;
            downloadIndex = 0;
            downloading = new Loading();
            downloading.Show();
            downloading.SetText("Downloading mods ... (1/" + downloadArray.Count + ")");
        }

        public DownloadManager(UserData userData)
        {
            this.userData = userData;
        }

        public void Download()
        {
            if (downloadArray.ElementAtOrDefault(downloadIndex) == null)
            {
                downloadCallback();

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
                Download();

                return;
            }

            foreach (string f in Directory.GetFiles(StaticVar.gameFolder + "mods/"))
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
            client.DownloadFileAsync(new Uri("https://mods.factorio.com" + url + "?username=" + userData.username + "&token=" + userData.token), StaticVar.gameFolder + "mods/" + url.Replace("%20", " ").Split('/')[url.Split('/').Length - 1]);
            client.DownloadFileCompleted += client_downloadCompleted;

        }

        public Action downloadCallback;

        private void client_downloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Download();
        }
    }
}