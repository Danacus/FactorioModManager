﻿using System;
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
    public partial class Downloading : Form
    {
        public Downloading()
        {
            InitializeComponent();
        }

        public void Progress(int progress)
        {
            progressBar1.Value = progress;
        }
    }
}