use anchor_lang::prelude::*;
use instructions::*;

pub mod config;
pub mod errors;

pub mod instructions;
pub mod state;

declare_id!("4gtjA1MKqvEPgZDdFDPwnsWeGoA2o8aes4uhNkuy1LTq");

#[program]
pub mod settlement {
    use super::*;

    pub fn initialise(ctx: Context<GameInit>) -> Result<()> {
        instructions::initialise::initialise(ctx)
    }

    pub fn reset(ctx: Context<GameAction>) -> Result<()> {
        instructions::reset::reset(ctx)
    }

    pub fn add_building(ctx: Context<GameAction>, x: u8, y: u8, id: u8, agent_id: u8) -> Result<()> {
        instructions::add_building::add_building(ctx, x, y, id, agent_id)
    }

    pub fn fix_buildings(ctx: Context<GameAction>, agent_id: u8) -> Result<()> {
        instructions::fix_buildings::fix_buildings(ctx, agent_id)
    }

    pub fn level_up_buildings(ctx: Context<GameAction>, agent_id: u8) -> Result<()> {
        instructions::level_up_buildings::level_up_buildings(ctx, agent_id)
    }

    pub fn next(ctx: Context<GameAction>) -> Result<()> {
        instructions::next::next(ctx)
    }

    pub fn buy_agent(ctx: Context<GameAction>, id: u8) -> Result<()> {
        instructions::buy_agent::buy_agent(ctx, id)
    }

    pub fn withdraw(ctx: Context<TokenGameAction>, value: u8) -> Result<()> {
        instructions::withdraw::withdraw(ctx, value)
    }

    pub fn deposit(ctx: Context<TokenGameAction>, value: u8) -> Result<()> {
        instructions::deposit::deposit(ctx, value)
    }

    pub fn mint_agent(ctx: Context<NFTMintGameAction>, id: u8) -> Result<()> {
        instructions::mint_agent::mint_agent(ctx, id)
    }

}
