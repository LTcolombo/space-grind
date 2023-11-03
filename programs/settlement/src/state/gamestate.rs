use anchor_lang::prelude::*;

#[derive(AnchorSerialize, AnchorDeserialize, Clone, PartialEq, Eq)]
pub struct Building {
    pub x: u8,
    pub y: u8,
    pub state: u8,
    pub id: u8,
    pub level: u8,
}

#[derive(AnchorSerialize, AnchorDeserialize, Clone, PartialEq, Eq)]
pub struct Agent {
    pub cooldown: u8,
    pub id: u8,
    pub level: u8,
}

#[account]
pub struct GameState {
    pub buildings: Vec<Building>,
    pub agents: Vec<Agent>,
    pub day: u16,
    pub credits: u32,
    pub bump: u8,
}