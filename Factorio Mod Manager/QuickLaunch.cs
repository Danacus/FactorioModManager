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
    public partial class QuickLaunch : Form
    {
        private ExecutableManager executableManager = new ExecutableManager();

        public QuickLaunch()
        {
            InitializeComponent();
            executableManager.LoadExecutables();
            SetExecutables();
        }

        public void SetExecutables()
        {
            comboBox1.Items.Clear();

            foreach (Executable e in executableManager.executables)
            {
                comboBox1.Items.Add("Factorio " + e.version);
            }

            comboBox1.Items.Add("Add Executable ...");
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Add Executable ...")
            {
                executableManager.AddExecutable();
                SetExecutables();
            }
        }

        private void launchButton_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null) return;

            if (comboBox1.SelectedItem.ToString() != "Add Executable ...")
            {
                executableManager.RunExecutable(executableManager.executables[comboBox1.Items.IndexOf(comboBox1.SelectedItem)]);
            }
        }

        private void selectModsButton_Click(object sender, EventArgs e)
        {
            NewForm n = new NewForm();
            n.Show();
        }
    }
}