using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Mod_Manager
{
    public static class StaticVar
    {
        public static string gameFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Factorio/";
    }
}