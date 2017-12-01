using Pure.Core;
using Pure.Wallets;
using System;

namespace Pure.UI
{
    internal class TxOutListBoxItem
    {
        public TransactionOutput Output;
        public string AssetName;
        public UIntBase AssetId;
        public BigDecimal Value;
        public UInt160 ScriptHash;

        public override string ToString()
        {
            return $"{Wallet.ToAddress(Output.ScriptHash)}\t{Output.Value}\t{AssetName}";
        }

        public TransactionOutput ToTxOutput()
        {
            if (AssetId is UInt256 asset_id && Value.Decimals == 8)
                return new TransactionOutput
                {
                    AssetId = asset_id,
                    Value = new Fixed8((long)Value.Value),
                    ScriptHash = ScriptHash
                };
            throw new NotSupportedException();
        }
    }
}
