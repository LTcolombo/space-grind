using System;
using System.Linq;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;
using Settlement.Accounts;
using Settlement.Program;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using UnityEngine;
using Utils.Injection;
using View;

namespace Service
{
    [Singleton]
    public class SettlementService : InjectableObject<SettlementService>
    {
        [Inject] private WalletService _wallet;

        [Inject] private BalanceModel _balance;
        [Inject] private BuildingsModel _buildings;
        [Inject] private AgentsModel _agents;
        [Inject] private ConfigService _config;

        public async Task<bool> ReloadData()
        {
            Debug.Log("ReloadData");
            
            var gameDataRaw =
                await _wallet.Rpc.GetAccountInfoAsync(_wallet.PDA, Commitment.Confirmed, BinaryEncoding.JsonParsed);

            
            Debug.Log(JsonConvert.SerializeObject(gameDataRaw.Result.Value.Data));
            Debug.Log(gameDataRaw.ErrorData);
            
            if (gameDataRaw.ErrorData != null)
            {
                Debug.LogError(gameDataRaw.ErrorData.Error); //add more info if needed
                return false;
            }

            Debug.Log("Dimmer.Visible = false");
            
            Dimmer.Visible = false;
            
            if (gameDataRaw.Result?.Value?.Data?.Count > 0)
            {
                var gameData = GameState.Deserialize(Convert.FromBase64String(gameDataRaw.Result.Value.Data[0]));

                var agentConfigs = _config.GetAgents();

                _balance.Set(gameData.Credits);
                _buildings.Set(gameData.Buildings);
                _agents.Set(gameData.Agents);

                return true;
            }

            return false;
        }

        public async Task<bool> Initialise()
        {
            return await _wallet.SignAndSend(new Transaction().Add(SettlementProgram.Initialise(new InitialiseAccounts()
            {
                State = _wallet.PDA,
                Owner = _wallet.AccountId,
                SystemProgram = SystemProgram.ProgramIdKey
            }, _wallet.ProgramId)));
        }

        public async Task<bool> PlaceBuilding(byte x, byte y, byte id)
        {
            return await _wallet.SignAndSend(new Transaction().Add(SettlementProgram.AddBuilding(new AddBuildingAccounts
            {
                State = _wallet.PDA,
                Owner = _wallet.AccountId
            }, x, y, id, DialogueTrigger.CurrentDialogue, _wallet.ProgramId)));
        }

        public async Task<bool> SubmitTurn()
        {
            return await _wallet.SignAndSend(new Transaction().Add(SettlementProgram.Next(new NextAccounts
            {
                State = _wallet.PDA,
                Owner = _wallet.AccountId
            }, _wallet.ProgramId)));
        }

        public async Task<bool> FixBuildings()
        {
            return await _wallet.SignAndSend(new Transaction().Add(SettlementProgram.FixBuildings(
                new FixBuildingsAccounts()
                {
                    State = _wallet.PDA,
                    Owner = _wallet.AccountId
                }, DialogueTrigger.CurrentDialogue, _wallet.ProgramId)));
        }

        public async Task<bool> LevelUpBuildings()
        {
            return await _wallet.SignAndSend(new Transaction().Add(SettlementProgram.LevelUpBuildings(
                new LevelUpBuildingsAccounts()
                {
                    State = _wallet.PDA,
                    Owner = _wallet.AccountId
                }, DialogueTrigger.CurrentDialogue, _wallet.ProgramId)));
        }

        public async Task<bool> BuyAgent(byte id)
        {
            return await _wallet.SignAndSend(new Transaction().Add(SettlementProgram.BuyAgent(new BuyAgentAccounts
            {
                State = _wallet.PDA,
                Owner = _wallet.AccountId
            }, id, _wallet.ProgramId)));
        }
    }
}