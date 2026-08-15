using System.Globalization;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppPantheonPersist;
using Il2CppSystem.Runtime.InteropServices;
using ShalazamPlugin.SDK.Models;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.Extensions;

public static class ItemExtensions
{
    // Most of the payload comes off item.Template (the shared definition); the stat modifiers are the one
    // piece that only exists on the live instance (item.statModifiers), so they're read separately. The
    // template's own StatModifiers array is always null on the client, which is why we can't work from a
    // bare ItemTemplate.
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
        // template.BuyPrice is a raw Nullable<int> il2cpp field that NREs on access (unlike CoinValue, a plain
        // int). Use the game's BuyPriceOrDefault accessor instead; treat 0 as "not sold" to keep the null-omit
        // semantics the server already expects.
        try
        {
            var bp = template.BuyPriceOrDefault;
            buyPrice = bp == 0 ? null : bp;
        }
        catch (Exception) { }
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
        try { damageType = template.DamageType?.Unbox<int>(); } catch (Exception) { }
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
            Instance = BuildInstance(item, template),
            ItemFlags = GetEnumFlags<ItemFlags>(template.ItemFlags),
            ItemWeight = template.ItemWeight,
            MaxDamage = template.MaxDamage,
            ModelId = template.ModelId,
            PrimaryBonus = primaryBonus?.ToString(),
            PrimarySkill = template.PrimarySkill.ToString(),
            Proficiency = GetProficiency(template)?.ToString(),
            Rarity = template.RarityId.ToString(),
            RecipeId = recipeId,
            RequiredLevel = template.RequiredLevel,
            SkillEffectiveness = skillEffectiveness,
            RequirementOverrides = template.RequirementOverrides?.Select(ToRequirementOverride),
            StatModifiers = BuildTemplateStatModifiers(template),
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

