use anchor_lang::prelude::*;

use super::add_building::GameAction;


//this is used for FTUE testing purposes. todo remove
pub fn reset(ctx: Context<GameAction>) -> Result<()> {
    ctx.accounts.state.day = 0;
    ctx.accounts.state.credits = 50;

    ctx.accounts.state.buildings.clear();
    ctx.accounts.state.agents.clear();
    Ok(())
}
