using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Mod_Manager
{
    public static class ListViewExtensions
    {
        public static void RefreshMods(this ObjectListView olv, NewForm nf)
        {
            List<Mod> list = nf.modReader.CreateFinalModList(nf.installedMods, nf.onlineModList);
            olv.SetObjects(list);
            nf.finalModList = list;
        }

        public static List<Mod> GetSelection(this ObjectListView olv)
        {
            List<Mod> selection = new List<Mod>();
            selection = olv.SelectedObjects.Cast<Mod>().ToList();
            return selection;
        }

        public static List<ModPack> GetModPackSelection(this ObjectListView olv)
        {
            List<ModPack> selection = new List<ModPack>();
            selection = olv.SelectedObjects.Cast<ModPack>().ToList();
            return selection;
        }

    }
}