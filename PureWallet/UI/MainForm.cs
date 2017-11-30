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
using System.IO;
using System.IO.Compression;

using Pure.Core;
using Pure.Implementations.Blockchains.LevelDB;
using Pure.IO;
using Pure.VM;
using Pure.Implementations.Wallets.EntityFramework;
using Pure.Properties;

namespace Pure.UI
{
    public partial class MainForm : Form
    {
        private static readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();
        private bool balance_changed = false;
        private DateTime persistence_time = DateTime.MinValue;

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
            this.btn_menu.BackgroundImage = global::Pure.Properties.Resources.btn_menu_hover;
        }

        private void btn_menu_MouseLeave(object sender, EventArgs e)
        {
            this.btn_menu.BackgroundImage = global::Pure.Properties.Resources.btn_menu_normal;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tss_lbl_height_value.Text = $"{Blockchain.Default.Height}/{Blockchain.Default.HeaderHeight}";
            tss_lbl_connected_value.Text = Program.LocalNode.RemoteNodeCount.ToString();

            TimeSpan persistence_span = DateTime.Now - persistence_time;
            if (persistence_span < TimeSpan.Zero) persistence_span = TimeSpan.Zero;
            if (persistence_span > Blockchain.TimePerBlock)
            {
                this.tss_pgs_wait_block.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                this.tss_pgs_wait_block.Value = persistence_span.Seconds;
                this.tss_pgs_wait_block.Style = ProgressBarStyle.Blocks;
            }
        }

        private void ImportBlocks(Stream stream)
        {
            LevelDBBlockchain blockchain = (LevelDBBlockchain)Blockchain.Default;
            blockchain.VerifyBlocks = false;
            using (BinaryReader r = new BinaryReader(stream))
            {
                uint count = r.ReadUInt32();
                for (int height = 0; height < count; height++)
                {
                    byte[] array = r.ReadBytes(r.ReadInt32());
                    if (height > Blockchain.Default.Height)
                    {
                        Block block = array.AsSerializable<Block>();
                        Blockchain.Default.AddBlock(block);
                    }
                }
            }
            blockchain.VerifyBlocks = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                const string acc_path = "chain.acc";
                const string acc_zip_path = acc_path + ".zip";
                if (File.Exists(acc_path))
                {
                    using (FileStream fs = new FileStream(acc_path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        ImportBlocks(fs);
                    }
                    File.Delete(acc_path);
                }
                else if (File.Exists(acc_zip_path))
                {
                    using (FileStream fs = new FileStream(acc_zip_path, FileMode.Open, FileAccess.Read, FileShare.None))
                    using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                    using (Stream zs = zip.GetEntry(acc_path).Open())
                    {
                        ImportBlocks(zs);
                    }
                    File.Delete(acc_zip_path);
                }
                Blockchain.PersistCompleted += Blockchain_PersistCompleted;
                Program.LocalNode.Start(Settings.Default.NodePort, Settings.Default.WsPort);
            });
        }

        private void Blockchain_PersistCompleted(object sender, Block block)
        {
            persistence_time = DateTime.Now;
            if (Program.CurrentWallet?.GetCoins().Any(p => !p.State.HasFlag(CoinState.Spent) && p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)) == true)
                balance_changed = true;
            CurrentWallet_TransactionsChanged(null, Enumerable.Empty<TransactionInfo>());
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Blockchain.PersistCompleted -= Blockchain_PersistCompleted;
            ChangeWallet(null);
        }

        private void CurrentWallet_BalanceChanged(object sender, EventArgs e)
        {
            balance_changed = true;
        }

        private void ChangeWallet(UserWallet wallet)
        {
            if (Program.CurrentWallet != null)
            {
                Program.CurrentWallet.BalanceChanged -= CurrentWallet_BalanceChanged;
                Program.CurrentWallet.TransactionsChanged -= CurrentWallet_TransactionsChanged;
                Program.CurrentWallet.Dispose();
            }
            Program.CurrentWallet = wallet;
            dgv_transaction_history.DataSource = null;
            if (Program.CurrentWallet != null)
            {
                CurrentWallet_TransactionsChanged(null, Program.CurrentWallet.LoadTransactions());
                Program.CurrentWallet.BalanceChanged += CurrentWallet_BalanceChanged;
                Program.CurrentWallet.TransactionsChanged += CurrentWallet_TransactionsChanged;
            }

            changePasswordToolStripMenuItem.Enabled = Program.CurrentWallet != null;
            rebuildIndexToolStripMenuItem.Enabled = Program.CurrentWallet != null;
            restoreAccountToolStripMenuItem.Enabled = Program.CurrentWallet != null;
            transactionToolStripMenuItem.Enabled = Program.CurrentWallet != null;
            pureGasToolStripMenuItem.Enabled = Program.CurrentWallet != null;
            requestToolStripMenuItem.Enabled = Program.CurrentWallet != null;
            assetDistributionToolStripMenuItem.Enabled = Program.CurrentWallet != null;
            assetRegistrationToolStripMenuItem.Enabled = Program.CurrentWallet != null;
            deployContractToolStripMenuItem.Enabled = Program.CurrentWallet != null;
            invokeToolStripMenuItem.Enabled = Program.CurrentWallet != null;
            electionToolStripMenuItem.Enabled = Program.CurrentWallet != null;

            lst_addr.Items.Clear();
            if (Program.CurrentWallet != null)
            {
                foreach (UInt160 scriptHash in Program.CurrentWallet.GetAddresses().ToArray())
                {
                    Contract contract = Program.CurrentWallet.GetContract(scriptHash);
                    if (contract == null)
                        AddAddressToListView(scriptHash);
                    else
                        AddContractToListView(contract);
                }
            }
            balance_changed = true;
            
        }

        private void CurrentWallet_TransactionsChanged(object sender, IEnumerable<TransactionInfo> transactions)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, IEnumerable<TransactionInfo>>(CurrentWallet_TransactionsChanged), sender, transactions);
            }
            else
            {
                foreach (TransactionInfo info in transactions)
                {
                    string txid = info.Transaction.Hash.ToString();
                    /*
                    if (listView3.Items.ContainsKey(txid))
                    {
                        listView3.Items[txid].Tag = info;
                    }
                    else
                    {
                        listView3.Items.Insert(0, new ListViewItem(new[]
                        {
                            new ListViewItem.ListViewSubItem
                            {
                                Name = "time",
                                Text = info.Time.ToString()
                            },
                            new ListViewItem.ListViewSubItem
                            {
                                Name = "hash",
                                Text = txid
                            },
                            new ListViewItem.ListViewSubItem
                            {
                                Name = "confirmations",
                                Text = Strings.Unconfirmed
                            },
                            //add transaction type to list by phinx
                            new ListViewItem.ListViewSubItem
                            {
                                Name = "txtype",
                                Text = info.Transaction.Type.ToString()
                            }
                            //end

                        }, -1)
                        {
                            Name = txid,
                            Tag = info
                        });
                    }
                    */
                }
                /*
                foreach (ListViewItem item in listView3.Items)
                {
                    int? confirmations = (int)Blockchain.Default.Height - (int?)((TransactionInfo)item.Tag).Height + 1;
                    if (confirmations <= 0) confirmations = null;
                    item.SubItems["confirmations"].Text = confirmations?.ToString() ?? Strings.Unconfirmed;
                }
                */
            }
        }

        private void newWalletToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (CreateWalletDialog dialog = new CreateWalletDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                ChangeWallet(UserWallet.Create(dialog.WalletPath, dialog.Password));
                Settings.Default.LastWalletPath = dialog.WalletPath;
                Settings.Default.Save();
            }
        }

        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
