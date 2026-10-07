#nullable enable
using System.Text.Json.Serialization;

namespace SCUMQuestEditor.Models
{
    public class AppSettings
    {
        public string FileNameFormat { get; set; } = "T{tier}_{trader}_{title}";
        public string PrimaryColor { get; set; } = "BlueGrey";
        public string SecondaryColor { get; set; } = "Green";
        public string ErrorHighlightColor { get; set; } = "Red";
        public bool DarkMode { get; set; } = true;
    }
}
