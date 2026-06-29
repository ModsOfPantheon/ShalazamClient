namespace ShalazamPlugin.SDK.Models;

public class NpcInfoPayload
{
    public string Name { get; set; }
    public string? Title { get; set; }
    public string Kind { get; set; }
    public string Profession { get; set; }
    public string? Race { get; set; }
    public string? Class { get; set; }
    public string? Tier { get; set; }
    public int Level { get; set; }
    public int Hour { get; set; }
    public int MaxHp { get; set; }
    public float TrackRadius { get; set; }
    public string Zone { get; set; }
    public float LocX { get; set; }
    public float LocY { get; set; }
    public float LocZ { get; set; }
    public int? PrimaryFactionId { get; set; }
}
