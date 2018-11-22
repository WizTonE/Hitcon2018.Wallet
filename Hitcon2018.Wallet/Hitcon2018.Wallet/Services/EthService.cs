using Acr.UserDialogs;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Hitcon2018.Wallet
{
    [Function("transfer", "bool")]
    public class TransferFunction : FunctionMessage
    {
        [Parameter("address", "_to", 1)]
        public string To { get; set; }

        [Parameter("uint256", "_value", 2)]
        public BigInteger TokenAmount { get; set; }
    }
    public class EthService
    {
        //private readonly string _hitConCoinContract = @"[{""constant"":true,""inputs"":[],""name"":""name"",""outputs"":[{""name"":"""",""type"":""string""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_spender"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""approve"",""outputs"":[{""name"":""success"",""type"":""bool""}],""type"":""function""},{""constant"":true,""inputs"":[],""name"":""totalSupply"",""outputs"":[{""name"":"""",""type"":""uint256""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_from"",""type"":""address""},{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""transferFrom"",""outputs"":[{""name"":""success"",""type"":""bool""}],""type"":""function""},{""constant"":true,""inputs"":[],""name"":""decimals"",""outputs"":[{""name"":"""",""type"":""uint8""}],""type"":""function""},{""constant"":true,""inputs"":[],""name"":""version"",""outputs"":[{""name"":"""",""type"":""string""}],""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""type"":""function""},{""constant"":true,""inputs"":[],""name"":""symbol"",""outputs"":[{""name"":"""",""type"":""string""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""name"":""success"",""type"":""bool""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_spender"",""type"":""address""},{""name"":""_value"",""type"":""uint256""},{""name"":""_extraData"",""type"":""bytes""}],""name"":""approveAndCall"",""outputs"":[{""name"":""success"",""type"":""bool""}],""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""},{""name"":""_spender"",""type"":""address""}],""name"":""allowance"",""outputs"":[{""name"":""remaining"",""type"":""uint256""}],""type"":""function""},{""inputs"":[{""name"":""_initialAmount"",""type"":""uint256""},{""name"":""_tokenName"",""type"":""string""},{""name"":""_decimalUnits"",""type"":""uint8""},{""name"":""_tokenSymbol"",""type"":""string""}],""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""_from"",""type"":""address""},{""indexed"":true,""name"":""_to"",""type"":""address""},{""indexed"":false,""name"":""_value"",""type"":""uint256""}],""name"":""Transfer"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""_owner"",""type"":""address""},{""indexed"":true,""name"":""_spender"",""type"":""address""},{""indexed"":false,""name"":""_value"",""type"":""uint256""}],""name"":""Approval"",""type"":""event""}]";
        //private readonly string _hitConCoinContract = "0xCAa21697717Cc1dd1F119FEeE7e17702A764dE56";

        public string HitConCoinContract { get; } = "0x503b0C139665E7e9F863ba1BCf0e635a2E87aA5b";


        readonly string[] apiTokens = {
            "VSURNRD5YD8UCGM74HHEBBZG8UQ5WRKX1E",
            "EZ9SRW458YSBPZWEXRF1Q35NWZY5UXZQ41",
            "R6HQ6DGNKEYUGJAR4R436CUKVRVYABY37N",
            "96YIYNNJDKFXMI2DI16ZDWEZEMHH792IS3",
            "97XM5ASTHEI9R51UYJA19ANJPFF8QEE2CW",
            "5E1RVYJB26DJKWJ584GYRJP9A7UA2PP12Y",
            "ITMVEEAZV2PBC7B7AK4HAYIDYQUI33ZHVW",
            "PTV1BXQMS6XW5XGZYR3XV2FMTKNQF5SBMX",
            "DYR1N3F6DU5TRVSGSAXIUEKVGB54RGMQH4",
            "AFW8K49CZDIC8MH2ZSMZ9YCWJBYCEX7FBN"
        };

        private readonly Web3 web3;
        //private string abi = @"[{""constant"":false,""inputs"":[{""name"":""symbol"",""type"":""bytes32""}],""name"":""getToken"",""outputs"":[{""name"":"""",""type"":""address""}],""type"":""function""}]";
        //private string privateKey = "768737baf760264de8b8f9352ed135af8b7f69b9a6158ff616a00d81e03ab71a";
        //private string apiKeyToken = "VSURNRD5YD8UCGM74HHEBBZG8UQ5WRKX1E";
        private readonly RpcClient client = new RpcClient(new Uri($"https://api-ropsten.etherscan.io"));
        

        private Contract contract;

        private static readonly Random Rnd = new Random();

        private static EthService instance;
        public static EthService Instance { get { return instance ?? (instance = new EthService()); } }

        public Contract Contract { get => contract; set => contract = value; }

        private EthService()
        {
            
        }

        //public EthService(Web3 web3)
        //{
        //    //var account = new Nethereum.Web3.Accounts.Account(privateKey);
        //    this.web3 = web3;
            
            
        //    //this.web3 = new Web3("https://api.myetherapi.com/eth");
        //    //this.web3 = new Web3(account, "http://api-ropsten.etherscan.io");
        //    //this.web3 = new Web3("http://api-ropsten.etherscan.io");
        //    //var contract = web3.Eth.GetContract(abi, "http://api-ropsten.etherscan.io");
        //    //balanceFunction = contract.GetFunction("balanceOf");
        //    //this.contract = web3.Eth.GetContract(abi, address);
        //}

        public string GetAddressByPrivateKey(string privateKey)
        {
            return new Account(privateKey).Address;
        }

        public async Task<BigInteger> GetTokenBalance(string address, string tokenAddress)
        {
            var token = default(BigInteger);
            try
            {
                token = await client.SendRequestAsync<BigInteger>("", "http://api-ropsten.etherscan.io/api?module=account&action=tokenbalance&address=" + address + "&contractaddress=" + tokenAddress + "&startblock=0&endblock=999999999&sort=asc&apikey=" + apiTokens[Rnd.Next(9)]);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return token;
        }

        public async Task<BigInteger> GetBalance(string address)
        {
            //var balance = await web3.Eth.GetBalance.SendRequestAsync(address);
            //var balance = await client.SendRequestAsync<HexBigInteger>("ethsupply", "stats",new object[] { new { apiKey = apiKeyToken} });
            //var balance = await balanceFunction.CallAsync<int>(address);
            //Console.WriteLine(balance);
            var balance = default(BigInteger);
            try
            {
                balance = await client.SendRequestAsync<BigInteger>("", "https://api-ropsten.etherscan.io/api?module=account&action=balance&address=" + address + "&tag=latest&apikey=apikey=" + apiTokens[Rnd.Next(9)]);
                Console.WriteLine(balance);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return balance;
        }

        public async Task<BigInteger> GetNonceByAddress(string address)
        {
            var txCount = await client.SendRequestAsync<string>("", "https://api-ropsten.etherscan.io/api?module=proxy&action=eth_getTransactionCount&address=" + address + "&tag=latest&apikey=" + apiTokens[Rnd.Next(9)]);
            return BigInteger.Parse(Convert.ToInt32(txCount, 16).ToString());
        }

        public async Task<object> Transfer(string privateKey, string address, string receiveAddress, BigInteger tokenAmount, BigInteger gas, BigInteger gasPrice)
        {
            //client.SendRequestAsync<string>("", "https://api-ropsten.etherscan.io/api?module=transaction&action=getstatus&txhash=0x15f8e5ea1079d9a0bb04a4c58ae5fe7654b5b2b4463375ff7ffb490aa0032f3a&apikey=VSURNRD5YD8UCGM74HHEBBZG8UQ5WRKX1E");
            //var contract = await client.SendRequestAsync<string>("", "https://api-ropsten.etherscan.io/api?module=contract&action=getabi&address=0xCAa21697717Cc1dd1F119FEeE7e17702A764dE56&apikey=VSURNRD5YD8UCGM74HHEBBZG8UQ5WRKX1E");
            var web3 = new Web3(new Account(privateKey));
            var txCount = await client.SendRequestAsync<string>("", "https://api-ropsten.etherscan.io/api?module=proxy&action=eth_getTransactionCount&address=" + address + "&tag=latest&apikey=" + apiTokens[Rnd.Next(9)]);
            //File.AppendAllText(@"C:\HitconLog.txt", $"txCount: {txCount}" + Environment.NewLine);
            //var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address);
            var transfer = new TransferFunction()
            {
                To = receiveAddress,
                TokenAmount = tokenAmount,
                Nonce = BigInteger.Parse(Convert.ToInt32(txCount, 16).ToString()),
                Gas = gas,
                GasPrice = gasPrice,
            };
            //var encoded = await web3.Eth.GetContractHandler("0xCAa21697717Cc1dd1F119FEeE7e17702A764dE56").SignTransactionAsync(transfer);
            var encoded = await web3.Eth.GetContractHandler(HitConCoinContract).SignTransactionAsync(transfer);
            //File.AppendAllText(@"C:\HitconLog.txt", $"encoded: {encoded}" + Environment.NewLine);
            //var encoded = Web3.OfflineTransactionSigner.SignTransaction(privateKey, receiveAddress, 10, BigInteger.Parse(txCount.Replace("0x","")),99,300000);

            var txId = await client.SendRequestAsync<string>("", "https://api-ropsten.etherscan.io/api?module=proxy&action=eth_sendRawTransaction&hex="+ encoded + "&apikey=" + apiTokens[Rnd.Next(9)]);
            //var txId = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encoded);
            var receipt = await client.SendRequestAsync<object>("", "https://api-ropsten.etherscan.io/api?module=proxy&action=eth_getTransactionReceipt&txhash="+ txId + "&apikey=" + apiTokens[Rnd.Next(9)]);
            //var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
            while (receipt == null)
            {
                Thread.Sleep(1000);
                receipt = await client.SendRequestAsync<object>("", "https://api-ropsten.etherscan.io/api?module=proxy&action=eth_getTransactionReceipt&txhash="+ txId + "&apikey=" + apiTokens[Rnd.Next(9)]);
            }
            return receipt;
        }

        public async Task<object> SendTransaction(string encoded)
        {
            if(encoded.Length < 5)
            {
                UserDialogs.Instance.Alert("TxN Error");
                return null;
            }
            var receipt = default(object);
            try
            {
                var txId = await client.SendRequestAsync<string>("", "https://api-ropsten.etherscan.io/api?module=proxy&action=eth_sendRawTransaction&hex=" + encoded + "&apikey=" + apiTokens[Rnd.Next(9)]);

                //var txId = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encoded);
                receipt = await client.SendRequestAsync<object>("", "https://api-ropsten.etherscan.io/api?module=proxy&action=eth_getTransactionReceipt&txhash=" + txId + "&apikey=" + apiTokens[Rnd.Next(9)]);
                //var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
                while (receipt == null)
                {
                    Thread.Sleep(1000);
                    receipt = await client.SendRequestAsync<object>("", "https://api-ropsten.etherscan.io/api?module=proxy&action=eth_getTransactionReceipt&txhash=" + txId + "&apikey=" + apiTokens[Rnd.Next(9)]);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            return receipt;
        }

        public async Task<List<TransactionHistory>> GetTransactionsAsync(string address)
        {
            var transactionHistory = await client.SendRequestAsync<TransactionHistory[]>("", "http://api-ropsten.etherscan.io/api?module=account&action=tokentx&address=" + address + "&startblock=0&endblock=999999999&sort=asc&apikey=" + apiTokens[Rnd.Next(9)]);
            return transactionHistory.ToList();
        }


        public async Task<string> GetAccount()
        {
            var account = await web3.Eth.CoinBase.SendRequestAsync();
            Console.WriteLine(account);
            return account;
        }

        public Function GetGetTokenFunction()
        {
            return Contract.GetFunction("getToken");

        }
        public async Task<string> GetTokenAsyncCall(string symbol)
        {
            var function = GetGetTokenFunction();
            return await function.CallAsync<string>(symbol);
        }
        public async Task<string> GetTokenAsync(string addressFrom, string symbol, HexBigInteger gas = null, HexBigInteger valueAmount = null)
        {
            var function = GetGetTokenFunction();
            return await function.SendTransactionAsync(addressFrom, gas, valueAmount, symbol);

        }

        //public async Task<StandardTokenService> GetEthTokenServiceAsync(string symbol)
        //{
        //    var tokenAddress = await GetTokenAsyncCall(symbol);
        //    return new StandardTokenService(web3, tokenAddress);
        //}
    }
}
