#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using SCUMQuestEditor.Models;

namespace SCUMQuestEditor.Converters
{
    public class ConditionConverter : JsonConverter<Condition>
    {
        public override Condition? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            JsonElement element = JsonSerializer.Deserialize<JsonElement>(ref reader);

            if (!element.TryGetProperty("Type", out JsonElement typeElement))
            {
                throw new JsonException("Condition JSON must contain a 'Type' property.");
            }

            string? conditionType = typeElement.GetString();
            if (string.IsNullOrEmpty(conditionType))
            {
                throw new JsonException("Condition 'Type' property is missing or empty.");
            }

            System.Type targetType = conditionType switch
            {
                "Elimination" => typeof(EliminationCondition),
                "Fetch" => typeof(FetchCondition),
                "Interaction" => typeof(InteractionCondition),
                _ => throw new JsonException($"Unknown condition type: {conditionType}")
            };

            var result = JsonSerializer.Deserialize(element.GetRawText(), targetType, options);

            if (result is not Condition condition)
            {
                throw new JsonException($"Deserialized object is not a Condition (got {result?.GetType()?.Name ?? "null"}).");
            }

            return condition;
        }

        public override void Write(Utf8JsonWriter writer, Condition value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("TrackingCaption", value.TrackingCaption);
            writer.WriteNumber("SequenceIndex", value.SequenceIndex);
            writer.WriteBoolean("CanBeAutoCompleted", value.CanBeAutoCompleted);
            writer.WriteString("Type", value.Type);

            if (value is EliminationCondition elimination)
            {
                writer.WriteNumber("Amount", elimination.Amount);

                if (elimination.TargetCharacters != null && elimination.TargetCharacters.Count > 0)
                {
                    writer.WriteStartArray("TargetCharacters");
                    foreach (var item in elimination.TargetCharacters) writer.WriteStringValue(item);
                    writer.WriteEndArray();
                }

                if (elimination.AllowedWeapons != null && elimination.AllowedWeapons.Count > 0)
                {
                    writer.WriteStartArray("AllowedWeapons");
                    foreach (var item in elimination.AllowedWeapons) writer.WriteStringValue(item);
                    writer.WriteEndArray();
                }
            }
            else if (value is FetchCondition fetch)
            {
                writer.WriteBoolean("DisablePurchaseOfRequiredItems", fetch.DisablePurchaseOfRequiredItems);
                writer.WriteBoolean("PlayerKeepsItems", fetch.PlayerKeepsItems);

                if (fetch.RequiredItems != null && fetch.RequiredItems.Count > 0)
                {
                    writer.WriteStartArray("RequiredItems");
                    foreach (var item in fetch.RequiredItems)
                    {
                        writer.WriteStartObject();
                        writer.WriteStartArray("AcceptedItems");
                        foreach (var acceptedItem in item.AcceptedItems) writer.WriteStringValue(acceptedItem);
                        writer.WriteEndArray();
                        writer.WriteNumber("RequiredNum", item.RequiredNum);

                        if (item.MinAcceptedItemUses > 0) writer.WriteNumber("MinAcceptedItemUses", item.MinAcceptedItemUses);
                        if (item.MinAcceptedItemMass > 0) writer.WriteNumber("MinAcceptedItemMass", item.MinAcceptedItemMass);
                        if (item.MinAcceptedItemHealth > 0) writer.WriteNumber("MinAcceptedItemHealth", item.MinAcceptedItemHealth);
                        if (!string.IsNullOrEmpty(item.MinAcceptedCookLevel)) writer.WriteString("MinAcceptedCookLevel", item.MinAcceptedCookLevel);
                        if (!string.IsNullOrEmpty(item.MaxAcceptedCookLevel)) writer.WriteString("MaxAcceptedCookLevel", item.MaxAcceptedCookLevel);
                        if (!string.IsNullOrEmpty(item.MinAcceptedCookQuality)) writer.WriteString("MinAcceptedCookQuality", item.MinAcceptedCookQuality);
                        if (item.MinAcceptedItemResourceRatio > 0) writer.WriteNumber("MinAcceptedItemResourceRatio", item.MinAcceptedItemResourceRatio);
                        if (item.MinAcceptedItemResourceAmount > 0) writer.WriteNumber("MinAcceptedItemResourceAmount", item.MinAcceptedItemResourceAmount);
                        if (item.RandomAdditionalRequiredNum > 0) writer.WriteNumber("RandomAdditionalRequiredNum", item.RandomAdditionalRequiredNum);

                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }
            }
            else if (value is InteractionCondition interaction)
            {
                if (interaction.Locations != null && interaction.Locations.Count > 0)
                {
                    writer.WriteStartArray("Locations");
                    foreach (var loc in interaction.Locations)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("AnchorMesh", loc.AnchorMesh);
                        writer.WriteString("FallbackTransform", loc.FallbackTransform);
                        writer.WriteString("VisibleMesh", loc.VisibleMesh);
                        if (loc.Instance.HasValue)
                        {
                            writer.WriteNumber("Instance", loc.Instance.Value);
                        }
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }

                writer.WriteNumber("MinNeeded", interaction.MinNeeded);
                writer.WriteNumber("MaxNeeded", interaction.MaxNeeded);
                writer.WriteBoolean("SpawnOnlyNeeded", interaction.SpawnOnlyNeeded);
                writer.WriteNumber("WorldMarkerShowDistance", interaction.WorldMarkerShowDistance);
            }

            if (value.LocationsShownOnMap != null && value.LocationsShownOnMap.Count > 0)
            {
                writer.WriteStartArray("LocationsShownOnMap");
                foreach (var loc in value.LocationsShownOnMap)
                {
                    writer.WriteStartObject();
                    writer.WriteStartObject("Location");
                    writer.WriteNumber("X", loc.Location.X);
                    writer.WriteNumber("Y", loc.Location.Y);
                    writer.WriteNumber("Z", loc.Location.Z);
                    writer.WriteEndObject();
                    writer.WriteNumber("SizeFactor", loc.SizeFactor);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}
