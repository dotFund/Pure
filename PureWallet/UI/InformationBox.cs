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
    public partial class InformationBox : Form
    {
        public InformationBox()
        {
            InitializeComponent();
        }

        public static DialogResult Show(string text, string message = null, string title = null)
        {
            using (InformationBox box = new InformationBox())
            {
                box.textBox1.Text = text;
                if (message != null)
                {
                    box.label1.Text = message;
                }
                if (title != null)
                {
                    box.Text = title;
                }
                return box.ShowDialog();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.SelectAll();
            textBox1.Copy();
        }
    }
}
