// #define FTUE_TESTING

using System.Threading.Tasks;
using Service;
using Settlement.Program;
using Solana.Unity.Rpc.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils.Injection;

namespace Utils
{
    public class Bootstrap : InjectableBehaviour
    {
        [SerializeField] private Text label;

        [Inject] private WalletService _wallet;
        [Inject] private SettlementService _service;

        private async void Start()
        {
            label.text = "Sign In..";
            await _wallet.SignIn();
            HandleSignIn();
        }


        private async void HandleSignIn()
        {
            label.text = $"[{_wallet.AccountId}] Loading Player Data.. ";

            await _wallet.EnsureBalance();


#if FTUE_TESTING
            await _wallet.SignAndSend(new Transaction().Add(SettlementProgram.Reset(new ResetAccounts()
            {
                State = _wallet.PDA,
                Owner = _wallet.AccountId
            }, _wallet.ProgramId)));
            
            await DelayAsync(3);
#endif

            if (!await _service.ReloadData())
            {
                label.text = $"[{_wallet.AccountId}] Creating A New Player..";

                await _service.Initialise();

                Debug.Log("Initialised!");

                label.text = $"[{_wallet.AccountId}] New Player Created..";

                var delay = 1;

                for (var i = 0; i < 10; i++)
                {
                    await DelayAsync(delay);

                    if (await _service.ReloadData())
                        break;

                    delay += i;
                }
            }

            SceneManager.LoadScene("MainArea");
        }

        public static async Task DelayAsync(float secondsDelay)
        {
            var startTime = Time.time;
            while (Time.time < startTime + secondsDelay) await Task.Yield();
        }
    }
}