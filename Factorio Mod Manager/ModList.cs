using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Mod_Manager
{
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