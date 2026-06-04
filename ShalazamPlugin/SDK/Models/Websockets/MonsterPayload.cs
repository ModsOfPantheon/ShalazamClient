namespace ShalazamPlugin.SDK.Models.Websockets;

public class MonsterPayload : WebsocketPayload
{
    public MonsterBody Monster { get; set; }
}

public class MonsterBody
{
    public string Name { get; set; }
    public string Difficulty { get; set; }
    public float LocX { get; set; }
    public float LocY { get; set; }
    public float LocZ { get; set; }
    public int Level { get; set; }
    public int Hour { get; set; }
    public string Zone { get; set; }
    public int MaxHp { get; set; }
}