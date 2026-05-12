using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class Doubler : ModifierSpell
{
    private float delay;
    private float manaMultiplier;
    private float cooldownMultiplier;

    public Doubler(Spell inner, SpellCaster owner, JObject data) : base(inner, owner)
    {
        delay = float.Parse(data["delay"].ToString());
        manaMultiplier = float.Parse(data["mana_multiplier"].ToString());
        cooldownMultiplier = float.Parse(data["cooldown_multiplier"].ToString());
    }

    public override string GetName() => innerSpell.GetName() + " (doubled)";
    public override int GetManaCost() => Mathf.RoundToInt(innerSpell.GetManaCost() * manaMultiplier);
    public override float GetCooldown() => innerSpell.GetCooldown() * cooldownMultiplier;

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, float spellPower, int waveNumber)
    {
        //cast once
        yield return innerSpell.Cast(where, target, team, spellPower, waveNumber);
        //cast again
        yield return new WaitForSeconds(delay);
        yield return innerSpell.Cast(where, target, team, spellPower, waveNumber);
    }
}