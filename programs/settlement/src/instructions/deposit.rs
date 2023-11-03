use anchor_lang::prelude::*;
use anchor_spl::token::Transfer;

use crate::{errors::SettlementError, config::FT_MINT};

use super::TokenGameAction;


pub fn deposit(ctx: Context<TokenGameAction>, value: u8) -> Result<()> {
    require_keys_eq!(
        ctx.accounts.token_owner.key(),
        ctx.accounts.owner.key(),
        SettlementError::DepositTokenOwnerIsNotPlayer
    );

    msg!("balance : {}", ctx.accounts.sender.amount);

    if ctx.accounts.sender.amount < u64::from(value) {
        return err!(SettlementError::InsufficientTokenBalance);
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

    ctx.accounts.state.credits += u32::from(value);

    Ok(())
}
