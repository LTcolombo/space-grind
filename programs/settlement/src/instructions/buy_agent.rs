use anchor_lang::prelude::*;

use crate::{errors::SettlementError, config::AGENTS_CONFIG, state::Agent};

use super::add_building::GameAction;

pub fn buy_agent(ctx: Context<GameAction>, id: u8) -> Result<()> {
    

    msg!("buy_agent{}", id);

    let agent = &AGENTS_CONFIG[id as usize];

    if ctx.accounts.state.credits < u32::from(agent.cost) {
        return err!(SettlementError::NotEnoughCredits);
    }

    ctx.accounts.state.credits -= u32::from(agent.cost);

    let new_agent:Agent = Agent {
        id,
        level: 1,
        cooldown: 0,
    };

    ctx.accounts.state.agents.push(new_agent);

    Ok(())
}

