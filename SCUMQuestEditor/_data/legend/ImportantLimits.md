# Important Limitations

## Rewards Limits

- Maximum 5 total rewards per quest
- Currency (Money + Gold + Fame combined) counts as 1 reward
- Each skill XP counts as 1 reward
- First trade deal counts as 2 rewards
- Additional trade deals count as 1 reward each

## Condition Limits

- Maximum 5 conditions per quest
- Minimum 1 condition per quest
- At least one condition must have SequenceIndex = 0

## Validation Requirements

- Elimination conditions must have at least one target character
- Fetch conditions must have at least one required item
- Interaction conditions must have at least one location

## Other Limitations

- Associated NPC must be one of the valid trader types
- Tier must be between 1-3
- Time limit must be greater than 0.0
- Title and Description are required
- Quest JSON file must contain only one Quest object

**Recommended Character Limits:**

- Quest Title: 30 characters
- Quest Description: 325 characters
- Condition Tracking Caption: 30 characters per requirement
- Reward Description: 30 characters per reward

**Note for custom servers:** If you don't need to support multiple languages, you can use up to twice these limits. However, it's recommended to check how the text appears in-game, as font sizes may vary.


## Server Impact

Remember that all quest customizations require a server restart to be applied.