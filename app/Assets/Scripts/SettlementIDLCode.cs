using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Solana.Unity;
using Solana.Unity.Programs.Abstract;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Wallet;
using Settlement;
using Settlement.Program;
using Settlement.Errors;
using Settlement.Accounts;
using Settlement.Types;

namespace Settlement
{
    namespace Accounts
    {
        public partial class GameState
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 8684738851132956304UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{144, 94, 208, 172, 248, 99, 134, 120};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "R9aG661U96X";
            public Building[] Buildings { get; set; }

            public Agent[] Agents { get; set; }

            public ushort Day { get; set; }

            public uint Credits { get; set; }

            public byte Bump { get; set; }

            public static GameState Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                GameState result = new GameState();
                int resultBuildingsLength = (int)_data.GetU32(offset);
                offset += 4;
                result.Buildings = new Building[resultBuildingsLength];
                for (uint resultBuildingsIdx = 0; resultBuildingsIdx < resultBuildingsLength; resultBuildingsIdx++)
                {
                    offset += Building.Deserialize(_data, offset, out var resultBuildingsresultBuildingsIdx);
                    result.Buildings[resultBuildingsIdx] = resultBuildingsresultBuildingsIdx;
                }

                int resultAgentsLength = (int)_data.GetU32(offset);
                offset += 4;
                result.Agents = new Agent[resultAgentsLength];
                for (uint resultAgentsIdx = 0; resultAgentsIdx < resultAgentsLength; resultAgentsIdx++)
                {
                    offset += Agent.Deserialize(_data, offset, out var resultAgentsresultAgentsIdx);
                    result.Agents[resultAgentsIdx] = resultAgentsresultAgentsIdx;
                }

                result.Day = _data.GetU16(offset);
                offset += 2;
                result.Credits = _data.GetU32(offset);
                offset += 4;
                result.Bump = _data.GetU8(offset);
                offset += 1;
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum SettlementErrorKind : uint
        {
            WontFit = 6000U,
            OutOfBounds = 6001U,
            NotEnoughCredits = 6002U,
            DepositTokenOwnerIsNotPlayer = 6003U,
            InsufficientTokenBalance = 6004U,
            MintMismatch = 6005U,
            AgentOutOfBounds = 6006U,
            WrongAgentType = 6007U
        }
    }

    namespace Types
    {
        public partial class Building
        {
            public byte X { get; set; }

            public byte Y { get; set; }

            public byte State { get; set; }

            public byte Id { get; set; }

            public byte Level { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WriteU8(X, offset);
                offset += 1;
                _data.WriteU8(Y, offset);
                offset += 1;
                _data.WriteU8(State, offset);
                offset += 1;
                _data.WriteU8(Id, offset);
                offset += 1;
                _data.WriteU8(Level, offset);
                offset += 1;
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out Building result)
            {
                int offset = initialOffset;
                result = new Building();
                result.X = _data.GetU8(offset);
                offset += 1;
                result.Y = _data.GetU8(offset);
                offset += 1;
                result.State = _data.GetU8(offset);
                offset += 1;
                result.Id = _data.GetU8(offset);
                offset += 1;
                result.Level = _data.GetU8(offset);
                offset += 1;
                return offset - initialOffset;
            }
        }

        public partial class Agent
        {
            public byte Cooldown { get; set; }

            public byte Id { get; set; }

            public byte Level { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WriteU8(Cooldown, offset);
                offset += 1;
                _data.WriteU8(Id, offset);
                offset += 1;
                _data.WriteU8(Level, offset);
                offset += 1;
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out Agent result)
            {
                int offset = initialOffset;
                result = new Agent();
                result.Cooldown = _data.GetU8(offset);
                offset += 1;
                result.Id = _data.GetU8(offset);
                offset += 1;
                result.Level = _data.GetU8(offset);
                offset += 1;
                return offset - initialOffset;
            }
        }
    }

