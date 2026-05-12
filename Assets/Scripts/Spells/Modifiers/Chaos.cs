using Newtonsoft.Json.Linq;
using UnityEngine;

// Required Modifier: Significantly increased damage, but projectile spirals.

public class Chaos : ModifierSpell
{
    float damageMultiplier = 1.5f;

    public Chaos(Spell inner, SpellCaster owner, JObject data) : base(inner, owner)
    {
        if (data["damage_multiplier"] != null)
        {
            string expr = data["damage_multiplier"].ToString();
            if (!float.TryParse(expr, out damageMultiplier))
                damageMultiplier = 1.5f;
        }
    }

    public override string GetDescription() => innerSpell.GetDescription() + " [Chaotic]";

    public override string GetProjectileTrajectory() => "spiraling";

    public override SpellProperties GetProperties()
    {
        var props = innerSpell.GetProperties();
        props.damageModifiers.Add(new ValueModifier(ValueModifier.Operation.MULTIPLY, damageMultiplier));
        props.projectileTrajectory = "spiraling";
        return props;
    }
}