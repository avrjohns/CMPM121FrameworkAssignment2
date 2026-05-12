using Newtonsoft.Json.Linq;
using UnityEngine;

public class Piercing : ModifierSpell
{
    int pierceCount = 3;
    float damageReductionPerPierce = 0.75f;
    float manaMultiplier = 1.3f;

    public Piercing(Spell innerSpell, SpellCaster owner, JObject data) : base(innerSpell, owner)
    {
        if (data["pierce_count"] != null)
            int.TryParse(data["pierce_count"].ToString(), out pierceCount);
        if (data["damage_reduction_per_pierce"] != null)
            float.TryParse(data["damage_reduction_per_pierce"].ToString(), out damageReductionPerPierce);
        if (data["mana_multiplier"] != null)
            float.TryParse(data["mana_multiplier"].ToString(), out manaMultiplier);
    }

    public override string GetDescription() => innerSpell.GetDescription() + " [Piercing]";

    public override SpellProperties GetProperties()
    {
        var props = innerSpell.GetProperties();
        props.manaCostModifiers.Add(new ValueModifier(ValueModifier.Operation.MULTIPLY, manaMultiplier));
        props.isPiercing = true;
        props.pierceCount = pierceCount;
        props.damageReductionPerPierce = damageReductionPerPierce;
        return props;
    }
}