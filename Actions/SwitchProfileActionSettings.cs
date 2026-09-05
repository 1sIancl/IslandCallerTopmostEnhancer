using System.Text.Json.Serialization;

namespace Classcaller.Actions;

/// <summary>
/// 切换档案行动的配置。
/// </summary>
public sealed class SwitchProfileActionSettings
{
    [JsonPropertyName("profileId")]
    public Guid ProfileId { get; set; }
}
