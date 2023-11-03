use anchor_lang::prelude::*;
use anchor_spl::token::{Transfer, TokenAccount, Token};

use crate::{errors::SettlementError, state::GameState, config::FT_MINT};


pub fn withdraw(ctx: Context<TokenGameAction>, value: u8) -> Result<()> {
    if ctx.accounts.state.credits < u32::from(value) {
        return err!(SettlementError::NotEnoughCredits);
    }

    require_keys_eq!(
        ctx.accounts.sender.mint,
        FT_MINT,
        SettlementError::MintMismatch
    );

    require_keys_eq!(
        ctx.accounts.receiver.mint,
        FT_MINT,
        SettlementError::MintMismatch
    );

    // Create the Transfer struct for our context
    let transfer_instruction = Transfer {
        from: ctx.accounts.sender.to_account_info(),
        to: ctx.accounts.receiver.to_account_info(),
        authority: ctx.accounts.token_owner.to_account_info(),
    };

    let cpi_program = ctx.accounts.token_program.to_account_info();
    // Create the Context for our Transfer request
    let cpi_ctx = CpiContext::new(cpi_program, transfer_instruction);

    // Execute anchor's helper function to transfer tokens
    anchor_spl::token::transfer(cpi_ctx, u64::from(value))?;

    ctx.accounts.state.credits -= u32::from(value);

    Ok(())
}

#[derive(Accounts)]
pub struct TokenGameAction<'info> {
    #[account(mut, seeds = [b"state", owner.key().as_ref()], bump)]
    pub state: Account<'info, GameState>,
    #[account(mut)]
    pub owner: Signer<'info>,

    /// CHECK:
    #[account(mut)]
    pub token_owner: UncheckedAccount<'info>,

    #[account(mut)]
    pub sender: Account<'info, TokenAccount>,
    #[account(mut)]
    pub receiver: Account<'info, TokenAccount>,

    pub token_program: Program<'info, Token>,
}