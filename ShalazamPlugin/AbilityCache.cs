using Il2Cpp;

namespace ShalazamPlugin;

public static class AbilityCache
{
    private static readonly ICollection<int> _seenAbilityIds = new List<int>();


    public static void PostAbility(AbilityData ability)
    {
        if (_seenAbilityIds.Contains(ability.Id))
        {
            return;
        }

        _seenAbilityIds.Add(ability.Id);

        ModMain.ShalazamClient.PostAbility(ability);
    }

    public static void SuccessfullyPosted(int abilityId)
    {
        _seenAbilityIds.Add(abilityId);
    }
}