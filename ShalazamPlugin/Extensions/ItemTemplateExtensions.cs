using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppPantheonPersist;
using Il2CppSystem.Runtime.InteropServices;
using ShalazamPlugin.SDK.Models;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.Extensions;

public static class ItemExtensions
{
    public static ItemPayload ToItemPayload(this Item item)
    {
        var template = item.Template;
        
        // This is so hacky...
        // Github issue tracking the reason for this: https://github.com/BepInEx/Il2CppInterop/issues/182
        StatType? secondaryBonus = null;
        int? durability = null;
        int? duration = null;
        EntityClassMask? allowedClassFlags = null;
        EquipSlotTypeFlag? allowedLocations = null;
        bool? polarity = null;
        float? potency = null;
        EntityRaceMask? allowedRaces = null;
        float? armorModifier = null;
        float? blockMod = null;
        int? buyPrice = null;
        CraftingFamilyType? craftingFamily = null;
        float? damageModifier = null;
        float? delayModifier = null;
        float? durabilityModifier = null;
        int? effectId = null;
        float? effectivenessMod = null;
        StatType? primaryBonus = null;
        int? recipeId = null;
        float? skillEffectiveness = null;
        int? requiredQuestId = null;
        EntityAction? useAnimation = null;
        float? useSeconds = null;
        int? cachedRecipeSkillLevel = null;
        SkillType? cachedRecipeSkillType = null;
        int? itemMaterialTypeId = null;
        int? damageType = null;
        
        // ReSharper disable EmptyGeneralCatchClause
        try { secondaryBonus = template.SecondaryBonus?.Unbox<StatType>(); } catch (Exception) { }
        try { durability = template.Durability?.Unbox<int>(); } catch (Exception) { }
        try { duration = template.Duration?.Unbox<int>(); } catch (Exception) { }
        try { allowedClassFlags = template.AllowedClasses?.Unbox<EntityClassMask>(); } catch (Exception) { }
        try { allowedLocations = template.AllowedLocations?.Unbox<EquipSlotTypeFlag>(); } catch (Exception) { }
        try { polarity = template.Polarity?.Unbox<bool>(); } catch (Exception) { }
        try { potency = template.Potency?.Unbox<float>(); } catch (Exception) { }
        try { allowedRaces = template.allowedRaces?.Unbox<EntityRaceMask>(); } catch (Exception) { }
        try { armorModifier = template.ArmorModifier?.Unbox<float>(); } catch (Exception) { }
        try { blockMod = template.BlockMod?.Unbox<float>(); } catch (Exception) { }
        try { buyPrice = template.BuyPrice?.Unbox<int>(); } catch (Exception) { }
        try { craftingFamily = template.CraftingFamily?.Unbox<CraftingFamilyType>(); } catch (Exception) { }
        try { damageModifier = template.DamageModifier?.Unbox<float>(); } catch (Exception) { }
        try { delayModifier = template.DelayModifier?.Unbox<float>(); } catch (Exception) { }
        try { durabilityModifier = template.DurabilityModifier?.Unbox<float>(); } catch (Exception) { }
        try { effectId = template.EffectId?.Unbox<int>(); } catch (Exception) { }
        try { effectivenessMod = template.EffectivenessMod?.Unbox<float>(); } catch (Exception) { }
        try { primaryBonus = template.PrimaryBonus?.Unbox<StatType>(); } catch (Exception) { }
        try { recipeId = template.RecipeId?.Unbox<int>(); } catch (Exception) { }
        try { skillEffectiveness = template.SkillEffectiveness?.Unbox<float>(); } catch (Exception) { }
        try { requiredQuestId = template.RequiredQuestId?.Unbox<int>(); } catch (Exception) { }
        try { useAnimation = template.UseAnimation?.Unbox<EntityAction>(); } catch (Exception) { }
        try { useSeconds = template.UseSeconds?.Unbox<float>(); } catch (Exception) { }
        try { cachedRecipeSkillLevel = template.CachedRecipeSkillLevel?.Unbox<int>(); } catch (Exception) { }
        try { cachedRecipeSkillType = template.CachedRecipeSkillType?.Unbox<SkillType>(); } catch (Exception) { }
        try { itemMaterialTypeId = template.ItemMaterialTypeId?.Unbox<int>(); } catch (Exception) { }
        try { damageType = template.DamageType?.Unbox<int>(); } catch (Exception ex) { }
        // ReSharper restore EmptyGeneralCatchClause

        var itemData = new ItemInfoPayload
        {
            ItemId = template.ItemId,
            ItemName = template.ItemName,
            ItemKey = template.ItemKey,
            DesignerNotes = template.DesignerNotes,
            ItemLevel = template.ItemLevel,
            ToolType = template.ToolType.ToString(),
            DamageType = damageType == null ? null : ((DamageType)damageType).ToString(),
            ItemDescription = template.ItemDescription,
            SecondaryBonus = secondaryBonus?.ToString(),
            Delay = template.delay,
            Durability = durability,
            Duration = duration,
            Polarity = polarity,
            Potency = potency,
            AllowedClasses = GetEnumFlags(allowedClassFlags),
            AllowedLocations = GetEnumFlags(allowedLocations),
            AllowedRaces = GetEnumFlags(allowedRaces),
            ArmorModifier = armorModifier,
            ArmorType = ((ArmorType)template.armorType).ToString(),
            BlockMod = blockMod,
            BuyPrice = buyPrice,
            CoinValue = template.CoinValue,
            ContainerCapacity = template.ContainerCapacity,
            ContainerType = template.ContainerType.ToString(),
            CraftingFamily = craftingFamily?.ToString(),
            DamageModifier = damageModifier,
            DelayModifier = delayModifier,
            DurabilityModifier = durabilityModifier,
            EffectId = effectId,
            EffectivenessMod = effectivenessMod,
            IconKey = template.IconKey,
            ItemFlags = GetEnumFlags<ItemFlags>(template.ItemFlags),
            ItemWeight = template.ItemWeight,
            MaxDamage = template.MaxDamage,
            ModelId = template.ModelId,
            PrimaryBonus = primaryBonus?.ToString(),
            PrimarySkill = template.PrimarySkill.ToString(),
            Rarity = template.RarityId.ToString(),
            RecipeId = recipeId,
            RequiredLevel = template.RequiredLevel,
            SkillEffectiveness = skillEffectiveness,
            RequirementOverrides = template.RequirementOverrides?.Select(ToRequirementOverride),
            StatModifiers = null,
            UseAnimation = useAnimation?.ToString(),
            UseSeconds = useSeconds,
            UseRestrictions = template.UseRestrictions,
            WeaponType = template.WeaponType.ToString(),
            ActivatedAbilityId = template.ActivatedAbilityId,
            ActivatedBuffId = template.ActivatedBuffId,
            BlockValueMod = template.BlockValueMod,
            ClassSetId = template.ClassSetId,
            EquipBuffId = template.EquipBuffId,
            ItemType = template.ItemTypeId.ToString(),
            LearnedAbilityId = template.LearnedAbilityId,
            MaxStackSize = template.MaxStackSize,
            RequiredQuestId = requiredQuestId,
            InWorldModelId = template.InWorldModelId,
            CachedRecipeSkillLevel = cachedRecipeSkillLevel,
            CachedRecipeSkillType = cachedRecipeSkillType?.ToString(),
            ItemMaterialTypeId = itemMaterialTypeId,
            MaxStackSizeOrCharges = template.MaxStackSizeOrCharges,
            CachedRecipeCraftingSlots = template.CachedRecipeCraftingSlots?.Select(ToCraftingSlot)
        };
        
        var statModifiersList = new List<ItemInfoPayloadStatModifier>();
        foreach (var statModifier in item.statModifiers.ToArray())
        {
            // Hacky fix for il2cppinterop bug
            var rawData = new Il2CppStructArray<byte>(17);
            Marshal.Copy(statModifier.Pointer, rawData, 0, rawData.Length);

            var statType = rawData.Last();
                        
            var mod = statModifier.Item2;
            var value = mod.Value;
            var modifierType = mod.ModifierType;

            statModifiersList.Add(new ItemInfoPayloadStatModifier
            {
                Stat = ((StatType)statType).ToString(),
                ModifierType = modifierType.ToString(),
                Amount = value
            });
        }
        
        itemData.StatModifiers = statModifiersList;

        return new ItemPayload
        {
            Id = (uint)itemData.ItemId,
            Item = new ItemBody
            {
                Id = itemData.ItemId,
                Data = itemData
            },
            Type = "item"
        };
    }

    private static ItemRequirementOverride ToRequirementOverride(SkillUnlock.ClassOverride classOverride)
    {
        return new ItemRequirementOverride
        {
            Class = classOverride.EntityClass.ToString(),
            Level = classOverride.OverrideLevel
        };
    }

    private static RecipeCraftingSlot ToCraftingSlot(CraftingSlot craftingSlot)
    {
        return new RecipeCraftingSlot
        {
            CraftingFamily = craftingSlot.CraftingFamily.ToString(),
            Amount = craftingSlot.Count,
            DisplayName = craftingSlot.DisplayName,
            Optional = craftingSlot.Optional
        };
    }

    private static List<string> GetEnumFlags<T>(T? mask) where T : struct, Enum
    {
        var result = new List<string>();

        // Check if mask is null
        if (!mask.HasValue)
        {
            return result;
        }

        foreach (T value in Enum.GetValues(typeof(T)))
        {
            if (mask.Value.HasFlag(value) && !value.Equals(default(T)))
            {
                result.Add(value.ToString());
            }
        }

        return result;
    }
}