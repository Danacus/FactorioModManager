using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factorio_Mod_Manager
{
    public partial class Loading : Form
    {
        public Loading()
        {
            InitializeComponent();
            pictureBox1.Hide();
        }

        public ProgressBar progressBar
        {
            get
            {
                return progressBar1;
            }

            set
            {
                progressBar1 = value;
            }
        }

        public void ShowPicture()
        {
            pictureBox1.Show();
        }

        public void SetText(string s)
        {
            Text = s;

        }

        public void Finish()
        {
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Value = 100;
            Stopwatch sw = new Stopwatch(); // sw cotructor
            sw.Start(); // starts the stopwatch
            for (int i = 0; ; i++)
            {
                if (i % 100000 == 0) // if in 100000th iteration (could be any other large number
                                     // depending on how often you want the time to be checked)
                {
                    sw.Stop(); // stop the time measurement
                    if (sw.ElapsedMilliseconds > 1000) // check if desired period of time has elapsed
                    {
                        break; // if more than 5000 milliseconds have passed, stop looping and return
                               // to the existing code
                    }
                    else
                    {
                        sw.Start(); // if less than 5000 milliseconds have elapsed, continue looping
                                    // and resume time measurement
                    }
                }
            }
            this.Close();
        }
    }
}