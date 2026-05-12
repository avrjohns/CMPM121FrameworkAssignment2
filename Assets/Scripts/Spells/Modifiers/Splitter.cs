using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;

// casts spell twice in different directions.

public class Splitter : ModifierSpell
{
    float angle = 10f;
    float manaMultiplier = 1.5f;

    public Splitter(Spell inner, SpellCaster owner, JObject data) : base(inner, owner)
    {
        if (data["angle"] != null)
            float.TryParse(data["angle"].ToString(), out angle);
        if (data["mana_multiplier"] != null)
            float.TryParse(data["mana_multiplier"].ToString(), out manaMultiplier);
    }

    public override string GetDescription() => innerSpell.GetDescription() + " [Split]";

    public override SpellProperties GetProperties()
    {
        var props = innerSpell.GetProperties();
        props.manaCostModifiers.Add(new ValueModifier(ValueModifier.Operation.MULTIPLY, manaMultiplier));
        return props;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team,
                                     float spellPower, int waveNumber)
    {
        Vector3 dir = target - where;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 leftDir = new Vector3(
            Mathf.Cos((baseAngle - angle) * Mathf.Deg2Rad),
            Mathf.Sin((baseAngle - angle) * Mathf.Deg2Rad), 0);
        yield return innerSpell.Cast(where, where + leftDir, team, spellPower, waveNumber);

        Vector3 rightDir = new Vector3(
            Mathf.Cos((baseAngle + angle) * Mathf.Deg2Rad),
            Mathf.Sin((baseAngle + angle) * Mathf.Deg2Rad), 0);
        yield return innerSpell.Cast(where, where + rightDir, team, spellPower, waveNumber);
    }
}