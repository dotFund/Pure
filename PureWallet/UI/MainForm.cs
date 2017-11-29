using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Reflection;

using Pure.Core;
namespace PureWallet.UI
{
    public partial class MainForm : Form
    {
        public MainForm(XDocument xdoc = null)
        {
            InitializeComponent();

            if (xdoc != null)
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                Version latest = Version.Parse(xdoc.Element("update").Attribute("latest").Value);
                if (version < latest)
                {
                    //Require Downloads
                }
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void transactionToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tss_lbl_wait_block_Click(object sender, EventArgs e)
        {

        }

        private void tss_pgs_wait_block_Click(object sender, EventArgs e)
        {

        }

        private void btn_menu_MouseHover(object sender, EventArgs e)
        {
            this.btn_menu.BackgroundImage = global::PureWallet.Properties.Resources.btn_menu_hover;
        }

        private void btn_menu_MouseLeave(object sender, EventArgs e)
        {
            this.btn_menu.BackgroundImage = global::PureWallet.Properties.Resources.btn_menu_normal;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tss_lbl_height_value.Text = $"{Blockchain.Default.Height}/{Blockchain.Default.HeaderHeight}";
            tss_lbl_connected_value.Text = Program.LocalNode.RemoteNodeCount.ToString();
        }
    }
}