    public partial class SettlementClient : TransactionalBaseClient<SettlementErrorKind>
    {
        public SettlementClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId) : base(rpcClient, streamingRpcClient, programId)
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<GameState>>> GetGameStatesAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = GameState.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<GameState>>(res);
            List<GameState> resultingAccounts = new List<GameState>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => GameState.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<GameState>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<GameState>> GetGameStateAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<GameState>(res);
            var resultingAccount = GameState.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<GameState>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeGameStateAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, GameState> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                GameState parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = GameState.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<RequestResult<string>> SendInitialiseAsync(InitialiseAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.SettlementProgram.Initialise(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendResetAsync(ResetAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.SettlementProgram.Reset(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendAddBuildingAsync(AddBuildingAccounts accounts, byte x, byte y, byte id, byte agentId, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.SettlementProgram.AddBuilding(accounts, x, y, id, agentId, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendFixBuildingsAsync(FixBuildingsAccounts accounts, byte agentId, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.SettlementProgram.FixBuildings(accounts, agentId, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendLevelUpBuildingsAsync(LevelUpBuildingsAccounts accounts, byte agentId, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.SettlementProgram.LevelUpBuildings(accounts, agentId, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendNextAsync(NextAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.SettlementProgram.Next(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendBuyAgentAsync(BuyAgentAccounts accounts, byte id, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.SettlementProgram.BuyAgent(accounts, id, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendWithdrawAsync(WithdrawAccounts accounts, byte value, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.SettlementProgram.Withdraw(accounts, value, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendDepositAsync(DepositAccounts accounts, byte value, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.SettlementProgram.Deposit(accounts, value, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendMintAgentAsync(MintAgentAccounts accounts, byte id, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.SettlementProgram.MintAgent(accounts, id, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        protected override Dictionary<uint, ProgramError<SettlementErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<SettlementErrorKind>>{{6000U, new ProgramError<SettlementErrorKind>(SettlementErrorKind.WontFit, "Supplied Building Overlaps With Existing One")}, {6001U, new ProgramError<SettlementErrorKind>(SettlementErrorKind.OutOfBounds, "Supplied Building Outisde Of Settlement Bounds")}, {6002U, new ProgramError<SettlementErrorKind>(SettlementErrorKind.NotEnoughCredits, "Not Enough Credits")}, {6003U, new ProgramError<SettlementErrorKind>(SettlementErrorKind.DepositTokenOwnerIsNotPlayer, "Deposit Token Owner Is Not Player")}, {6004U, new ProgramError<SettlementErrorKind>(SettlementErrorKind.InsufficientTokenBalance, "Insufficient Token Balance")}, {6005U, new ProgramError<SettlementErrorKind>(SettlementErrorKind.MintMismatch, "Supplied Token Accounts Dont Match The Mint")}, {6006U, new ProgramError<SettlementErrorKind>(SettlementErrorKind.AgentOutOfBounds, "Supplied Agent Id Is Not Owned")}, {6007U, new ProgramError<SettlementErrorKind>(SettlementErrorKind.WrongAgentType, "Supplied Agent Id Has A Different Action Type")}, };
        }
    }

    namespace Program
    {
        public class InitialiseAccounts
        {
            public PublicKey State { get; set; }

            public PublicKey Owner { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class ResetAccounts
        {
            public PublicKey State { get; set; }

            public PublicKey Owner { get; set; }
        }

        public class AddBuildingAccounts
        {
            public PublicKey State { get; set; }

            public PublicKey Owner { get; set; }
        }

        public class FixBuildingsAccounts
        {
            public PublicKey State { get; set; }

            public PublicKey Owner { get; set; }
        }

        public class LevelUpBuildingsAccounts
        {
            public PublicKey State { get; set; }

            public PublicKey Owner { get; set; }
        }

        public class NextAccounts
        {
            public PublicKey State { get; set; }

            public PublicKey Owner { get; set; }
        }

        public class BuyAgentAccounts
        {
            public PublicKey State { get; set; }

            public PublicKey Owner { get; set; }
        }

        public class WithdrawAccounts
        {
            public PublicKey State { get; set; }

            public PublicKey Owner { get; set; }

            public PublicKey TokenOwner { get; set; }

            public PublicKey Sender { get; set; }

            public PublicKey Receiver { get; set; }

            public PublicKey TokenProgram { get; set; }
        }

        public class DepositAccounts
        {
            public PublicKey State { get; set; }

            public PublicKey Owner { get; set; }

            public PublicKey TokenOwner { get; set; }

            public PublicKey Sender { get; set; }

            public PublicKey Receiver { get; set; }

            public PublicKey TokenProgram { get; set; }
        }

        public class MintAgentAccounts
        {
            public PublicKey State { get; set; }

            public PublicKey Owner { get; set; }

            public PublicKey NftTokenOwner { get; set; }

            public PublicKey NftMint { get; set; }

            public PublicKey NftReceiver { get; set; }

            public PublicKey TokenProgram { get; set; }
        }

        public static class SettlementProgram
        {
            public static Solana.Unity.Rpc.Models.TransactionInstruction Initialise(InitialiseAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.State, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(8510105477633722018UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction Reset(ResetAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.State, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(15488080923286262039UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction AddBuilding(AddBuildingAccounts accounts, byte x, byte y, byte id, byte agentId, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.State, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(7218354388676262534UL, offset);
                offset += 8;
                _data.WriteU8(x, offset);
                offset += 1;
                _data.WriteU8(y, offset);
                offset += 1;
                _data.WriteU8(id, offset);
                offset += 1;
                _data.WriteU8(agentId, offset);
                offset += 1;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction FixBuildings(FixBuildingsAccounts accounts, byte agentId, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.State, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(11460669261221901078UL, offset);
                offset += 8;
                _data.WriteU8(agentId, offset);
                offset += 1;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction LevelUpBuildings(LevelUpBuildingsAccounts accounts, byte agentId, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.State, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(5249582144202405041UL, offset);
                offset += 8;
                _data.WriteU8(agentId, offset);
                offset += 1;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction Next(NextAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.State, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(9329260747220385696UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction BuyAgent(BuyAgentAccounts accounts, byte id, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.State, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(16463669418962354266UL, offset);
                offset += 8;
                _data.WriteU8(id, offset);
                offset += 1;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction Withdraw(WithdrawAccounts accounts, byte value, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.State, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.TokenOwner, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Sender, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Receiver, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(2495396153584390839UL, offset);
                offset += 8;
                _data.WriteU8(value, offset);
                offset += 1;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction Deposit(DepositAccounts accounts, byte value, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.State, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.TokenOwner, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Sender, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Receiver, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(13182846803881894898UL, offset);
                offset += 8;
                _data.WriteU8(value, offset);
                offset += 1;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction MintAgent(MintAgentAccounts accounts, byte id, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.State, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.NftTokenOwner, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.NftMint, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.NftReceiver, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(15828912957389565918UL, offset);
                offset += 8;
                _data.WriteU8(id, offset);
                offset += 1;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}