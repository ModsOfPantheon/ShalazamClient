namespace ShalazamPlugin.SDK.Models;

public class ItemInfoPayloadMultiplierModifier
{
    public string MultiplierType { get; set; }

    public string? ModifierType { get; set; }

    // Creature-kind / race the bonus applies against (e.g. "vs Animal"); null when it applies universally.
    public string? BaneKind { get; set; }

    public string? BaneRace { get; set; }

    public float Amount { get; set; }
}
