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

    public override string GetName() => innerSpell.GetName() + " (damage-amplified)";
    public override float GetDamage() => innerSpell.GetDamage() * damageMultiplier;
    public override int GetManaCost() => Mathf.RoundToInt(innerSpell.GetManaCost() * manaMultiplier);
}