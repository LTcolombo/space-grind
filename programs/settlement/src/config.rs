use anchor_lang::prelude::*;
use solana_program::pubkey;

pub struct BuildingConfig {
    pub width: u8,
    pub height: u8,
    pub cost: u8,
    pub rate: u8,
}

pub const FT_MINT: Pubkey = pubkey!("6H7CS53VHp9XFeEs3d5LRtySb9m6junhj1sBcVTGZVys");

pub const MAP_WIDTH: u8 = 26;
pub const MAP_HEIGHT: u8 = 23;

pub const FIX_COST: u32 = 20;
pub const LEVEL_UP_COST: u32 = 100;


pub const PERFECT_STATE: u8 = 4;

pub const BUILD_COOLDOWN: u8 = 3;
pub const FIX_COOLDOWN: u8 = 6;
pub const LEVELUP_COOLDOWN: u8 = 9;


//todo enum
pub const BUILD_ACTION_ID: u8 = 0;
pub const FIX_ACTION_ID: u8 = 1;
pub const LEVELUP_ACTION_ID: u8 = 92;

#[constant]
pub const BUILDINGS_CONFIG: [BuildingConfig; 3] = [
    BuildingConfig {
        width: 4,
        height: 4,
        cost: 30,
        rate: 10,
    },
    BuildingConfig {
        width: 4,
        height: 4,
        cost: 60,
        rate: 20,
    },
    BuildingConfig {
        width: 4,
        height: 4,
        cost: 120,
        rate: 40,
    },
];

pub struct AgentConfig {
    pub cost: u8,
    pub action: u8,
    pub mint: Pubkey,
}

#[constant]
pub const AGENTS_CONFIG: [AgentConfig; 3] = [
    AgentConfig {
        cost: 20,
        action:BUILD_ACTION_ID,
        mint: pubkey!("GH2K2QmzSNLsmnaA6HNsQ1eNehsuvpSyoQEvVMe2TQa"),
    },
    AgentConfig {
        cost: 60,
        action:FIX_ACTION_ID,
        mint: pubkey!("6DP2bBuFLVgjgQT2TSUgZqKz5ErBxdkUzZnkCLs3jQtr"),
    },
    AgentConfig {
        cost: 200,
        action:LEVELUP_ACTION_ID,
        mint: pubkey!("3bjNmxDCcTj2jArspwsp7xmdKfXNauEkx3E5EKWC8vuF"),
    },
    // AgentConfig {
    //     cost: 30,
    //     mint: pubkey!("62JJ3djYyLkGRjRWH1VfJRDjUNXoWuUzeQu4ihP3Aa4N"),
    // },
];
