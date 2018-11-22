using Hitcon2018.Wallet;
using Nethereum.Web3;
using System;
using System.Text;
using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Fact]
        public async void GetBalance()
        {
            //var htoaddress = BitConverter.ToString(Encoding.Default.GetBytes("0xFbF065C72Ad57611d26DCcd5041806fB5A988D92"));
            //var a = "01" + String.Format("%02X", htoaddress.Length / 2) + htoaddress;
            string privateKey = "768737baf760264de8b8f9352ed135af8b7f69b9a6158ff616a00d81e03ab71a";
            //var account = new Nethereum.Web3.Accounts.Account(privateKey);
            //var eth = new EthService(new Web3(account));

            var account = EthService.Instance.GetAddressByPrivateKey(privateKey);
            //Test();
            var balance = await EthService.Instance.GetBalance(account);
            Console.WriteLine("the balance is :" + balance);
            Assert.True(balance > 0);
        }

        [Fact]
        public async void Transaction()
        {
            string privateKey = "768737baf760264de8b8f9352ed135af8b7f69b9a6158ff616a00d81e03ab71a";
            var account = new Nethereum.Web3.Accounts.Account(privateKey);
            //var eth = new EthService(new Web3(account));
            var result = await EthService.Instance.Transfer(privateKey, "0xFbF065C72Ad57611d26DCcd5041806fB5A988D92", "0xea7A01878d1886A7c55db3200144855A9547E905", 1000000000000000000, 300000, 99);
            Console.WriteLine(result);
            Assert.True(result != null);
        }

        [Fact]
        public async void Pairing()
        {
            string pairingString = "hitcon://pair?v=18&a=808c2257d778e5f1340d9325116f5a7273b33f5d&k=4eeaa8ed990541516a6319b145a738e1&s=1c4bd84d-0ee1-f0a6-0d16-9882238e3322&c=2a68fc53650832b34076697b3bdf3fffa53e5888ceee6d4a";
            var scanModel = new ScanQRCcodeViewModel();
            scanModel.Pair(pairingString);
        }


        [Fact]
        public async void TransactionHistory()
        {
            var result = await EthService.Instance.GetTransactionsAsync("0xFbF065C72Ad57611d26DCcd5041806fB5A988D92");
            Console.WriteLine(result);
            Assert.True(result != null);
        }

        
    }
}
