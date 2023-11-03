use anchor_lang::prelude::*;
use anchor_spl::token::{TokenAccount, Token, MintTo};

use crate::{errors::SettlementError, config::AGENTS_CONFIG, state::GameState};

pub fn mint_agent(ctx: Context<NFTMintGameAction>, id: u8) -> Result<()> {
    

    msg!("mint_agent{}", id);

    let agent = &AGENTS_CONFIG[id as usize];

    if ctx.accounts.state.credits < u32::from(agent.cost) {
        return err!(SettlementError::NotEnoughCredits);
    }

    require_keys_eq!(*ctx.accounts.nft_mint.key, agent.mint, SettlementError::MintMismatch);

    ctx.accounts.state.credits -= u32::from(agent.cost);

    msg!("Initializing Mint Ticket");
    let cpi_accounts = MintTo {
        mint: ctx.accounts.nft_mint.to_account_info(),
        to: ctx.accounts.nft_receiver.to_account_info(),
        authority: ctx.accounts.nft_token_owner.to_account_info(),
    };
    msg!("CPI Accounts Assigned");
    let cpi_program = ctx.accounts.token_program.to_account_info();
    msg!("CPI Program Assigned");
    let cpi_ctx = CpiContext::new(cpi_program, cpi_accounts);
    msg!("CPI Context Assigned");
    anchor_spl::token::mint_to(cpi_ctx, 1)?;
    msg!("Token Minted !!!");



    Ok(())
}

#[derive(Accounts)]
pub struct NFTMintGameAction<'info> {
    #[account(mut, seeds = [b"state", owner.key().as_ref()], bump)]
    pub state: Account<'info, GameState>,
    #[account(mut)]
    pub owner: Signer<'info>,

    /// CHECK:
    #[account(mut)]
    pub nft_token_owner: Signer<'info>,


    /// CHECK:
    #[account(mut)]
    pub nft_mint: UncheckedAccount<'info>,
    #[account(mut)]
    pub nft_receiver: Account<'info, TokenAccount>,

    pub token_program: Program<'info, Token>,
}