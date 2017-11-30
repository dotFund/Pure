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
using Pure.Wallets;
using Pure.Cryptography;

namespace Pure.UI
{
    public partial class MainForm : Form
    {
        private static readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();
        private bool balance_changed = false;
        private bool check_nep5_balance = false;
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

            if (Program.CurrentWallet != null)
            {
                if (Program.CurrentWallet.WalletHeight <= Blockchain.Default.Height + 1)
                {
                    if (balance_changed)
                    {
                        IEnumerable<Coin> coins = Program.CurrentWallet?.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)) ?? Enumerable.Empty<Coin>();
                        Fixed8 bonus_available = Blockchain.CalculateBonus(Program.CurrentWallet.GetUnclaimedCoins().Select(p => p.Reference));
                        Fixed8 bonus_unavailable = Blockchain.CalculateBonus(coins.Where(p => p.State.HasFlag(CoinState.Confirmed) && p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)).Select(p => p.Reference), Blockchain.Default.Height + 1);
                        Fixed8 bonus = bonus_available + bonus_unavailable;
                        var assets = coins.GroupBy(p => p.Output.AssetId, (k, g) => new
                        {
                            Asset = Blockchain.Default.GetAssetState(k),
                            Value = g.Sum(p => p.Output.Value),
                            Claim = k.Equals(Blockchain.UtilityToken.Hash) ? bonus : Fixed8.Zero
                        }).ToDictionary(p => p.Asset.AssetId);
                        if (bonus != Fixed8.Zero && !assets.ContainsKey(Blockchain.UtilityToken.Hash))
                        {
                            assets[Blockchain.UtilityToken.Hash] = new
                            {
                                Asset = Blockchain.Default.GetAssetState(Blockchain.UtilityToken.Hash),
                                Value = Fixed8.Zero,
                                Claim = bonus
                            };
                        }
                        var balance_ans = coins.Where(p => p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
                        var balance_anc = coins.Where(p => p.Output.AssetId.Equals(Blockchain.UtilityToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
                        foreach (ListViewItem item in lst_addr.Items)
                        {
                            UInt160 script_hash = Wallet.ToScriptHash(item.Name);
                            Fixed8 ans = balance_ans.ContainsKey(script_hash) ? balance_ans[script_hash] : Fixed8.Zero;
                            Fixed8 anc = balance_anc.ContainsKey(script_hash) ? balance_anc[script_hash] : Fixed8.Zero;
                            item.SubItems["ans"].Text = ans.ToString();
                            item.SubItems["anc"].Text = anc.ToString();
                        }

                        foreach (AssetState asset in lst_asset.Items.OfType<ListViewItem>().Select(p => p.Tag as AssetState).Where(p => p != null).ToArray())
                        {
                            if (!assets.ContainsKey(asset.AssetId))
                            {
                                lst_asset.Items.RemoveByKey(asset.AssetId.ToString());
                            }
                        }
                        foreach (var asset in assets.Values)
                        {
                            string value_text = asset.Value.ToString() + (asset.Asset.AssetId.Equals(Blockchain.UtilityToken.Hash) ? $"+({asset.Claim})" : "");
                            if (lst_asset.Items.ContainsKey(asset.Asset.AssetId.ToString()))
                            {
                                lst_asset.Items[asset.Asset.AssetId.ToString()].SubItems["value"].Text = value_text;
                            }
                            else
                            {
                                string asset_name = asset.Asset.AssetType == AssetType.GoverningToken ? "NEO" :
                                                    asset.Asset.AssetType == AssetType.UtilityToken ? "NeoGas" :
                                                    asset.Asset.GetName();
                                lst_asset.Items.Add(new ListViewItem(new[]
                                {
                                    new ListViewItem.ListViewSubItem
                                    {
                                        Name = "name",
                                        Text = asset_name
                                    },
                                    new ListViewItem.ListViewSubItem
                                    {
                                        Name = "type",
                                        Text = asset.Asset.AssetType.ToString()
                                    },
                                    new ListViewItem.ListViewSubItem
                                    {
                                        Name = "value",
                                        Text = value_text
                                    },
                                    new ListViewItem.ListViewSubItem
                                    {
                                        ForeColor = Color.Gray,
                                        Name = "issuer",
                                        Text = $"{Strings.UnknownIssuer}[{asset.Asset.Owner}]"
                                    }
                                }, -1, lst_asset.Groups["unchecked"])
                                {
                                    Name = asset.Asset.AssetId.ToString(),
                                    Tag = asset.Asset,
                                    UseItemStyleForSubItems = false
                                });
                            }
                        }
                        balance_changed = false;
                    }

                    foreach (ListViewItem item in lst_asset.Groups["unchecked"].Items.OfType<ListViewItem>().ToArray())
                    {
                        ListViewItem.ListViewSubItem subitem = item.SubItems["issuer"];
                        AssetState asset = (AssetState)item.Tag;
                        CertificateQueryResult result;
                        if (asset.AssetType == AssetType.GoverningToken || asset.AssetType == AssetType.UtilityToken)
                        {
                            result = new CertificateQueryResult { Type = CertificateQueryResultType.System };
                        }
                        else
                        {
                            result = CertificateQueryService.Query(asset.Owner);
                        }
                        using (result)
                        {
                            subitem.Tag = result.Type;
                            switch (result.Type)
                            {
                                case CertificateQueryResultType.Querying:
                                case CertificateQueryResultType.QueryFailed:
                                    break;
                                case CertificateQueryResultType.System:
                                    subitem.ForeColor = Color.Green;
                                    subitem.Text = Strings.SystemIssuer;
                                    break;
                                case CertificateQueryResultType.Invalid:
                                    subitem.ForeColor = Color.Red;
                                    subitem.Text = $"[{Strings.InvalidCertificate}][{asset.Owner}]";
                                    break;
                                case CertificateQueryResultType.Expired:
                                    subitem.ForeColor = Color.Yellow;
                                    subitem.Text = $"[{Strings.ExpiredCertificate}]{result.Certificate.Subject}[{asset.Owner}]";
                                    break;
                                case CertificateQueryResultType.Good:
                                    subitem.ForeColor = Color.Black;
                                    subitem.Text = $"{result.Certificate.Subject}[{asset.Owner}]";
                                    break;
                            }
                            switch (result.Type)
                            {
                                case CertificateQueryResultType.System:
                                case CertificateQueryResultType.Missing:
                                case CertificateQueryResultType.Invalid:
                                case CertificateQueryResultType.Expired:
                                case CertificateQueryResultType.Good:
                                    item.Group = lst_asset.Groups["checked"];
                                    break;
                            }
                        }
                    }
                }

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
            lst_transaction.Items.Clear();
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
                    VerificationContract contract = Program.CurrentWallet.GetContract(scriptHash);
                    if (contract == null)
                        AddAddressToListView(scriptHash);
                    else
                        AddContractToListView(contract);
                }
            }
            balance_changed = true;
            
        }

        private void AddAddressToListView(UInt160 scriptHash, bool selected = false)
        {
            string address = Wallet.ToAddress(scriptHash);
            ListViewItem item = lst_addr.Items[address];
            if (item == null)
            {
                ListViewGroup group = lst_addr.Groups["watchOnlyGroup"];
                item = lst_addr.Items.Add(new ListViewItem(new[]
                {
                    new ListViewItem.ListViewSubItem
                    {
                        Name = "address",
                        Text = address
                    },
                    new ListViewItem.ListViewSubItem
                    {
                        Name = "ans"
                    },
                    new ListViewItem.ListViewSubItem
                    {
                        Name = "anc"
                    }
                }, -1, group)
                {
                    Name = address,
                    Tag = scriptHash
                });
            }
            item.Selected = selected;
        }

        private void AddContractToListView(VerificationContract contract, bool selected = false)
        {
            ListViewItem item = lst_addr.Items[contract.Address];
            if (item?.Tag is UInt160)
            {
                lst_addr.Items.Remove(item);
                item = null;
            }
            if (item == null)
            {
                ListViewGroup group = contract.IsStandard ? lst_addr.Groups["standardContractGroup"] : lst_addr.Groups["nonstandardContractGroup"];
                item = lst_addr.Items.Add(new ListViewItem(new[]
                {
                    new ListViewItem.ListViewSubItem
                    {
                        Name = "address",
                        Text = contract.Address
                    },
                    new ListViewItem.ListViewSubItem
                    {
                        Name = "ans"
                    },
                    new ListViewItem.ListViewSubItem
                    {
                        Name = "anc"
                    }
                }, -1, group)
                {
                    Name = contract.Address,
                    Tag = contract
                });
            }
            item.Selected = selected;
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
                    
                    if (lst_transaction.Items.ContainsKey(txid))
                    {
                        lst_transaction.Items[txid].Tag = info;
                    }
                    else
                    {
                        lst_transaction.Items.Insert(0, new ListViewItem(new[]
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
                    
                }
                
                foreach (ListViewItem item in lst_transaction.Items)
                {
                    int? confirmations = (int)Blockchain.Default.Height - (int?)((TransactionInfo)item.Tag).Height + 1;
                    if (confirmations <= 0) confirmations = null;
                    item.SubItems["confirmations"].Text = confirmations?.ToString() ?? Strings.Unconfirmed;
                }
                
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

        private void lst_addr_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Near;

                using (var headerFont = new Font("Microsoft Sans Serif", 9, FontStyle.Regular))
                {
                    Rectangle rt = e.Bounds;
                    rt.Width += 1000;
                    rt.X += 1;
                    e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(13)))))), rt);

                    Rectangle rtText = e.Bounds;
                    rtText.Y = (rtText.Height - headerFont.Height) / 3;
                    rtText.X += 10;
                    rtText.Width -= 10;
                    e.Graphics.DrawString(e.Header.Text, headerFont,
                        Brushes.White, rtText, sf);
                    //e.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));

                    e.Graphics.DrawLine(new Pen(System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(63)))))), new Point(e.Bounds.X, e.Bounds.Height - 2), new Point(e.Bounds.Width + e.Bounds.X, e.Bounds.Height - 2));
                    e.Graphics.DrawLine(new Pen(System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(63)))))), new Point(e.Bounds.X + e.Bounds.Width, 2), new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Height - 6));
                }
            }
        }

        private void lst_addr_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void lst_addr_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {

        }
    }
}
