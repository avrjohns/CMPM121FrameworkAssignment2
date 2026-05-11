using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

/// <summary>
/// Base spell: straight-firing arcane projectile.
/// Reference implementation for all base spells.
/// </summary>
public class ArcaneBolt : Spell
{
    public ArcaneBolt(SpellCaster owner, JObject data) : base(owner)
    {
        baseName = data["name"]?.ToString() ?? "Arcane Bolt";
        baseDescription = data["description"]?.ToString() ?? "A straight-flying bolt.";
        baseIcon = data["icon"]?.Value<int>() ?? 0;

        var damageObj = data["damage"] as JObject;
        if (damageObj != null)
        {
            baseDamage = 25;
            damageType = Damage.TypeFromString(damageObj["type"]?.ToString() ?? "arcane");
        }

        baseManaCost = 10;
        baseCooldown = 2f;
        baseProjectileTrajectory = "straight";
        baseProjectileSpeed = 8f;
        baseProjectileSprite = 0;

        SetAttributes(data);
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team,
                                     float spellPower, int waveNumber)
    {
        this.team = team;
        last_cast = Time.time;
        float damage = EvalRPN("25 power 5 / +", spellPower, waveNumber);
        float speed = EvalRPN("8 power 5 / +", spellPower, waveNumber);

        // gget modified
        var props = GetProperties();
        float finalDamage = props.GetDamage(damage);
        float finalSpeed = props.GetSpeed(speed);
        string trajectory = props.projectileTrajectory ?? GetProjectileTrajectory();
        int sprite = props.projectileSprite ?? GetProjectileSprite();
        float? lifetime = props.projectileLifetime ?? GetProjectileLifetime();

        if (lifetime.HasValue)
        {
            GameManager.Instance.projectileManager.CreateProjectile(
                sprite, trajectory, where, target - where, finalSpeed,
                (hittable, pos) => OnHit(hittable, pos, finalDamage),
                lifetime.Value);
        }
        else
        {
            GameManager.Instance.projectileManager.CreateProjectile(
                sprite, trajectory, where, target - where, finalSpeed,
                (hittable, pos) => OnHit(hittable, pos, finalDamage));
        }

        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact, float damage)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(Mathf.RoundToInt(damage), damageType));
        }
    }
}