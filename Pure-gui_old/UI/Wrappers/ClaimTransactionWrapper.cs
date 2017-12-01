using Pure.Core;
using System.Collections.Generic;
using System.Linq;

namespace Pure.UI.Wrappers
{
    internal class ClaimTransactionWrapper : TransactionWrapper
    {
        public List<CoinReferenceWrapper> Claims { get; set; } = new List<CoinReferenceWrapper>();

        public override Transaction Unwrap()
        {
            ClaimTransaction tx = (ClaimTransaction)base.Unwrap();
            tx.Claims = Claims.Select(p => p.Unwrap()).ToArray();
            return tx;
        }
    }
}
