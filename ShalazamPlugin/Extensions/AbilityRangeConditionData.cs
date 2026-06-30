namespace ShalazamPlugin.Extensions;

public class AbilityRangeConditionData
{
    public float MinRange { get; set; }
    public float MaxRange { get; set; }

    // Only set for the long-term-position-storage variant; null otherwise.
    public string? Key { get; set; }
}
