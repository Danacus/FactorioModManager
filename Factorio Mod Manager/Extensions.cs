using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Mod_Manager
{
    public static class Extensions
    {
        public static void EnableMods(this List<Mod> finalModList, List<Mod> selection, NewForm nf)
        {

            List<Mod> downloads = new List<Mod>();

            finalModList.ForEach(mod =>
            {
                if (selection.Any(m => m.name == mod.name))
                {
                    if (mod.installed)
                        mod.Enable();
                    else
                        downloads.Add(mod);
                }
            });

            nf.installedMods.ForEach(mod =>
            {
                if (selection.Any(m => m.name == mod.name))
                {
                    if (mod.installed)
                        mod.Enable();
                }
            });

            nf.RefreshMods();

            if (downloads.Count == 0)
                return;

            DownloadPrompt prompt = new DownloadPrompt();
            prompt.Show();
            prompt.SetFinishedCallback(() =>
            {
                prompt.Close();
                nf.downloadManager.Initialize(downloads);

                nf.downloadManager.downloadCallback = () => nf.LoadAllModsInstalled(() =>
                {

                    finalModList.ForEach(mod =>
                    {
                        if (selection.Any(m => m.name == mod.name))
                        {
                            if (mod.installed)
                                mod.Enable();
                            else
                                downloads.Add(mod);
                        }
                    });

                    nf.installedMods.ForEach(mod =>
                    {
                        if (selection.Any(m => m.name == mod.name))
                        {
                            if (mod.installed)
                                mod.Enable();
                        }
                    });

                    nf.downloadManager.downloading.SetText("Download Completed!");
                    nf.downloadManager.downloading.Finish();
                    nf.RefreshMods();
                    Console.WriteLine("Finished!");
                });

                nf.downloadManager.Download();

            });
        }

        public static void DisableMods(this List<Mod> finalModList, List<Mod> selection, NewForm nf)
        {
            finalModList.ForEach(mod =>
            {
                if (selection.Any(m => m.name == mod.name))
                {
                    mod.Disable();
                }
            });

            nf.installedMods.ForEach(mod =>
            {
                if (selection.Any(m => m.name == mod.name))
                {
                    mod.Disable();
                }
            });

            nf.RefreshMods();
        }
    }
}