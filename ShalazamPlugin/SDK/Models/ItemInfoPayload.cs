using Il2Cpp;

namespace ShalazamPlugin.SDK.Models;

public class ItemInfoPayload
{
    public int ActivatedAbilityId { get; set; }
    public int ActivatedBuffId { get; set; }
    public List<string> AllowedClasses { get; set; }
    public List<string> AllowedLocations { get; set; }
    public List<string> AllowedRaces { get; set; }
    public float? ArmorModifier { get; set; }
    public string? ArmorType { get; set; }
    public float? BlockMod { get; set; }
    public float BlockValueMod { get; set; }
    public int? BuyPrice { get; set; }
    public IEnumerable<RecipeCraftingSlot>? CachedRecipeCraftingSlots { get; set; }
    public int? CachedRecipeSkillLevel { get; set; }
    public string? CachedRecipeSkillType { get; set; }
    public int ClassSetId { get; set; }
    public int CoinValue { get; set; }
    public int ContainerCapacity { get; set; }
    public string ContainerType { get; set; }
    public string? CraftingFamily { get; set; }
    public float? DamageModifier { get; set; }
    public string? DamageType { get; set; }
    public float Delay { get; set; }
    public float? DelayModifier { get; set; }
    public string DesignerNotes { get; set; }
    public int? Durability { get; set; }
    public float? DurabilityModifier { get; set; }
    public int? Duration { get; set; }
    public int? EffectId { get; set; }
    public float? EffectivenessMod { get; set; }
    public int EquipBuffId { get; set; }
    public string IconKey { get; set; }
    public int InWorldModelId { get; set; }
    public string ItemDescription { get; set; }
    public IEnumerable<string> ItemFlags { get; set; }
    public int ItemId { get; set; }
    public string ItemKey { get; set; }
    public int ItemLevel { get; set; }
    public int? ItemMaterialTypeId { get; set; }
    public string ItemName { get; set; }
    public string ItemType { get; set; }
    public float ItemWeight { get; set; }
    public int LearnedAbilityId { get; set; }
    public int LearnedOnAcquire { get; set; }
    public int LearnedWhileEquipped { get; set; }
    public int MaxDamage { get; set; }
    public int MaxStackSize { get; set; }
    public int MaxStackSizeOrCharges { get; set; }
    public int ModelId { get; set; }
    public bool? Polarity { get; set; }
    public float? Potency { get; set; }
    public string? PrimaryBonus { get; set; }
    public string PrimarySkill { get; set; }
    public string Rarity { get; set; }
    public int? RecipeId { get; set; }
    public int RequiredLevel { get; set; }
    public int? RequiredQuestId { get; set; }
    public IEnumerable<ItemRequirementOverride>? RequirementOverrides { get; set; }
    public string? SecondaryBonus { get; set; }
    public float? SkillEffectiveness { get; set; }
    public List<ItemInfoPayloadStatModifier>? StatModifiers { get; set; }
    public string? ToolType { get; set; }
    public string? UseAnimation { get; set; }
    public ItemUseRestrictions UseRestrictions { get; set; }
    public float? UseSeconds { get; set; }
    public string? WeaponType { get; set; }
}