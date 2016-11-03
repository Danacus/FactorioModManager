using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factorio_Mod_Manager
{
    public partial class DownloadPrompt : Form
    {
        private Action cb;

        public DownloadPrompt()
        {
            InitializeComponent();
            AcceptButton = button2;

        }

        public void SetText(string s)
        {
            label1.Text = s;
        }

        public void SetTitle(string s)
        {
            Text = s;
        }

        public void SetFinishedCallback(Action callback)
        {
            cb = callback;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            cb();
        }
    }
}