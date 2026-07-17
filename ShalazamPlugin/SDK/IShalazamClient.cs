using Il2Cpp;
using ShalazamPlugin.SDK.Models;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.SDK;

public interface IShalazamClient
{
    public string? Username { get; }

    // Update status from the last "me" message: "none", "optional", or "required".
    public string? UpdateStatus { get; }

    public void PostResourceLocation(NetworkWorldItem networkWorldItem);

    public void PostWorldItemLocation(NetworkWorldItem networkWorldItem);

    public void PostMonster(EntityNpcGameObject entityNpcGameObject);

    public void PostItem(Item item);
    public void PostAbility(AbilityData ability);
    public void PostBuffs(IEnumerable<BuffData> buffs);
    public void PostDrops(EntityNpcGameObject entityNpcGameObject, bool isSkinning, IEnumerable<Item> itemsDropped);
    public void PostNpc(EntityNpcGameObject entityNpcGameObject);
    public void PostNpcVendorItems(uint networkId, string npcName, float locX, float locY, float locZ, string zone, IEnumerable<NpcVendorItemEntry> items);
    public void PostMastery(MasteryPayload payload);
}