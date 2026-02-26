using Il2Cpp;

namespace ShalazamPlugin;

public static class AbilityCache
{
    private static readonly ICollection<int> SeenAbilityIds = new List<int>();

    
    public static void PostAbility(AbilityData ability)
    {
        if (SeenAbilityIds.Contains(ability.Id))
        {
            return;
        }

        SeenAbilityIds.Add(ability.Id);
        
        ModMain.ShalazamClient.PostAbility(ability);
    }

    public static void SuccessfullyPosted(int abilityId)
    {
        SeenAbilityIds.Add(abilityId);
    }
}