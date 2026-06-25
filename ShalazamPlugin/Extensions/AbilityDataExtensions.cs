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
        var conditions = ParseConditions(abilityData.Conditions, abilityData.DisplayName);

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
            MeleeMaxRange = conditions.MeleeRange,
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
            MinRange = conditions.CastRange?.MinRange,
            MaxRange = conditions.CastRange?.MaxRange,
            MinLevel = conditions.LevelRange?.MinLevel,
            MaxLevel = conditions.LevelRange?.MaxLevel,
            RequiresCasterMissingStatuses = conditions.RequiresCasterMissingStatuses,
            RequiresEntityKind = conditions.TargetEntityKind,
            RequiresFacingTarget = conditions.MustFaceTarget,
            RequiresLineOfSight = conditions.RequiresLineOfSight,
            RequiresTargetIsRezablePlayerCorpse = conditions.RequiresTargetIsRezableCorpse,
            RequiresTargetMissingStatuses = conditions.RequiresTargetMissingStatuses,
            RequiresTargetMissingBuffs = conditions.RequiresTargetMissingBuffs,
            TargetMustBeAlive = conditions.TargetMustBeAlive,
            MinimumPrimaryWeaponSkill = conditions.MinimumPrimaryWeaponSkill,
            MinimumSecondaryWeaponSkill = conditions.MinimumSecondaryWeaponSkill,
            RequiresTargetInCombat = conditions.RequiresTargetInCombat,
            RequiresTargetNotInCombat = conditions.RequiresTargetNotInCombat,
            RequiresCasterInCombat = conditions.RequiresCasterInCombat,
            RequiresCasterNotInCombat = conditions.RequiresCasterNotInCombat,
            MaxLevelGapBetweenCasterAndTarget = conditions.MaxLevelGap,
            RequiresPoolWithinRange = conditions.CasterBetweenPool,
            RequiresTargetIsAPet = conditions.RequiresTargetIsAPet,
            RequiresCasterHasStatuses = conditions.RequiresCasterHasStatuses,
            RequiresCasterPrimaryWeaponTypes = conditions.RequiresCasterPrimaryWeaponTypes,
            RequiresCasterAtLeastPool = conditions.RequiresCasterAtLeastPool,
            RequiresCasterOrTargetLineOfSightToDefensiveTarget = conditions.RequiresCasterOrTargetLineOfSightToDefensiveTarget,
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

    private sealed class ConditionData
    {
        public (float MinRange, float MaxRange)? CastRange;
        public float? MeleeRange;
        public (int MinLevel, int MaxLevel)? LevelRange;
        public bool RequiresLineOfSight;
        public string? TargetEntityKind;
        public bool TargetMustBeAlive;
        public bool MustFaceTarget;
        public int? MinimumPrimaryWeaponSkill;
        public int? MinimumSecondaryWeaponSkill;
        public string[] RequiresTargetMissingStatuses = Array.Empty<string>();
        public int[] RequiresTargetMissingBuffs = Array.Empty<int>();
        public string[] RequiresCasterMissingStatuses = Array.Empty<string>();
        public bool RequiresTargetIsRezableCorpse;
        public bool RequiresTargetInCombat;
        public bool RequiresTargetNotInCombat;
        public bool RequiresCasterNotInCombat;
        public bool RequiresCasterInCombat;
        public AbilityPoolBetweenConditionData? CasterBetweenPool;
        public int? MaxLevelGap;
        public bool RequiresTargetIsAPet;
        public string[] RequiresCasterHasStatuses = Array.Empty<string>();
        public string[] RequiresCasterPrimaryWeaponTypes = Array.Empty<string>();
        public AbilityAtLeastPoolConditionData? RequiresCasterAtLeastPool;
        public bool RequiresCasterOrTargetLineOfSightToDefensiveTarget;
    }

    private static ConditionData ParseConditions(Il2CppSystem.Collections.Generic.List<ICondition> conditions, string abilityName)
    {
        var data = new ConditionData();
        var missingBuffs = new List<int>();

        foreach (var condition in conditions)
        {
            var obj = condition.TryCast<Object>();
            var il2cppType = obj.GetIl2CppType();

            if (!KnownTypes.Contains(il2cppType))
            {
                MelonLogger.Msg($"Ability {abilityName} contains unhandled type {il2cppType.Name}");
                continue;
            }

            if (condition.TryCast<CasterIsWithinDistanceToTargetCondition>() is { } castRange)
            {
                data.CastRange = (castRange.MinRange, castRange.MaxRange);
            }
            else if (condition.TryCast<CasterIsWithinMeleeDistanceToTargetCondition>() is { } melee)
            {
                data.MeleeRange = melee.MaxRange;
            }
            else if (condition.TryCast<CasterIsWithinLevelRange>() is { } levelRange)
            {
                data.LevelRange = ((int)levelRange.Min, (int)levelRange.Max);
            }
            else if (condition.TryCast<TargetWithinLineOfSight>() != null)
            {
                data.RequiresLineOfSight = true;
            }
            else if (condition.TryCast<TargetIsEntityKindCondition>() is { } entityKind)
            {
                data.TargetEntityKind = entityKind.eKind.ToString();
            }
            else if (condition.TryCast<TargetIsAliveCondition>() != null)
            {
                data.TargetMustBeAlive = true;
            }
            else if (condition.TryCast<CasterIsFacingTargetCondition>() != null)
            {
                data.MustFaceTarget = true;
            }
            else if (condition.TryCast<CasterPrimaryWeaponSkillLevelIsAtLeast>() is { } primarySkill)
            {
                data.MinimumPrimaryWeaponSkill = primarySkill.Min;
            }
            else if (condition.TryCast<CasterSecondaryWeaponSkillLevelIsAtLeast>() is { } secondarySkill)
            {
                data.MinimumSecondaryWeaponSkill = secondarySkill.Min;
            }
            else if (condition.TryCast<TargetHasNoStatusCondition>() is { } targetMissingStatus)
            {
                var list = new List<string>();
                foreach (var status in targetMissingStatus.StatusTypes) list.Add(status.ToString());
                data.RequiresTargetMissingStatuses = list.ToArray();
            }
            else if (condition.TryCast<TargetMissingBuffCondition>() is { } missingBuff)
            {
                missingBuffs.Add(missingBuff.Buff.BuffId);
            }
            else if (condition.TryCast<CasterHasNoStatusCondition>() is { } casterMissingStatus)
            {
                var list = new List<string>();
                foreach (var status in casterMissingStatus.StatusTypes) list.Add(status.ToString());
                data.RequiresCasterMissingStatuses = list.ToArray();
            }
            else if (condition.TryCast<TargetIsRezablePlayerCorpseCondition>() != null)
            {
                data.RequiresTargetIsRezableCorpse = true;
            }
            else if (condition.TryCast<TargetIsInCombat>() != null)
            {
                data.RequiresTargetInCombat = true;
            }
            else if (condition.TryCast<TargetIsNotInCombat>() != null)
            {
                data.RequiresTargetNotInCombat = true;
            }
            else if (condition.TryCast<CasterIsNotInCombat>() != null)
            {
                data.RequiresCasterNotInCombat = true;
            }
            else if (condition.TryCast<CasterIsInCombat>() != null)
            {
                data.RequiresCasterInCombat = true;
            }
            else if (condition.TryCast<CasterBetweenPercentPoolCondition>() is { } betweenPool)
            {
                data.CasterBetweenPool = new AbilityPoolBetweenConditionData
                {
                    MinPercent = betweenPool.MinPercent,
                    MaxPercent = betweenPool.MaxPercent,
                    PoolType = betweenPool.PoolType.ToString()
                };
            }
            else if (condition.TryCast<MaxLevelGapBetweenCasterAndTargetCondition>() is { } levelGap)
            {
                data.MaxLevelGap = levelGap.MaxLevelGap;
            }
            else if (condition.TryCast<TargetIsPetCondition>() != null)
            {
                data.RequiresTargetIsAPet = true;
            }
            else if (condition.TryCast<CasterHasAllStatusCondition>() is { } casterHasStatuses)
            {
                var list = new List<string>();
                foreach (var status in casterHasStatuses.StatusTypes) list.Add(status.ToString());
                data.RequiresCasterHasStatuses = list.ToArray();
            }
            else if (condition.TryCast<CasterHasPrimaryWeaponTypeList>() is { } weaponTypes)
            {
                var list = new List<string>();
                foreach (var wt in weaponTypes.WeaponTypes) list.Add(wt.ToString());
                data.RequiresCasterPrimaryWeaponTypes = list.ToArray();
            }
            else if (condition.TryCast<CasterHasAtLeastPercentPoolCondition>() is { } atLeastPool)
            {
                data.RequiresCasterAtLeastPool = new AbilityAtLeastPoolConditionData
                {
                    Percent = atLeastPool.Percent,
                    PoolType = atLeastPool.PoolType.ToString()
                };
            }
            else if (condition.TryCast<CasterOrTargetHasLineOfSightToCastersDefensiveTarget>() != null)
            {
                data.RequiresCasterOrTargetLineOfSightToDefensiveTarget = true;
            }
        }

        data.RequiresTargetMissingBuffs = missingBuffs.ToArray();
        return data;
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
        Il2CppType.Of<TargetIsPetCondition>(),
        Il2CppType.Of<CasterHasAllStatusCondition>(),
        Il2CppType.Of<CasterHasPrimaryWeaponTypeList>(),
        Il2CppType.Of<CasterHasAtLeastPercentPoolCondition>(),
        Il2CppType.Of<CasterOrTargetHasLineOfSightToCastersDefensiveTarget>()
    };
}
