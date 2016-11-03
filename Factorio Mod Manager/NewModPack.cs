using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factorio_Mod_Manager
{
    public partial class NewModPack : Form
    {
        private Action<string> callback;

        public NewModPack()
        {
            InitializeComponent();
        }

        public void SetCallBack(Action<string> a)
        {
            callback = a;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            callback(textBox1.Text);
        }
    }
}