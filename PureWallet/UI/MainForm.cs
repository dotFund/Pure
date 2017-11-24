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

namespace PureWallet.UI
{
    public partial class MainForm : Form
    {
        public MainForm(XDocument xdoc = null)
        {
            InitializeComponent();
        }
    }
}
