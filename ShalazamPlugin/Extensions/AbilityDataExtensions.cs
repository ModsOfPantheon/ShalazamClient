using Il2Cpp;
using Il2CppInterop.Runtime;
using ShalazamPlugin.SDK.Models;
using MelonLoader;
using ShalazamPlugin.SDK.Models.Websockets;
using Object = Il2CppSystem.Object;

namespace ShalazamPlugin.Extensions;

public static class AbilityDataExtensions
{
    public static AbilityPayload ToRequestPayload(this AbilityData abilityData)
    {
        CheckConditions(abilityData);
        
        // TODO: This got out of hand, consolidate this wankery to a single function
        var castRange = GetCastRange(abilityData.Conditions);
        var meleeRange = GetMeleeRange(abilityData.Conditions);
        var levelRange = GetLevelRange(abilityData.Conditions);
        var requiresLineOfSight = GetLineOfSightRequirement(abilityData.Conditions);
        var targetEntityKindRestriction = GetTargetEntityKind(abilityData.Conditions);
        var targetMustBeAlive = GetTargetAliveCondition(abilityData.Conditions);
        var mustFaceTarget = GetMustFaceTargetCondition(abilityData.Conditions);
        var minimumPrimaryWeaponSkill = GetMinimumPrimaryWeaponSkill(abilityData.Conditions);
        var minimumSecondaryWeaponSkill = GetMinimumSecondaryWeaponSkill(abilityData.Conditions);
        var requiresTargetMissingStatus = GetMissingStatusRequirement(abilityData.Conditions);
        var requiresTargetMissingBuffs = GetMissingBuffsRequirement(abilityData.Conditions);
        var requiresCasterMissingStatuses = GetCasterMissingStatusRequirement(abilityData.Conditions);
        var requiresTargetIsRezableCorpse = GetTargetIsRezableCorpse(abilityData.Conditions);
        var requiresTargetInCombat = GetTargetInCombatCondition(abilityData.Conditions);
        var requiresTargetNotInCombat = GetTargetNotInCombatCondition(abilityData.Conditions);
        var requiresCasterNotInCombat = GetCasterNotInCombatCondition(abilityData.Conditions);
        var requiresCasterInCombat = GetCasterNotInCombatCondition(abilityData.Conditions);
        var casterBetweenPoolCondition = GetCasterBetweenPoolCondition(abilityData.Conditions);
        var maxLevelGapBetweenCasterAndTarget = GetMaxLevelBetweenCasterAndTarget(abilityData.Conditions);
        var requiresTargetIsAPet = GetTargetPetCondition(abilityData.Conditions);

        var masteryAbilityIds = new List<int>();
        foreach (var ability in abilityData.MasteryUpgrades)
        {
            masteryAbilityIds.Add(ability.AbilityId);
        }

        var description = "";
        try
        {
            if (!UIHelpers.TryParseAbilityDescription(abilityData, out description))
            {
            }
        }
        catch (Exception e)
        {
            // ignored
        }

        var data = new AbilityInfoPayload
        {
            // TODO: AbilityLines
            // TODO: AbilityGroups
            // TODO: AbilityLineIndex
            // TODO: AbilityLogicGraphs
            // TODO: BaseAbility
            // TODO: BaseCosts
            // TODO: CachedRangeData
            AbilitySchool = abilityData.AbilitySchool.ToString(),
            AbilityType = abilityData.AbilityType.ToString(),
            ActionType = abilityData.ActionType.ToString(),
            AffectedByGlobalCooldown = abilityData.AffectedByGlobalCooldown,
            AllowManyOfDifferentTiers = abilityData.AllowManyOfDifferentTiers,
            AllowManyOfSameTier = abilityData.AllowManyOfSameTier,
            CastableWhileHardCCd = abilityData.CastableWhileHardCCd,
            CastableWhileSilenced = abilityData.CastableWhileSilenced,
            CastTime = abilityData.CastTime,
            CastType = abilityData.CastType.ToString(),
            CastTypeBasedBuffId = abilityData.CastTypeBasedBuffId,
            ClassName = Globals.LocalPlayer.info.Class.ToString(),
            CharmUsable = abilityData.CharmUseable,
            ComboAbilities = GetComboAbilities(abilityData.ComboAbilities),
            Cooldown = abilityData.Cooldown,
            CooldownOverrideGroup = abilityData.CooldownOverrideGroup,
            Costs = GetCosts(abilityData.Costs),
            Description = string.IsNullOrWhiteSpace(description) ? abilityData.Description : description,
            DesignerId = abilityData.DesignerId,
            DisplayName = abilityData.DisplayName,
            FinishSoundType = abilityData.FinishSoundType.ToString(),
            GlobalCooldownMultiplier = abilityData.GlobalCooldownMultiplier,
            GroundTargetModel = abilityData.GroundTargetModel,
            GroundTargetRadius = abilityData.GroundTargetRadius,
            HasteAffectType = abilityData.HasteAffectType.ToString(),
            HideCastBarDuringCast = abilityData.HideCastBarDuringCast,
            HideForPets = abilityData.HideForPets,
            IconName = abilityData.Icon.IconName,
            Id = abilityData.Id,
            IgnoreCastTimeMultipliers = abilityData.IgnoreCastTimeMultipliers,
            ImpactSoundType = abilityData.ImpactSoundType.ToString(),
            IsCombo = abilityData.IsCombo,
            LeftHandEffect = abilityData.LeftHandEffect,
            LogicalGraphId = abilityData.LogicalGraphId,
            LoopSoundType = abilityData.LoopSoundType.ToString(),
            LoweredDesignerId = abilityData.loweredDesignerId,
            LoweredDisplayName = abilityData.loweredDisplayName,
            MeleeMaxRange = meleeRange,
            OverridePriority = abilityData.overridePriority,
            PassiveBuffAlwaysApplied = abilityData.PassiveBuffAlwaysApplied,
            PassiveBuffAppliedWhileOffCooldown = abilityData.PassiveBuffAppliedWhileOffCooldown,
            PrimaryWeaponBasedActionType = abilityData.PrimaryWeaponBasedActionType,
            RangedBasedActionType = abilityData.RangedBasedActionType,
            RequireMemorizeToCast = abilityData.RequireMemorizeToCast,
            RightHandEffect = abilityData.RightHandEffect,
            SecondaryWeaponBasedActionType = abilityData.SecondaryWeaponBasedActionType,
            SpellType = abilityData.SpellType.ToString(),
            TargetType = abilityData.TargetType.ToString(),
            TriggerGlobalCooldown = abilityData.TriggerGlobalCooldown,
            UseAllReadiness = abilityData.UseAllReadiness,
            Version = abilityData.Version,
            MinRange = castRange?.MinRange,
            MaxRange = castRange?.MaxRange,
            MinLevel = levelRange?.MinLevel,
            MaxLevel = levelRange?.MaxLevel,
            RequiresCasterMissingStatuses = requiresCasterMissingStatuses,
            RequiresEntityKind = targetEntityKindRestriction,
            RequiresFacingTarget = mustFaceTarget,
            RequiresLineOfSight = requiresLineOfSight,
            RequiresTargetIsRezablePlayerCorpse = requiresTargetIsRezableCorpse,
            RequiresTargetMissingStatuses = requiresTargetMissingStatus,
            RequiresTargetMissingBuffs = requiresTargetMissingBuffs,
            TargetMustBeAlive = targetMustBeAlive,
            MinimumPrimaryWeaponSkill = minimumPrimaryWeaponSkill,
            MinimumSecondaryWeaponSkill = minimumSecondaryWeaponSkill,
            RequiresTargetInCombat = requiresTargetInCombat,
            RequiresTargetNotInCombat = requiresTargetNotInCombat,
            RequiresCasterInCombat = requiresCasterInCombat,
            RequiresCasterNotInCombat = requiresCasterNotInCombat,
            MaxLevelGapBetweenCasterAndTarget = maxLevelGapBetweenCasterAndTarget,
            RequiresPoolWithinRange = casterBetweenPoolCondition,
            RequiresTargetIsAPet = requiresTargetIsAPet,
            MasteryAbilities = masteryAbilityIds.ToArray(),
            BaseAbilityId = abilityData.baseAbility?.Id
        };

        return new AbilityPayload
        {
            Id =  (uint)abilityData.Id,
            Type = "ability",
            Ability = new AbilityBody
            {
                Id = abilityData.Id,
                Data = data
            }
        };
    }

