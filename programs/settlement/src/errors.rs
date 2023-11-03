use anchor_lang::error_code;

#[error_code]
pub enum SettlementError {
    #[msg("Supplied Building Overlaps With Existing One")]
    WontFit,
    #[msg("Supplied Building Outisde Of Settlement Bounds")]
    OutOfBounds,
    #[msg("Not Enough Credits")]
    NotEnoughCredits,
    #[msg("Deposit Token Owner Is Not Player")]
    DepositTokenOwnerIsNotPlayer,
    #[msg("Insufficient Token Balance")]
    InsufficientTokenBalance,
    #[msg("Supplied Token Accounts Dont Match The Mint")]
    MintMismatch,
    #[msg("Supplied Agent Id Is Not Owned")]
    AgentOutOfBounds,
    #[msg("Supplied Agent Id Has A Different Action Type")]
    WrongAgentType,
    #[msg("Supplied Agent Id Is On CoolDown")]
    AgentOnCoolDown
}
