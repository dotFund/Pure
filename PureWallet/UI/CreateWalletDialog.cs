using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pure.UI
{
    public partial class CreateWalletDialog : Form
    {
        public CreateWalletDialog()
        {
            InitializeComponent();
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
                textBox3.Text = value;
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

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.TextLength == 0 || textBox2.TextLength == 0 || textBox3.TextLength == 0)
            {
                button2.Enabled = false;
                return;
            }
            if (textBox2.Text != textBox3.Text)
            {
                button2.Enabled = false;
                return;
            }
            button2.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "myWallet";
            // DefaultExt is only used when "All files" is selected from 
            // the filter box and no extension is specified by the user.
            saveFileDialog1.DefaultExt = "db3";
            saveFileDialog1.Filter = "db3 files (*.db3)|*.db3|All files (*.*)|*.*";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = saveFileDialog1.FileName;
            }
        }
    }
}
