using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pure.Core
{
    public abstract class Blockchain
    {
        public static readonly UInt160 PureIssuer = "AWAm7VzveC4qPFxnF55nLPjeD9DAEDQZZB".ToScriptHash();

        public static readonly RegisterTransaction PureRT = new RegisterTransaction
        {
            RegisterType = RegisterType.System,
            RegisterName = "<names><name lang=\"zh-CHS\" value=\"小蚁股\"/><name lang=\"en\" value=\"PureRT\"/></names>",
            Amount = 100000000,
            Issuer = PureIssuer,
            Admin = PureIssuer, //Before publish, we have to convert admin with super.
            Inputs = new TransactionInput[0],
            Outputs = new TransactionOutput[0] //Premine is not determined.
        };

        public static readonly Block GenesisBlock = new Block
        {
            PrevBlock = new UInt256(),
            Timestamp = DateTime.Now.ToTimestamp(), //Set the GenesisBlock Time with now.
            Nonce = 2083236893, //Nonce.
            Miner = "AWAm7VzveC4qPFxnF55nLPjeD9DAEDQZZB".ToScriptHash(), // Set the Mine Address.
            Transactions = new Transaction[]
            {
                new GenerationTransaction
                {
                    Nonce = 2083236893,
                    Inputs = new TransactionInput[0],
                    Outputs = new TransactionOutput[]
                    {
                        new TransactionOutput
                        {

                        }
                    }
                }
            }
        };

        static Blockchain()
        {
            //1. Pure resource have to be signed.
            //2. Pure coin resource have to be registered.
            //3. Pure structure.
        }
    }
}
