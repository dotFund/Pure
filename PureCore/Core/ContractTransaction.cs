using Pure.IO;
using System;
using System.IO;

namespace Pure.Core
{
    public class ContractTransaction : Transaction
    {
        public TransactionAttribute[] Attributes;

        public ContractTransaction()
            : base(TransactionType.ContractTransaction)
        {
        }

        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            this.Attributes = reader.ReadSerializableArray<TransactionAttribute>();
        }

        public override UInt160[] GetScriptHashesForVerifying()
        {
            throw new NotImplementedException();
        }

        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.Write(Attributes);
        }
    }
}
