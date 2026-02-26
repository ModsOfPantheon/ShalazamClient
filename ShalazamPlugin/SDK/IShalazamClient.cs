using Il2Cpp;

namespace ShalazamPlugin.SDK;

public interface IShalazamClient
{
    public string? Username { get; }
    
    public void PostResourceLocation(NetworkWorldItem networkWorldItem);

    public void PostWorldItemLocation(NetworkWorldItem networkWorldItem);

    public void PostMonster(EntityNpcGameObject entityNpcGameObject);

    public void PostItem(Item item);
    public void PostAbility(AbilityData ability);
}