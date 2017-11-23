using Pure.Core.Scripts;
using Pure.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pure.Core
{
    public class AgencyTransaction : Transaction, ISignable
    {
        public Order[] Orders;

        public AgencyTransaction()
            : base(TransactionType.AgencyTransaction)
        {
        }

        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            this.Orders = reader.ReadSerializableArray<Order>();
        }

        public override IEnumerable<TransactionInput> GetAllInputs()
        {
            return Orders.SelectMany(p => p.Inputs).Concat(base.GetAllInputs());
        }

        public override UInt160[] GetScriptHashesForVerifying()
        {
            if (Orders.Select(p => p.Agent).Distinct().Count() != 1)
                throw new FormatException();
            return base.GetScriptHashesForVerifying().Union(new UInt160[] { Orders[0].Agent }).OrderBy(p => p).ToArray();
        }

        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.Write(Orders);
        }
    }
}
