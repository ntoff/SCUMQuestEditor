# Quest Conditions

Each quest can have between **1 and 5 conditions**. All conditions must be completed to finish the quest.

## Common Properties

- **Type**: "Elimination", "Fetch", or "Interaction"
- **CanBeAutoCompleted**: If false, player must return to trader
- **TrackingCaption**: Text displayed in quest tracking
- **SequenceIndex**: Order of completion (0 = first)
- **LocationsShownOnMap**: Areas shown on map for this condition

## Elimination Conditions

Kill specified targets:

- **TargetCharacters**: Array of character types to kill
- **Amount**: Number of characters to kill
- **AllowedWeapons**: Optional list of weapons that must be used

## Fetch Conditions

Collect specified items:

- **DisablePurchaseOfRequiredItems**: Prevent players from buying required items
- **PlayerKeepsItems**: If true, items aren't removed on completion
- **RequiredItems**: Array of items to collect with specific requirements

## Interaction Conditions

Interact with objects in the world:

- **Locations**: Array of location objects to interact with
- **MinNeeded**: Minimum interactions required
- **MaxNeeded**: Maximum interactions required
- **SpawnOnlyNeeded**: Only spawn required number of objects
- **WorldMarkerShowDistance**: Distance at which markers appear (meters)

## Sequence Indexes

- At least one condition must have SequenceIndex = 0
- Conditions are completed in order of their SequenceIndex
- Multiple conditions with the same SequenceIndex are active simultaneously