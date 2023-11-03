# space-grind
Space Grind virtual world

# Idea
Space Grind is an on-chain game in which you build a settlement on a distant planet and mine its resources.
The core game loop consists of settlement management (energy, resources, equipment) and actual mining outside the settlement, which requires risk assessment and planning.


## Settlement Management
The state of the settlement is stored in the user account. Well-optimised settlement allows to manage more mining vehicles/gear, recharge, and maintain them.

## Mining
The mining happens outside the settlement on a shared hex grid. A mining vehicle can occupy a cell, with chances to mine something increasing depending on the time it spends on it.
Mining itself requires energy and time. A stranded vehicle with depleted energy is burned, therefore it's important to plan the return to the settlement.

# Tech
Mining vehicles and mined resources would be SPL tokens to allow trading and possibly renting.
Technically it's a unity webGL build connected to anchor program via MagicBlock SDK
