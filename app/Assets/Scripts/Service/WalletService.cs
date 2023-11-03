using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;

namespace Service
{
    [Singleton]
    public class WalletService
    {
        private PrivateKey _keypair;
        private PublicKey _pda;

        public readonly PublicKey ProgramId = new("4gtjA1MKqvEPgZDdFDPwnsWeGoA2o8aes4uhNkuy1LTq");


        // ReSharper disable once InconsistentNaming (little you know)
        public PublicKey PDA => _pda;
        public IRpcClient Rpc { get; private set; }
        public PublicKey AccountId { get; private set; }

        public async Task SignIn()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        var account = await Web3.Instance.LoginPhantom();
        Rpc = Web3.Instance.Wallet.ActiveRpcClient;
        
        _keypair = account;
        AccountId = account;

        PublicKey.TryFindProgramAddress(new[]
            {
                Encoding.UTF8.GetBytes("state"),
                AccountId.KeyBytes
            },
            ProgramId, out _pda, out var bump);
#else

            Rpc = ClientFactory.GetClient(Cluster.DevNet);
            
            var reader = new StreamReader("Assets/creds.txt");
            _keypair = new PrivateKey(await reader.ReadLineAsync());
            AccountId = new PublicKey(await reader.ReadLineAsync());
            reader.Close();

            PublicKey.TryFindProgramAddress(new[]
                {
                    Encoding.UTF8.GetBytes("state"),
                    AccountId.KeyBytes
                },
                ProgramId, out _pda, out var bump);
#endif
        }

        //we dont use wallets built in signin funciton so we can use a hardcoded private key (e.g. in editor)
        public async Task<bool> SignAndSend(Transaction tx)
        {
            Dimmer.Visible = true;
            
            tx.FeePayer = AccountId;
            var hash = await Rpc.GetRecentBlockHashAsync();
            tx.RecentBlockHash = hash.Result.Value.Blockhash;
            tx.Signatures = new List<SignaturePubKeyPair>();
            
#if UNITY_EDITOR

            var message = tx.CompileMessage();
            var numArray = _keypair.Sign(message);
            tx.Signatures.Add(new SignaturePubKeyPair
            {
                PublicKey = AccountId,
                Signature = numArray
            });

            var result = await Rpc.SendTransactionAsync(
                Convert.ToBase64String(tx.Serialize()),
                true, Commitment.Finalized);

#else
            Debug.Log("SignAndSendTransaction Before");
            var result = await Web3.Instance.Wallet.SignAndSendTransaction(tx);
            Debug.Log("SignAndSendTransaction Done");
#endif
            Debug.Log(result?.RawRpcResponse);

            if (!result?.WasSuccessful ?? false)
            {
                Debug.LogError(result.RawRpcResponse);
            }

            return result.WasSuccessful;
        }

        public async Task EnsureBalance()
        {
            if ((await Rpc.GetBalanceAsync(AccountId)).Result.Value < 500000000)
                await Rpc.RequestAirdropAsync(AccountId, 1000000000);
        }
    }
}