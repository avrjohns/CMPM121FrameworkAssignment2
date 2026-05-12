using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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

        var props = GetProperties();
        float finalDamage = props.GetDamage(damage);
        float finalSpeed = props.GetSpeed(speed);
        string trajectory = props.projectileTrajectory ?? GetProjectileTrajectory();
        int sprite = props.projectileSprite ?? GetProjectileSprite();
        float? lifetime = props.projectileLifetime ?? GetProjectileLifetime();

        // Store props for OnHit to access
        SpellProperties capturedProps = props;

        if (lifetime.HasValue)
        {
            GameManager.Instance.projectileManager.CreateProjectile(
                sprite, trajectory, where, target - where, finalSpeed,
                (hittable, pos) => OnHit(hittable, pos, finalDamage, capturedProps),
                lifetime.Value);
        }
        else
        {
            GameManager.Instance.projectileManager.CreateProjectile(
                sprite, trajectory, where, target - where, finalSpeed,
                (hittable, pos) => OnHit(hittable, pos, finalDamage, capturedProps));
        }

        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact, float damage, SpellProperties props)
    {
        if (other.team != team)
        {
            int dmgAmount = Mathf.RoundToInt(damage);
            other.Damage(new Damage(dmgAmount, damageType));

            // VAMPIRIC: Heal caster
            if (props.isVampiric && ownerHittable != null)
            {
                int heal = Mathf.RoundToInt(damage * props.lifeStealPercent);
                ownerHittable.hp += heal;
                ownerHittable.hp = Mathf.Min(ownerHittable.hp, ownerHittable.max_hp);
                Debug.Log($"[Vampiric] Healed {heal} HP");
            }

            // BIG GUY: Damage all enemies
            if (props.isBigGuy)
            {
                float splashDamage = damage * props.splashMultiplier;
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

            EnemyController ec = go.GetComponent<EnemyController>();
            if (ec != null && ec.hp != null && ec.hp.team != team && ec.hp != excludeTarget)
            {
                ec.hp.Damage(new Damage(Mathf.RoundToInt(damage), damageType));
            }
        }
    }
}