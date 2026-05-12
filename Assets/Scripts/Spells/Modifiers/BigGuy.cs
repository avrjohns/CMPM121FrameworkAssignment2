using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;

// on hit damages ALL enemies for half the hit damage.

public class BigGuy : ModifierSpell
{
    float splashMultiplier = 0.5f;
    float manaMultiplier = 2.0f;

    public BigGuy(Spell innerSpell, SpellCaster owner, JObject data) : base(innerSpell, owner)
    {
        if (data["splash_multiplier"] != null)
            float.TryParse(data["splash_multiplier"].ToString(), out splashMultiplier);
        if (data["mana_multiplier"] != null)
            float.TryParse(data["mana_multiplier"].ToString(), out manaMultiplier);
    }

    public override string GetDescription() => innerSpell.GetDescription() + " [Big Guy]";

    public override SpellProperties GetProperties()
    {
        var props = innerSpell.GetProperties();
        props.manaCostModifiers.Add(new ValueModifier(ValueModifier.Operation.MULTIPLY, manaMultiplier));
        props.isBigGuy = true;
        props.splashMultiplier = splashMultiplier;
        return props;
    }
}