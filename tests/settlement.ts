import * as anchor from "@project-serum/anchor";
import { Program } from "@project-serum/anchor";
import { Settlement } from "../target/types/settlement";
import { expect, assert } from 'chai'
import { Keypair, PublicKey, Connection } from "@solana/web3.js";
const bs58 = require('bs58');


import * as splToken from "@solana/spl-token";

describe("settlement", () => {


  const endpoint = 'https://api.devnet.solana.com';
  const solanaConnection = new Connection(endpoint);

  // Configure the client to use the local cluster.
  anchor.setProvider(anchor.AnchorProvider.env());

  const program = anchor.workspace.Settlement as Program<Settlement>;
  const programProvider = program.provider as anchor.AnchorProvider;

  const playerKeypair = Keypair.generate();


  console.log(bs58.encode(playerKeypair.secretKey));



  //player PDA
  const [gameStatePDA, _] = PublicKey.findProgramAddressSync(
    [
      anchor.utils.bytes.utf8.encode('state'),
      playerKeypair.publicKey.toBuffer(),
    ],
    program.programId
  )

  console.log("gameStatePDA", gameStatePDA.toBase58());

  var playerATA = null;
  var programATA = null;


  it("Inits!", async () => {

    await initNewPlayer();

  });


  it("resets!", async () => {

    try {
      const tx = await program.methods
        .reset()
        .accounts({ state: gameStatePDA, owner: playerKeypair.publicKey })
        .signers([playerKeypair])
        .rpc({ skipPreflight: true });
      console.log("Your transaction signature", tx);

      assert(true);

    } catch (e) {
      console.error(e);
    }

  });



  it("buys a builder!!", async () => { await buyAgent(0); });

  it("Adds a building!", async () => { await addBuilding(15, 12, 0, 0); });

  it("collects tax!", async () => {
    for (var i = 0; i < 4; i++)
      await collectTax();
  });

  it("buys a fixer!!", async () => { await buyAgent(1); });


  it("fixes!", async () => { await fixAll(1); });

  it("collects tax!", async () => {
    for (var i = 0; i < 4; i++)
      await collectTax();
  });

  it("fixes!", async () => { fixAll(1); });

  it("Adds more buildings!", async () => { await addBuilding(2, 12, 1, 0); });

  it("Prevents overlapping buildings!", async () => {
    try {
      await addBuilding(15, 12, 0, 0);
      // we use this to make sure we definitely throw an error
      assert(false, "should've failed but didn't ")
    } catch (_err) {
      console.log("_err.code", _err.code);
      expect(_err.code).to.equal(6000)
    }
  });

  it("collects tax!", async () => {
    for (var i = 0; i < 4; i++)
      await collectTax();
  });

  it("buys an enchanceer!!", async () => { await buyAgent(2); });

  it("levelsUp!", async () => { await levelUpAll(2) });
  it("fails to level up on cool down!", async () => {
    try {
      await levelUpAll(2)

      // we use this to make sure we definitely throw an error
      assert(false, "should've failed but didn't ")
    } catch (_err) {
      console.log("_err.code", _err.code);
      expect(_err.code).to.equal(6008)
    }
  });

  it("fixes!", async () => { await fixAll(1); });

  it("withdraws!!", async () => {

  const FT_MINT = new PublicKey('6H7CS53VHp9XFeEs3d5LRtySb9m6junhj1sBcVTGZVys');

    playerATA = await splToken.getOrCreateAssociatedTokenAccount(
      solanaConnection,
      playerKeypair,
      FT_MINT,
      playerKeypair.publicKey
    );

    programATA = await splToken.getOrCreateAssociatedTokenAccount(
      solanaConnection,
      playerKeypair,
      FT_MINT,
      programProvider.wallet.publicKey
    );

    let gameState = await program.account.gameState.fetch(gameStatePDA);
    var credits = gameState.credits;

    try {
      const tx = await program.methods
        .withdraw(10)
        .accounts({ state: gameStatePDA, tokenOwner: programProvider.wallet.publicKey, receiver: playerATA.address, sender: programATA.address, tokenProgram: splToken.TOKEN_PROGRAM_ID, owner: playerKeypair.publicKey })
        .signers([playerKeypair])
        .rpc({
          skipPreflight: true
        });

      console.log("Your transaction signature", tx);
    } catch (error) {
      console.log(error)
    }


    gameState = await program.account.gameState.fetch(gameStatePDA);
    expect(gameState.credits).to.equal(credits - 10);

  });



  it("deposits!!", async () => {

  const FT_MINT = new PublicKey('6H7CS53VHp9XFeEs3d5LRtySb9m6junhj1sBcVTGZVys');
    let gameState = await program.account.gameState.fetch(gameStatePDA);
    var credits = gameState.credits;

    try {
      const tx = await program.methods
        .deposit(1)
        .accounts({ state: gameStatePDA, tokenOwner: playerKeypair.publicKey, receiver: programATA.address, sender: playerATA.address, tokenProgram: splToken.TOKEN_PROGRAM_ID, owner: playerKeypair.publicKey })
        .signers([playerKeypair])
        .rpc({
          skipPreflight: true
        });

      console.log("Your transaction signature", tx);
    } catch (error) {
      console.log(error)
    }


    gameState = await program.account.gameState.fetch(gameStatePDA);
    expect(gameState.credits).to.equal(credits + 1);

  });

  after(async () => {
    console.log("FINAL STATE", await program.account.gameState.fetch(gameStatePDA));
  })



  async function initNewPlayer() {
    const signature = await solanaConnection.requestAirdrop(playerKeypair.publicKey, 1000000000);
    await solanaConnection.confirmTransaction(signature);

    try {
      const tx = await program.methods
        .initialise()
        .accounts({ state: gameStatePDA, owner: playerKeypair.publicKey })
        .signers([playerKeypair])
        .rpc({ skipPreflight: true });
      console.log("Your transaction signature", tx);
    }
    catch (e) {
      console.log(e);
    }

    let gameState = await program.account.gameState.fetch(gameStatePDA);

    console.log(gameState);
    expect(gameState.day).to.equal(0);
  }


  async function buyAgent(id) {
    let gameState = await program.account.gameState.fetch(gameStatePDA);
    var credits = gameState.credits;
    var agentsCount = gameState.agents.length;

    const tx = await program.methods
      .buyAgent(id)
      .accounts({ state: gameStatePDA, owner: playerKeypair.publicKey })
      .signers([playerKeypair])
      .rpc({
        skipPreflight: true, commitment: 'finalized'
      });

    console.log("Your transaction signature", tx);


    gameState = await program.account.gameState.fetch(gameStatePDA);
    expect(gameState.agents.length).to.be.above(agentsCount);
    expect(gameState.credits).to.be.below(credits);
  }


  async function mintAgent(id) {

    //these can be taken from IDL
    var mints = [
      new PublicKey("GH2K2QmzSNLsmnaA6HNsQ1eNehsuvpSyoQEvVMe2TQa"),
      new PublicKey("6DP2bBuFLVgjgQT2TSUgZqKz5ErBxdkUzZnkCLs3jQtr"),
      new PublicKey("3bjNmxDCcTj2jArspwsp7xmdKfXNauEkx3E5EKWC8vuF")
    ]

    let gameState = await program.account.gameState.fetch(gameStatePDA);
    console.log(gameState.credits);

    var playerNFTATA = await splToken.getOrCreateAssociatedTokenAccount(
      solanaConnection,
      playerKeypair,
      mints[id],
      playerKeypair.publicKey
    );

    console.log("playerNFTATA", playerNFTATA);

    const tx = await program.methods
      .mintAgent(id)
      .accounts({
        state: gameStatePDA,
        owner: playerKeypair.publicKey,
        nftReceiver: playerNFTATA.address,
        tokenProgram: splToken.TOKEN_PROGRAM_ID,
        nftTokenOwner: programProvider.wallet.publicKey,
        nftMint: mints[id]
      })
      .signers([playerKeypair])
      .rpc({
        skipPreflight: true
      });

    console.log("Your transaction signature", tx);

    gameState = await program.account.gameState.fetch(gameStatePDA);
    console.log(gameState.credits);

    assert(true);
  }

  async function collectTax() {
    let gameState = await program.account.gameState.fetch(gameStatePDA);
    var credits = gameState.credits;
    console.log("before tax buildings:", gameState.buildings);

    const tx = await program.methods
      .next()
      .accounts({ state: gameStatePDA, owner: playerKeypair.publicKey })
      .signers([playerKeypair])
      .rpc({
        skipPreflight: true
      });

    console.log("Your transaction signature", tx);


    gameState = await program.account.gameState.fetch(gameStatePDA);
    console.log("collectTax", gameState.credits);
    expect(gameState.credits).to.be.above(credits);
  }


  async function addBuilding(x, y, type, agentId) {
    let gameState = await program.account.gameState.fetch(gameStatePDA);
    var buildingCount = gameState.buildings.length;

    const tx = await program.methods
      .addBuilding(x, y, type, agentId)
      .accounts({ state: gameStatePDA, owner: playerKeypair.publicKey })
      .signers([playerKeypair])
      .rpc({ skipPreflight: true });
    console.log("Your transaction signature", tx);


    gameState = await program.account.gameState.fetch(gameStatePDA);
    console.log("addBuilding", gameState.buildings);
    expect(gameState.buildings.length).to.equal(buildingCount + 1);
  }

  async function fixAll(agentId) {
    let gameState = await program.account.gameState.fetch(gameStatePDA);
    var credits = gameState.credits;
    var state = gameState.buildings[0].state;

    const tx = await program.methods
      .fixBuildings(agentId)
      .accounts({ state: gameStatePDA, owner: playerKeypair.publicKey })
      .signers([playerKeypair])
      .rpc({
        skipPreflight: true, commitment: 'finalized'
      });

    console.log("Your transaction signature", tx);

    gameState = await program.account.gameState.fetch(gameStatePDA);
    expect(gameState.buildings[0].state).to.be.above(state);
    console.log("buildings after fix:", gameState.buildings);
    expect(gameState.credits).to.be.below(credits);
  }

  async function levelUpAll(agentId) {
    let gameState = await program.account.gameState.fetch(gameStatePDA);
    var credits = gameState.credits;
    var level = gameState.buildings[0].level;

    const tx = await program.methods
      .levelUpBuildings(agentId)
      .accounts({ state: gameStatePDA, owner: playerKeypair.publicKey })
      .signers([playerKeypair])
      .rpc({
        skipPreflight: true
      });

    console.log("Your transaction signature", tx);

    gameState = await program.account.gameState.fetch(gameStatePDA);
    expect(gameState.buildings[0].state).to.be.above(level + 1);
    expect(gameState.credits).to.be.below(credits);
  }

});


