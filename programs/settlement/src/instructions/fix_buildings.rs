use anchor_lang::prelude::*;

use crate::{errors::SettlementError, config::{FIX_COST, AGENTS_CONFIG, PERFECT_STATE, FIX_COOLDOWN, FIX_ACTION_ID}};

use super::add_building::GameAction;


pub fn fix_buildings(ctx: Context<GameAction>, agent_id:u8) -> Result<()> {

    if agent_id as usize >= ctx.accounts.state.agents.len() {
        return err!(SettlementError::AgentOutOfBounds);
    }

    if AGENTS_CONFIG[ctx.accounts.state.agents[agent_id as usize].id as usize].action != u8::from(FIX_ACTION_ID){
        return err!(SettlementError::WrongAgentType);
    }

    let cost = FIX_COST * ctx.accounts.state.buildings.len() as u32;
    if ctx.accounts.state.credits < cost {
        return err!(SettlementError::NotEnoughCredits);
    }

    if ctx.accounts.state.agents[agent_id as usize].cooldown > 0 {
        return err!(SettlementError::AgentOnCoolDown);
    }

    ctx.accounts.state.agents[agent_id as usize].cooldown = FIX_COOLDOWN;

    ctx.accounts.state.credits -= cost;

    for existing_building in &mut ctx.accounts.state.buildings {
        existing_building.state = PERFECT_STATE;
    }

    Ok(())
}