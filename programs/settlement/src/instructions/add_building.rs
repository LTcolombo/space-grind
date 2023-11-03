use anchor_lang::prelude::*;

use crate::config::*;

use crate::errors::SettlementError;
use crate::state::Building;
use crate::state::GameState;

pub fn add_building(ctx: Context<GameAction>, x: u8, y: u8, id: u8, agent_id: u8) -> Result<()> {
    fn overlap(
        existing_building: &Building,
        existing_config: &BuildingConfig,
        new_building: &Building,
        new_config: &BuildingConfig,
    ) -> bool {
        return std::cmp::min(
            existing_building.x + existing_config.width,
            new_building.x + new_config.width,
        ) > std::cmp::max(existing_building.x, new_building.x)
            && std::cmp::min(
                existing_building.y + existing_config.height,
                new_building.y + new_config.height,
            ) > std::cmp::max(existing_building.y, new_building.y);
    }

    if agent_id as usize >= ctx.accounts.state.agents.len() {
        return err!(SettlementError::AgentOutOfBounds);
    }

    if AGENTS_CONFIG[ctx.accounts.state.agents[agent_id as usize].id as usize].action != u8::from(BUILD_ACTION_ID){
        return err!(SettlementError::WrongAgentType);
    }

    let new_building_config = &BUILDINGS_CONFIG[id as usize];

    //check map bounds
    if x + new_building_config.width >= MAP_WIDTH {
        return err!(SettlementError::OutOfBounds);
    }

    if y + new_building_config.height >= MAP_HEIGHT {
        return err!(SettlementError::OutOfBounds);
    }

    let new_building = Building {
        x,
        y,
        id,
        state: PERFECT_STATE,
        level: 1,
    };

    for existing_building in &ctx.accounts.state.buildings {
        let existing_config = &BUILDINGS_CONFIG[existing_building.id as usize];

        if overlap(
            existing_building,
            existing_config,
            &new_building,
            new_building_config,
        ) {
            return err!(SettlementError::WontFit);
        }
    }

    let cost = u32::from(BUILDINGS_CONFIG[id as usize].cost);
    if ctx.accounts.state.credits < cost {
        return err!(SettlementError::NotEnoughCredits);
    }

    if ctx.accounts.state.agents[agent_id as usize].cooldown > 0 {
        return err!(SettlementError::AgentOnCoolDown);
    }

    ctx.accounts.state.agents[agent_id as usize].cooldown = BUILD_COOLDOWN;

    ctx.accounts.state.credits -= cost;

    ctx.accounts.state.buildings.push(new_building);
    Ok(())
}

#[derive(Accounts)]
pub struct GameAction<'info> {
    #[account(mut, seeds = [b"state", owner.key().as_ref()], bump)]
    pub state: Account<'info, GameState>,

    #[account(mut)]
    pub owner: Signer<'info>,
}
