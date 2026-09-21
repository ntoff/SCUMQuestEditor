# Quest Rewards

## Reward Limits

Each quest can have a **maximum of 5 rewards**. The way rewards are counted:

1. **Currency**: Any combination of Normal Money, Gold, and Fame counts as **1 reward** total.
2. **Skills**: Each skill XP reward counts as **1 reward**.
3. **Trade Deals**: 
   - First trade deal counts as **2 rewards**
   - Each additional trade deal counts as **1 reward**

## Currency Rewards

- **CurrencyNormal**: Regular currency (Money)
- **CurrencyGold**: Premium gold currency
- **Fame**: Fame points for unlocking special trader items

## Skill Rewards

- **Skills**: Array of Skill objects
- **Skill Types**: Archery, Aviation, Awareness, Boxing, Camouflage, Cooking, Demolition, Driving, Endurance, Engineering, Farming, Handgun, Medical, MeleeWeapons, Motorcycle, Rifles, Running, Sniping, Stealth, Survival, Tactics, Thievery
- **Experience**: Amount of XP to award for the skill

## Trade Deal Rewards

- **Item**: Item name (must be from associated trader's inventory)
- **Price**: New discounted price (optional)
- **Amount**: Number of discounted items available
- **AllowExcluded**: Allow purchase of normally sell-only items
- **Fame**: Fame points required to buy the item (optional)

## Valid Reward Examples

- Currency + 2 Skills + 1 Trade Deal = 5 rewards
- Currency + 3 Trade Deals = 5 rewards
- Currency + 2 Skills + 1 Trade Deals = 5 rewards"