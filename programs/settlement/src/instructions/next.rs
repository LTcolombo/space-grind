use anchor_lang::prelude::*;

use crate::config::BUILDINGS_CONFIG;

use super::add_building::GameAction;

pub fn next(ctx: Context<GameAction>) -> Result<()> {
    let mut tax: u8 = 0;
    for existing in ctx.accounts.state.buildings.iter_mut() {
        if existing.state > 0 {
            tax += existing.state * BUILDINGS_CONFIG[existing.id as usize].rate;
            existing.state -= 1;
        }
    }

    for agent in ctx.accounts.state.agents.iter_mut() {
        if agent.cooldown > 0 {
            agent.cooldown -= 1;
        }
    }

    ctx.accounts.state.day += 1;
    ctx.accounts.state.credits += u32::from(tax);
    Ok(())
}
