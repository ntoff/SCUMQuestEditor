# Quest Structure

A quest consists of the following mandatory properties:

- **AssociatedNPC**: Must be one of "Armorer", "Banker", "Barber", "Bartender", "Doctor", "Fisherman", "GeneralGoods" or "Mechanic"
- **Tier**: Integer between 1-3
- **Title**: Any string text
- **Description**: Any string text
- **TimeLimitHours**: Number greater than 0.0
- **RewardPool**: Array of Reward objects (at least one)
- **Conditions**: Array of Condition objects (at least one, maximum 5)

## Quest Tiers

- **Tier 1**: Easy quests with small rewards
- **Tier 2**: Medium difficulty with moderate rewards
- **Tier 3**: Difficult quests with significant rewards

## Time Limit

The TimeLimitHours property determines how long (in hours) the player has to complete the quest before it fails.
