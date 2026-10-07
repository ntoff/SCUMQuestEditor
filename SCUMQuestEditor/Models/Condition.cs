#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SCUMQuestEditor.Models
{
    public class Condition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _trackingCaption = "";
        public string TrackingCaption
        {
            get => _trackingCaption;
            set
            {
                if (_trackingCaption != value)
                {
                    _trackingCaption = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _sequenceIndex = 0;
        public int SequenceIndex
        {
            get => _sequenceIndex;
            set
            {
                if (value < 0) value = 0;
                else if (value > 10) value = 10;

                if (_sequenceIndex != value)
                {
                    _sequenceIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canBeAutoCompleted;
        public bool CanBeAutoCompleted
        {
            get => _canBeAutoCompleted;
            set
            {
                if (_canBeAutoCompleted != value)
                {
                    _canBeAutoCompleted = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("Type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("LocationsShownOnMap")]
        public List<MapLocation> LocationsShownOnMap { get; set; } = new List<MapLocation>();
    }

    public class EliminationCondition : Condition
    {
        public EliminationCondition()
        {
            Type = "Elimination";
        }

        public int Amount { get; set; } = 1;
        public List<string>? TargetCharacters { get; set; } = new List<string>();
        public List<string>? AllowedWeapons { get; set; } = new List<string>();
    }

    public class FetchCondition : Condition
    {
        public FetchCondition()
        {
            Type = "Fetch";
        }

        [JsonPropertyName("DisablePurchaseOfRequiredItems")]
        public bool DisablePurchaseOfRequiredItems { get; set; } = true;

        [JsonPropertyName("PlayerKeepsItems")]
        public bool PlayerKeepsItems { get; set; } = false;

        [JsonPropertyName("RequiredItems")]
        public List<RequiredItem> RequiredItems { get; set; } = new List<RequiredItem>();
    }

    public class RequiredItem
    {
        [JsonPropertyName("AcceptedItems")]
        public List<string> AcceptedItems { get; set; } = new List<string>();

        public string ItemName => string.Join(", ", AcceptedItems);

        public string AcceptedItemsDisplay => string.Join(", ", AcceptedItems);

        [JsonPropertyName("RequiredNum")]
        public int RequiredNum { get; set; } = 0;

        public string Quantity => RequiredNum.ToString();

        private string? _propertiesCache;
        private int _propertiesVersion = 0;
        private int _lastAccessedVersion = -1;

        public string Properties
        {
            get
            {
                if (_lastAccessedVersion != _propertiesVersion)
                {
                    _propertiesCache = ComputeProperties();
                    _lastAccessedVersion = _propertiesVersion;
                }
                return _propertiesCache!;
            }
        }

        private string ComputeProperties()
        {
            var parts = new List<string>(8);
            if (MinAcceptedItemUses > 0) parts.Add($"Uses>={MinAcceptedItemUses}");
            if (MinAcceptedItemMass > 0) parts.Add($"Mass>={MinAcceptedItemMass}");
            if (MinAcceptedItemHealth > 0) parts.Add($"Health>={MinAcceptedItemHealth}");
            if (!string.IsNullOrEmpty(MinAcceptedCookLevel)) parts.Add($"MinCook>={MinAcceptedCookLevel}");
            if (!string.IsNullOrEmpty(MaxAcceptedCookLevel)) parts.Add($"MaxCook<={MaxAcceptedCookLevel}");
            if (!string.IsNullOrEmpty(MinAcceptedCookQuality)) parts.Add($"CookedQuality>={MinAcceptedCookQuality}");
            if (MinAcceptedItemResourceRatio > 0) parts.Add($"Resource%>={MinAcceptedItemResourceRatio}");
            if (MinAcceptedItemResourceAmount > 0) parts.Add($"Resource>={MinAcceptedItemResourceAmount}");
            if (RandomAdditionalRequiredNum > 0) parts.Add($"Random+={RandomAdditionalRequiredNum}");
            return string.Join("; ", parts);
        }

        public void MarkPropertiesDirty() => _propertiesVersion++;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int MinAcceptedItemUses { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int MinAcceptedItemMass { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int MinAcceptedItemHealth { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MinAcceptedCookLevel { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MaxAcceptedCookLevel { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MinAcceptedCookQuality { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int MinAcceptedItemResourceRatio { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int MinAcceptedItemResourceAmount { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int RandomAdditionalRequiredNum { get; set; }
    }

    public class InteractionCondition : Condition
    {
        public InteractionCondition()
        {
            Type = "Interaction";
        }

        [JsonPropertyName("SpawnOnlyNeeded")]
        public bool SpawnOnlyNeeded { get; set; } = true;

        [JsonPropertyName("MinNeeded")]
        public int MinNeeded { get; set; } = 1;

        [JsonPropertyName("MaxNeeded")]
        public int MaxNeeded { get; set; } = 1;

        [JsonPropertyName("WorldMarkerShowDistance")]
        public int WorldMarkerShowDistance { get; set; } = 5;

        [JsonPropertyName("Locations")]
        public List<InteractionLocation> Locations { get; set; } = new List<InteractionLocation>();
    }

    public class MapLocation
    {
        [JsonPropertyName("Location")]
        public MapLocationEntry Location { get; set; } = new MapLocationEntry();

        [JsonPropertyName("SizeFactor")]
        public double SizeFactor { get; set; } = 1.0;

        public string FormatString => $"X={Location.X} Y={Location.Y} Z={Location.Z}";
    }

    public class MapLocationEntry
    {
        [JsonPropertyName("X")]
        public double X { get; set; }

        [JsonPropertyName("Y")]
        public double Y { get; set; }

        [JsonPropertyName("Z")]
        public double Z { get; set; }
    }

    public class InteractionLocation
    {
        [JsonPropertyName("AnchorMesh")]
        public string AnchorMesh { get; set; } = "";

        [JsonPropertyName("FallbackTransform")]
        public string FallbackTransform { get; set; } = "";

        [JsonPropertyName("VisibleMesh")]
        public string VisibleMesh { get; set; } = "";

        [JsonPropertyName("Instance")]
        public int? Instance { get; set; } = null;

        [JsonIgnore]
        public string InstanceDisplay => Instance.HasValue ? Instance.Value.ToString() : "N/A";

        [JsonIgnore]
        public bool HasAnchor => !string.IsNullOrEmpty(AnchorMesh);
    }
}
