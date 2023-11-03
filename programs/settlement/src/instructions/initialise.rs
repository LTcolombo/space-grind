use anchor_lang::prelude::*;

use crate::{state::GameState};

pub fn initialise(ctx: Context<GameInit>) -> Result<()> {
    ctx.accounts.state.day = 0;
    ctx.accounts.state.credits = 50;
    Ok(())
}

#[derive(Accounts)]
pub struct GameInit<'info> {
    #[account(init, seeds = [b"state", owner.key().as_ref()], bump, payer = owner, space = 120)]
    pub state: Account<'info, GameState>,

    #[account(mut)]
    pub owner: Signer<'info>,
    pub system_program: Program<'info, System>,
}