using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pure.Implementations.Wallets.EntityFramework;
using Pure.Shell;
namespace Pure
{
    class Program
    {
        internal static UserWallet Wallet;
        static void Main(string[] args)
        {
            new MainService().Run(args);
        }
    }
}
