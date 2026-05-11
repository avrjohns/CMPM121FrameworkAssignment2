using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class SpeedAmp : Spell
{
    private Spell inner;
    private float speedMultiplier;

    public SpeedAmp(Spell inner, SpellCaster owner, JObject data) : base(owner)
    {
        this.inner = inner;
        speedMultiplier = float.Parse(data["speed_multiplier"].ToString());
    }

    public override string GetName() => inner.GetName() + " (speed-amplified)";
    public override float GetDamage() => inner.GetDamage();
    public override int GetManaCost() => inner.GetManaCost();
    public override float GetCooldown() => inner.GetCooldown();
    public override float GetProjectileSpeed() => inner.GetProjectileSpeed() * speedMultiplier;
    public override int GetProjectileSprite() => inner.GetProjectileSprite();
    public override string GetProjectileTrajectory() => inner.GetProjectileTrajectory();

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team,
                                     float spellPower, int waveNumber)
    {
        yield return inner.Cast(where, target, team, spellPower, waveNumber);
    }
}