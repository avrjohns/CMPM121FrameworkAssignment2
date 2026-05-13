using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class ArcaneBolt : Spell
{
    // Store props at cast time so OnHit can access them
    private SpellProperties currentProps;

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

        var props = GetProperties();
        float finalDamage = props.GetDamage(damage);
        float finalSpeed = props.GetSpeed(speed);
        string trajectory = props.projectileTrajectory ?? GetProjectileTrajectory();
        int sprite = props.projectileSprite ?? GetProjectileSprite();
        float? lifetime = props.projectileLifetime ?? GetProjectileLifetime();

        // STORE props so OnHit can access them
        currentProps = props;

        if (lifetime.HasValue)
        {
            GameManager.Instance.projectileManager.CreateProjectile(
                sprite, trajectory, where, target - where, finalSpeed,
                OnHit,
                lifetime.Value);
        }
        else
        {
            GameManager.Instance.projectileManager.CreateProjectile(
                sprite, trajectory, where, target - where, finalSpeed,
                OnHit);
        }

        yield return new WaitForEndOfFrame();
    }

    // This matches the Action<Hittable, Vector3> signature that CreateProjectile expects
    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            // Get damage from currentProps or base
            float damage = currentProps != null ? currentProps.GetDamage(baseDamage) : baseDamage;
            int dmgAmount = Mathf.RoundToInt(damage);
            other.Damage(new Damage(dmgAmount, damageType));

            // VAMPIRIC: Heal caster
            if (currentProps != null && currentProps.isVampiric && ownerHittable != null)
            {
                int heal = Mathf.RoundToInt(damage * currentProps.lifeStealPercent);
                ownerHittable.hp += heal;
                ownerHittable.hp = Mathf.Min(ownerHittable.hp, ownerHittable.max_hp);
                Debug.Log($"[Vampiric] Healed {heal} HP. Current: {ownerHittable.hp}/{ownerHittable.max_hp}");
            }

            // BIG GUY: Damage all enemies
            if (currentProps != null && currentProps.isBigGuy)
            {
                float splashDamage = damage * currentProps.splashMultiplier;
                DamageAllEnemies(splashDamage, impact, other);
            }
        }
    }

    void DamageAllEnemies(float damage, Vector3 impact, Hittable excludeTarget)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("unit");
        foreach (GameObject go in enemies)
        {
            if (go == null) continue;

            EnemyController ec = go.GetComponent <EnemyController > ();
            if (ec != null && ec.hp != null && ec.hp.team != team && ec.hp != excludeTarget)
            {
                ec.hp.Damage(new Damage(Mathf.RoundToInt(damage), damageType));
            }
        }
    }
}