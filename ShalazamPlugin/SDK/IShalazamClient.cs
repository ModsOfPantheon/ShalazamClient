using Il2Cpp;
using ShalazamPlugin.SDK.Models;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.SDK;

public interface IShalazamClient
{
    public string? Username { get; }

    public void PostResourceLocation(NetworkWorldItem networkWorldItem);

    public void PostWorldItemLocation(NetworkWorldItem networkWorldItem);

    public void PostMonster(EntityNpcGameObject entityNpcGameObject);

    public void PostItem(Item item);
    public void PostItemTemplate(ItemTemplate template);
    public void PostAbility(AbilityData ability);
    public void PostDrops(EntityNpcGameObject entityNpcGameObject, bool isSkinning, IEnumerable<Item> itemsDropped);
    public void PostNpc(EntityNpcGameObject entityNpcGameObject);
    public void PostNpcVendorItems(uint networkId, string npcName, IEnumerable<NpcVendorItemEntry> items);
    public void PostMastery(MasteryPayload payload);
}