        return new ItemPayload
        {
            Item = new ItemBody
            {
                Id = itemData.ItemId,
                Data = itemData
            },
            Type = "item"
        };
    }

    // Identifies an item by its definition *and* its rolled stats, for deduplication.
    //
    // ItemId alone isn't enough any more: since uncommon+ items roll their own stats, two copies of the
    // same ItemId can carry completely different stats and both are worth uploading. Keyed on the rolled
    // values rather than the instance guid so that re-seeing the same physical item (or an identical roll
    // on a different one) stays deduplicated — otherwise every relog would re-upload the whole inventory.
    //
    // Modifiers are sorted because the game gives no ordering guarantee; without it the same roll in a
    // different array order would look like a new one.
    public static string GetDedupeSignature(this Item item)
    {
        var itemId = item.Template.ItemId;

        var stats = BuildInstanceStatModifiers(item)
            .Select(s => $"{s.Stat}:{s.ModifierType}:{s.Amount.ToString("R", CultureInfo.InvariantCulture)}")
            .OrderBy(s => s, StringComparer.Ordinal);

        var multipliers = BuildMultiplierModifiers(item.Template)
            .Select(m => $"{m.MultiplierType}:{m.ModifierType}:{m.BaneKind}:{m.BaneRace}:" +
                         m.Amount.ToString("R", CultureInfo.InvariantCulture))
            .OrderBy(m => m, StringComparer.Ordinal);

        return $"{itemId}|{string.Join(",", stats)}|{string.Join(",", multipliers)}";
    }

    // Everything that varies between two copies of the same ItemId. The stat rolls come off the live Item;
    // the multiplier modifiers come off the ItemTemplate, but that's instance data too — the client
    // deserializes a fresh ItemTemplate per Item rather than sharing one per ItemId, so nothing on it is
    // guaranteed to be common across copies.
    private static ItemInstancePayload BuildInstance(Item item, ItemTemplate template)
    {
        var statModifiers = BuildInstanceStatModifiers(item);
        var multiplierModifiers = BuildMultiplierModifiers(template);

        return new ItemInstancePayload
        {
            InstanceGuid = item.ItemInstanceGuid.ToString(),
            StatModifiers = statModifiers.Count == 0 ? null : statModifiers,
            MultiplierModifiers = multiplierModifiers.Count == 0 ? null : multiplierModifiers
        };
    }

    // Definition-level stats off the template, as opposed to the instance rolls. Always empty in practice
    // (see the note on ItemInfoPayload.StatModifiers), but mapped rather than assumed so that a change
    // server-side shows up as data instead of being silently dropped.
    private static List<ItemInfoPayloadStatModifier>? BuildTemplateStatModifiers(ItemTemplate template)
    {
        var templateStatModifiers = template.StatModifiers;
        if (templateStatModifiers == null || templateStatModifiers.Length == 0)
        {
            return null;
        }

        var result = new List<ItemInfoPayloadStatModifier>();
        foreach (var statModifier in templateStatModifiers)
        {
            if (statModifier == null)
            {
                continue;
            }

            result.Add(new ItemInfoPayloadStatModifier
            {
                Stat = statModifier.Stat.ToString(),
                ModifierType = statModifier.ModifierType.ToString(),
                Amount = statModifier.Amount
            });
        }

        return result.Count == 0 ? null : result;
    }

    // The equip proficiency shown in tooltips ("Requires Short Spears proficiency"). ItemTemplate.PrimarySkill
    // looks like the obvious source but is always None on the client, so this goes through the game's own
    // WeaponType/ArmorType -> SkillType mapping helpers instead. Shields carry a WeaponType (Buckler,
    // SmallShield, LargeShield, TowerShield) so they take the weapon path; the armor path has to be gated on
    // ItemTypeId because ArmorType has no None member (0 is HeavyPlate), so every non-armor item would
    // otherwise look like heavy plate.
    private static SkillType? GetProficiency(ItemTemplate template)
    {
        try
        {
            if (template.WeaponType != WeaponType.None)
            {
                var weaponProficiency = WeaponTypeExtensions.ToProficiencySkillType(template.WeaponType);

                return weaponProficiency == SkillType.None ? null : weaponProficiency;
            }

            if (template.ItemTypeId == ItemType.Armor)
            {
                var armorProficiency = ArmorTypeExtensions.ToProficiencySkillType(template.GetArmorType());

                return armorProficiency == SkillType.None ? null : armorProficiency;
            }
        }
        catch (Exception)
        {
            // Same defensive stance as the nullable template reads above: a proficiency we can't resolve is
            // better omitted than fatal to the whole item upload.
        }

        return null;
    }

    // Instance-rolled stat modifiers off a live Item. Item1 (StatType) can't be read directly due to an
    // il2cppinterop unboxing bug, so we marshal the raw struct and read the enum out of the last byte.
    private static List<ItemInfoPayloadStatModifier> BuildInstanceStatModifiers(Item item)
    {
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

        return statModifiersList;
    }

    // Conditional/"bane" multipliers off the template (e.g. "10% Physical Crit Damage vs Animal"). These are
    // separate from the instance stat modifiers above: the qualifier ("vs Animal"/"vs Wolf") lives here on
    // BaneKind/BaneRace and isn't exposed by any of the flat stat fields. BaneKind==Any / BaneRace==None are
    // the "applies to everything" sentinels, so they're normalised to null rather than emitted.
    private static List<ItemInfoPayloadMultiplierModifier> BuildMultiplierModifiers(ItemTemplate template)
    {
        var result = new List<ItemInfoPayloadMultiplierModifier>();

        var multiplierModifiers = template.MultiplierModifiers;
        if (multiplierModifiers == null)
        {
            return result;
        }

        foreach (var multiplierModifier in multiplierModifiers)
        {
            if (multiplierModifier == null)
            {
                continue;
            }

            var baneKind = multiplierModifier.BaneKind;
            var baneRace = multiplierModifier.BaneRace;

            result.Add(new ItemInfoPayloadMultiplierModifier
            {
                MultiplierType = multiplierModifier.MultiplierType.ToString(),
                ModifierType = multiplierModifier.Modifier?.ModifierType.ToString(),
                BaneKind = baneKind == EntityKind.Any ? null : baneKind.ToString(),
                BaneRace = baneRace == EntityRace.None ? null : baneRace.ToString(),
                Amount = multiplierModifier.Amount
            });
        }

        return result;
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