using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Pure.Properties;
using System.IO;
namespace Pure.UI
{
    public partial class OpenWalletDialog : Form
    {
        public OpenWalletDialog()
        {
            InitializeComponent();
            if (File.Exists(Settings.Default.LastWalletPath))
                textBox1.Text = Settings.Default.LastWalletPath;
        }

        public string Password
        {
            get
            {
                return textBox2.Text;
            }
            set
            {
                textBox2.Text = value;
            }
        }

        public string WalletPath
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "myWallet";
            // DefaultExt is only used when "All files" is selected from 
            // the filter box and no extension is specified by the user.
            openFileDialog1.DefaultExt = "db3";
            openFileDialog1.Filter = "db3 files (*.db3)|*.db3|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.TextLength == 0 || textBox2.TextLength == 0)
            {
                button2.Enabled = false;
                return;
            }
            button2.Enabled = true;
        }
    }
}
