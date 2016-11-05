using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factorio_Mod_Manager
{
    public class ExecutableManager
    {
        public List<Executable> executables = new List<Executable>();

        public List<Executable> LoadExecutables()
        {
            if (!File.Exists(StaticVar.gameFolder + "ModManagerData.json"))
                File.WriteAllText(StaticVar.gameFolder + "ModManagerData.json", JsonConvert.SerializeObject(executables));

            executables.Clear();

            dynamic d = JsonConvert.DeserializeObject(File.ReadAllText(StaticVar.gameFolder + "ModManagerData.json"));

            foreach (dynamic e in d)
            {
                executables.Add(new Executable((string)e.version, (string)e.path));
            }

            return executables;
        }

        public void SaveExecutables()
        {
            File.WriteAllText(StaticVar.gameFolder + "ModManagerData.json", JsonConvert.SerializeObject(executables));
        }

        public void AddExecutable()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = Environment.SpecialFolder.ProgramFiles.ToString();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists(fbd.SelectedPath + "/data/base/info.json"))
                    MessageBox.Show("Please select a valid Factorio installation folder!");

                string path = "";

                if (!File.Exists(fbd.SelectedPath + "/bin/x64/factorio.exe"))
                {
                    if (File.Exists(fbd.SelectedPath + "/bin/x86/factorio.exe"))
                    {
                        path = fbd.SelectedPath + "/bin/x86/factorio.exe";
                    }
                }
                else
                {
                    path = fbd.SelectedPath + "/bin/x64/factorio.exe";
                }

                Executable e = new Executable(null, null);

                if (path.Contains("Steam\\steamapps\\common"))
                {
                    e = new Executable("(Steam)", path);
                }
                else
                {
                    string json = File.ReadAllText(fbd.SelectedPath + "/data/base/info.json");
                    dynamic d = JsonConvert.DeserializeObject(json);
                    e = new Executable((string)d.version, path);
                }
                AddExecutable(e);

                SaveExecutables();
                LoadExecutables();
            }
        }

        public void AddExecutable(Executable exe)
        {
            executables.Add(exe);
            SaveExecutables();
        }

        public void RunExecutable(Executable exe)
        {
            Process.Start(exe.path);
        }
    }
}