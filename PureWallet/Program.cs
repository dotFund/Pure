using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pure.Network;
namespace PureWallet
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static LocalNode LocalNode;

        [STAThread]
        static void Main()
        {
            LocalNode = new LocalNode();
            LocalNode.Start();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
