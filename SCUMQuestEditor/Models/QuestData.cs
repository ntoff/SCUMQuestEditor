#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SCUMQuestEditor.Models
{
    public class SkillReward
    {
        public string Skill { get; set; } = "";
        public double Experience { get; set; } = 0;
    }

    public class TradeDealReward
    {
        public string Item { get; set; } = "";
        public double Price { get; set; } = 0;
        public int Amount { get; set; } = 1;
        public double Fame { get; set; } = 0;
        public bool AllowExcluded { get; set; } = false;
    }

    public class RewardPool
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int CurrencyNormal { get; set; } = 0;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int CurrencyGold { get; set; } = 0;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Fame { get; set; } = 0;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SkillReward>? Skills { get; set; } = null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<TradeDealReward>? TradeDeals { get; set; } = null;
    }

    public class TradeDeal
    {
        public string AssociatedNpc { get; set; } = "";
        public int Tier { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public double TimeLimitHours { get; set; }
        public List<RewardPool> RewardPool { get; set; } = new List<RewardPool>();
        public List<Condition> Conditions { get; set; } = new List<Condition>();
    }
}
