using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class SpeedAmp : ModifierSpell
{
    private float speedMultiplier;

    public SpeedAmp(Spell inner, SpellCaster owner, JObject data) : base(inner, owner)
    {
        speedMultiplier = float.Parse(data["speed_multiplier"].ToString());
    }

    public override string GetName() => innerSpell.GetName() + " (speed-amplified)";
    public override float GetProjectileSpeed() => innerSpell.GetProjectileSpeed() * speedMultiplier;
}