    private static bool GetTargetPetCondition(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            if (condition.TryCast<TargetIsPetCondition>() != null)
            {
                return true;
            }
        }
        
        return false;
    }

    private static int? GetMaxLevelBetweenCasterAndTarget(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var maxLevelCondition = condition.TryCast<MaxLevelGapBetweenCasterAndTargetCondition>();
            if (maxLevelCondition != null)
            {
                return maxLevelCondition.MaxLevelGap;
            }
        }
        
        return null;
    }

    private static AbilityPoolBetweenConditionData? GetCasterBetweenPoolCondition(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var casterCondition = condition.TryCast<CasterBetweenPercentPoolCondition>();
            if (casterCondition != null)
            {
                return new AbilityPoolBetweenConditionData
                {
                    MaxPercent = casterCondition.MaxPercent,
                    PoolType = casterCondition.PoolType.ToString(),
                    MinPercent = casterCondition.MinPercent,
                };
            }
        }

        return null;
    }

    private static bool GetCasterNotInCombatCondition(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var casterCondition = condition.TryCast<CasterIsNotInCombat>();
            if (casterCondition != null)
            {
                return true;
            }
        }
        
        return false;
    }

    private static bool GetTargetNotInCombatCondition(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var notInCombatCondition = condition.TryCast<TargetIsNotInCombat>();
            if (notInCombatCondition != null)
            {
                return true;
            }
        }
        
        return false;
    }

    private static bool GetTargetInCombatCondition(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var inCombatCondition = condition.TryCast<TargetIsInCombat>();
            if (inCombatCondition != null)
            {
                return true;
            }
        }

        return false;
    }

    private static float? GetMeleeRange(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var meleeCondition = condition.TryCast<CasterIsWithinMeleeDistanceToTargetCondition>();
            if (meleeCondition != null)
            {
                return meleeCondition.MaxRange;
            }
        }
        
        return null;
    }

    private static bool GetTargetIsRezableCorpse(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            if (condition.TryCast<TargetIsRezablePlayerCorpseCondition>() != null)
            {
                return true;
            }
        }
        
        return false;
    }

    private static string[] GetCasterMissingStatusRequirement(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var missingStatusCondition = condition.TryCast<CasterHasNoStatusCondition>();
            if (missingStatusCondition != null)
            {
                var list = new List<string>();
                foreach (var status in missingStatusCondition.StatusTypes)
                {
                    list.Add(status.ToString());
                }

                return list.ToArray();
            }
        }
        
        return Array.Empty<string>();
    }

    private static int[] GetMissingBuffsRequirement(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        // NB: Unlike other conditions, abilities can have multiple of this one for some reason
        var list = new List<int>();
        foreach (var condition in conditions)
        {
            var missingBuffCondition = condition.TryCast<TargetMissingBuffCondition>();
            if (missingBuffCondition != null)
            {
                list.Add(missingBuffCondition.Buff.BuffId);
            }
        }

        return list.ToArray();
    }

    private static string[] GetMissingStatusRequirement(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var missingStatusCondition = condition.TryCast<TargetHasNoStatusCondition>();

            if (missingStatusCondition == null)
            {
                continue;
            }
            
            var list = new List<string>();
            foreach (var status in missingStatusCondition.StatusTypes)
            {
                list.Add(status.ToString());
            }

            return list.ToArray();
        }
        
        return Array.Empty<string>();
    }

    private static int? GetMinimumPrimaryWeaponSkill(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var minimumPrimaryWeaponSkillCondition = condition.TryCast<CasterPrimaryWeaponSkillLevelIsAtLeast>();

            if (minimumPrimaryWeaponSkillCondition != null)
            {
                return minimumPrimaryWeaponSkillCondition.Min;
            }
        }

        return null;
    }
    
    private static int? GetMinimumSecondaryWeaponSkill(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var minimumPrimaryWeaponSkillCondition = condition.TryCast<CasterSecondaryWeaponSkillLevelIsAtLeast>();

            if (minimumPrimaryWeaponSkillCondition != null)
            {
                return minimumPrimaryWeaponSkillCondition.Min;
            }
        }

        return null;
    }

    private static bool GetMustFaceTargetCondition(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            if (condition.TryCast<CasterIsFacingTargetCondition>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private static AbilityCostData[] GetCosts(Il2CppSystem.Collections.Generic.List<AbilityCost> costs)
    {
        var list = new List<AbilityCostData>();

        foreach (var cost in costs)
        {
            list.Add(new AbilityCostData
            {
                Amount = cost.Amount,
                CostType = cost.CostType.ToString(),
                PoolType = cost.PoolType.ToString(),
                DontSubtractWhenCast = cost.DontSubtractWhenCast
            });
        }
        
        return list.ToArray();
    }

    private static int[] GetComboAbilities(Il2CppSystem.Collections.Generic.List<SerializableAbility> comboAbilities)
    {
        var list = new List<int>();

        foreach (var ability in comboAbilities)
        {
            list.Add(ability.AbilityId);
        }

        return list.ToArray();
    }

    private static bool GetTargetAliveCondition(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            if (condition.TryCast<TargetIsAliveCondition>() != null)
            {
                return true;
            }
        }
        
        return false;
    }

    private static string? GetTargetEntityKind(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var targetEntityKind = condition.TryCast<TargetIsEntityKindCondition>();
            if (targetEntityKind != null)
            {
                return targetEntityKind.eKind.ToString();
            }
        }
        
        return null;
    }

    private static bool GetLineOfSightRequirement(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var lineOfSightCondition = condition.TryCast<TargetWithinLineOfSight>();
            if (lineOfSightCondition != null)
            {
                return true;
            }
        }

        return false;
    }

    private static (int MinLevel, int MaxLevel)? GetLevelRange(Il2CppSystem.Collections.Generic.List<ICondition> conditions)
    {
        foreach (var condition in conditions)
        {
            var levelCondition = condition.TryCast<CasterIsWithinLevelRange>();
            if (levelCondition != null)
            {
                return ((int)levelCondition.Min, (int)levelCondition.Max);
            }
        }

        return null;
    }

    private static (float MinRange, float MaxRange)? GetCastRange(
        Il2CppSystem.Collections.Generic.List<ICondition> abilityDataConditions)
    {
        foreach (var blah in abilityDataConditions)
        {
            var distanceCondition = blah.TryCast<CasterIsWithinDistanceToTargetCondition>();
            if (distanceCondition != null)
            {
                return (distanceCondition.MinRange, distanceCondition.MaxRange);
            }
        }

        return null;
    }
    
    private static void CheckConditions(AbilityData abilityData)
    {
        foreach (var condition in abilityData.Conditions)
        {
            var obj = condition.TryCast<Object>();
            var il2cppType = obj.GetIl2CppType();
            
            if (!KnownTypes.Contains(il2cppType))
            {
                MelonLogger.Msg($"Ability {abilityData.DisplayName} contains unhandled type {il2cppType.Name}");
            }
        }
    }

    private static readonly Il2CppSystem.Type[] KnownTypes = {
        Il2CppType.Of<CasterIsWithinDistanceToTargetCondition>(),
        Il2CppType.Of<CasterIsWithinLevelRange>(),
        Il2CppType.Of<TargetWithinLineOfSight>(),
        Il2CppType.Of<TargetIsEntityKindCondition>(),
        Il2CppType.Of<TargetIsAliveCondition>(),
        Il2CppType.Of<CasterIsFacingTargetCondition>(),
        Il2CppType.Of<CasterPrimaryWeaponSkillLevelIsAtLeast>(),
        Il2CppType.Of<CasterSecondaryWeaponSkillLevelIsAtLeast>(),
        Il2CppType.Of<TargetHasNoStatusCondition>(),
        Il2CppType.Of<TargetMissingBuffCondition>(),
        Il2CppType.Of<CasterHasNoStatusCondition>(),
        Il2CppType.Of<TargetIsRezablePlayerCorpseCondition>(),
        Il2CppType.Of<CasterIsWithinMeleeDistanceToTargetCondition>(),
        Il2CppType.Of<TargetIsInCombat>(),
        Il2CppType.Of<TargetIsNotInCombat>(),
        Il2CppType.Of<CasterIsInCombat>(),
        Il2CppType.Of<CasterIsNotInCombat>(),
        Il2CppType.Of<MaxLevelGapBetweenCasterAndTargetCondition>(),
        Il2CppType.Of<CasterBetweenPercentPoolCondition>(),
        Il2CppType.Of<TargetIsPetCondition>()
    };
}