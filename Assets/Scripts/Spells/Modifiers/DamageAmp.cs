using Newtonsoft.Json.Linq;
using UnityEngine;

/// required modifier increases damage and mana cost multiplicatively.
/// JSON "damage_multiplier": "1.5", "mana_multiplier": "1.5"
public class DamageAmp : ModifierSpell
{
    float damageMultiplier = 1.5f;
    float manaMultiplier = 1.5f;

    public DamageAmp(Spell inner, SpellCaster owner, JObject data) : base(inner, owner)
    {
        if (data["damage_multiplier"] != null)
            float.TryParse(data["damage_multiplier"].ToString(), out damageMultiplier);
        if (data["mana_multiplier"] != null)
            float.TryParse(data["mana_multiplier"].ToString(), out manaMultiplier);
    }

    public override string GetDescription() => innerSpell.GetDescription() + " [Damage Amp]";

    public override SpellProperties GetProperties()
    {
        var props = innerSpell.GetProperties();
        props.damageModifiers.Add(new ValueModifier(ValueModifier.Operation.MULTIPLY, damageMultiplier));
        props.manaCostModifiers.Add(new ValueModifier(ValueModifier.Operation.MULTIPLY, manaMultiplier));
        return props;
    }
}