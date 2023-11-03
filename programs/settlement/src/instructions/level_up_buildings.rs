use anchor_lang::prelude::*;

use crate::{
    config::{AGENTS_CONFIG, LEVELUP_COOLDOWN, LEVEL_UP_COST, LEVELUP_ACTION_ID},
    errors::SettlementError,
};

use super::add_building::GameAction;

pub fn level_up_buildings(ctx: Context<GameAction>, agent_id: u8) -> Result<()> {
    if agent_id as usize >= ctx.accounts.state.agents.len() {
        return err!(SettlementError::AgentOutOfBounds);
    }

    if AGENTS_CONFIG[ctx.accounts.state.agents[agent_id as usize].id as usize].action != u8::from(LEVELUP_ACTION_ID)
    {
        return err!(SettlementError::WrongAgentType);
    }

    let cost = LEVEL_UP_COST * ctx.accounts.state.buildings.len() as u32;
    if ctx.accounts.state.credits < cost {
        return err!(SettlementError::NotEnoughCredits);
    }

    if ctx.accounts.state.agents[agent_id as usize].cooldown > 0 {
        return err!(SettlementError::AgentOnCoolDown);
    }

    ctx.accounts.state.agents[agent_id as usize].cooldown = LEVELUP_COOLDOWN;

    ctx.accounts.state.credits -= cost;

    for existing_building in &mut ctx.accounts.state.buildings {
        existing_building.level += 1;
    }

    Ok(())
}
