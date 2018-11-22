using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Hitcon2018.Wallet
{
    public class TransactionHistory
    {
        public string BlockNumber { get; set; }
        public double TimeStamp { get; set; }
        public string Hash { get; set; }
        public string Nonce { get; set; }
        public string BlockHash { get; set; }
        public string From { get; set; }
        public string ContractAddress { get; set; }
        public string To { get; set; }
        public string Value { get; set; }
        public string TokenName { get; set; }
        public string TokenSymbol { get; set; }
        public string TokenDecimal { get; set; }
        public string TransactionIndex { get; set; }
        public string Gas { get; set; }
        public string GasPrice { get; set; }
        public string GasUsed { get; set; }
        public string CumulativeGasUsed { get; set; }
        public string Input { get; set; }
        public string Confirmations { get; set; }
        public string TokenValue => (BigInteger.Parse(Value) / 1000000000000000000).ToString();
        public string ShowToken => $"{TokenValue} - {TokenSymbol}";
        public DateTime TransactionDate => UnixTimeStampToDateTime(TimeStamp);


        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

    
}
