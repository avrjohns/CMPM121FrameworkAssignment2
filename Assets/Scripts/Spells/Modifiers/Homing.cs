using Newtonsoft.Json.Linq;
using UnityEngine;

public class Homing : ModifierSpell
{
    float damageMultiplier = 0.75f;
    int manaAdder = 10;

    public Homing(Spell inner, SpellCaster owner, JObject data) : base(inner, owner)
    {
        if (data["damage_multiplier"] != null)
            float.TryParse(data["damage_multiplier"].ToString(), out damageMultiplier);
        if (data["mana_adder"] != null)
            int.TryParse(data["mana_adder"].ToString(), out manaAdder);
    }

    public override string GetDescription() => innerSpell.GetDescription() + " [Homing]";

    public override string GetProjectileTrajectory() => "homing";

    public override SpellProperties GetProperties()
    {
        var props = innerSpell.GetProperties();
        props.damageModifiers.Add(new ValueModifier(ValueModifier.Operation.MULTIPLY, damageMultiplier));
        props.manaCostModifiers.Add(new ValueModifier(ValueModifier.Operation.ADD, manaAdder));
        props.projectileTrajectory = "homing";
        return props;
    }
}