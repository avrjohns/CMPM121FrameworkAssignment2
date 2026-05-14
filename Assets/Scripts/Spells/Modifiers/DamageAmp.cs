using Newtonsoft.Json.Linq;
using UnityEngine;

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

    public override string GetName() =>
        innerSpell.GetName() + " (damage-amplified)";

    public override string GetDescription() =>
        innerSpell.GetDescription() + " [Damage Amp]";

    public override SpellProperties GetProperties()
    {
        var props = innerSpell.GetProperties();

        // 🔥 DAMAGE scaling (THIS is what makes it actually affect gameplay)
        props.damageModifiers.Add(
            new ValueModifier(ValueModifier.Operation.MULTIPLY, damageMultiplier)
        );

        // 🔥 Mana scaling (cost increase)
        props.manaCostModifiers.Add(
            new ValueModifier(ValueModifier.Operation.MULTIPLY, manaMultiplier)
        );

        return props;
    }
}