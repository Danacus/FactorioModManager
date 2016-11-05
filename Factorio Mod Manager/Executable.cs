using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Mod_Manager
{
    public class Executable
    {
        public string version;
        public string path;

        public Executable(string version, string path)
        {
            this.version = version;
            this.path = path;
        }
    }
}