using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Mod_Manager
{
    public class UserData
    {
        public string username;
        public string token;

        public void Load(string filePath, out string error)
        {
            error = null;

            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException();

                string s = File.ReadAllText(filePath);
                s = s.Replace("-", "_");
                dynamic data = JsonConvert.DeserializeObject(s);

                if ((string)data.service_username == null || (string)data.service_username == "" || (string)data.service_token == null || (string)data.service_token == "")
                    throw new Exception();

                username = (string)data.service_username;
                token = (string)data.service_token;

                Console.WriteLine(string.Format("username = {0}, token = {1}", username, token));
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(FileNotFoundException))
                    error = "Unable to find player-data.json file! Please make sure you have installed Factorio and log in";
                else
                {
                    error = "Unable to find username or token! Please make sure you are logged in to Factorio";
                }
            }
        }
    }
}