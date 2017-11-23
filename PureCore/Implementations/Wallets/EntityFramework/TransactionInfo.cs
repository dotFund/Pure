using System;
using CoreTransaction = Pure.Core.Transaction;

namespace Pure.Implementations.Wallets.EntityFramework
{
    public class TransactionInfo
    {
        public CoreTransaction Transaction;
        public uint? Height;
        public DateTime Time;
    }
}
