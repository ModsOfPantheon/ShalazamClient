namespace ShalazamPlugin.SDK.Models.Websockets;

public class DropBody
{
    public string MonsterName { get; set; }
    public string Source { get; set; }
    public IEnumerable<ItemDrop> Items { get; set; }
}

public class ItemDrop
{
    public int Id { get; set; }
    public string Name { get; set; }
}