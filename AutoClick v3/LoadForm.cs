using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoClick_v3
{
    public partial class LoadForm : Form
    {
        bool canClose = false;
        public LoadForm()
        {
            InitializeComponent();
        }

        private void LoadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (canClose) return;
            e.Cancel = true;
            
        }

        private async void LoadForm_Load(object sender, EventArgs e)
        {
            Show();

            MainForm MainForm = new MainForm(true,progressBar1,label1,this);
            
        }

        public void HideForm()
        {
            Hide();
        }

        public void CloseForm()
        {
            canClose = true;
            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
