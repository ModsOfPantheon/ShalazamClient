namespace ShalazamPlugin.SDK.Models;

// The parts of an item that belong to one particular copy of it rather than to the item definition.
//
// Since the 2026-08 uncommon-item change, each instance of an uncommon+ item rolls its own stats at
// creation, so two items sharing an ItemId can have completely different stats. Those rolls live on the
// live Item (and on the per-instance ItemTemplate copy the client deserializes alongside it), never on
// anything shared, so they're reported separately from the definition rather than overwriting it.
public class ItemInstancePayload
{
    // Uniquely identifies this copy of the item, so repeat sightings of the same physical item can be
    // recognised rather than counted as a fresh roll.
    public string InstanceGuid { get; set; } = string.Empty;

    public List<ItemInfoPayloadStatModifier>? StatModifiers { get; set; }

    public List<ItemInfoPayloadMultiplierModifier>? MultiplierModifiers { get; set; }
}
