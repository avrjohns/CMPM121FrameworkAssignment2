using Newtonsoft.Json.Linq;
using UnityEngine;

public class Vampiric : ModifierSpell
{
    float lifeStealPercent = 0.25f;
    int manaAdder = 5;

    public Vampiric(Spell innerSpell, SpellCaster owner, JObject data) : base(innerSpell, owner)
    {
        if (data["life_steal_percent"] != null)
            float.TryParse(data["life_steal_percent"].ToString(), out lifeStealPercent);
        if (data["mana_adder"] != null)
            int.TryParse(data["mana_adder"].ToString(), out manaAdder);
    }

    public override string GetDescription() => innerSpell.GetDescription() + " [Vampiric]";

    public override SpellProperties GetProperties()
    {
        var props = innerSpell.GetProperties();
        props.manaCostModifiers.Add(new ValueModifier(ValueModifier.Operation.ADD, manaAdder));
        props.isVampiric = true;
        props.lifeStealPercent = lifeStealPercent;
        return props;
    }
}