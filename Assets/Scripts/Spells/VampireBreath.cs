using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

//Blood magic that consumes caster HP instead of mana
public class VampireBreath : Spell
{
    float hpCostBase = 10f;
    float hpCostPercent = 0.08f;
    float lifeStealPercent = 0.4f;
    float damagePerHPCost = 2.5f;

    public VampireBreath(SpellCaster owner, JObject data) : base(owner)
    {
        baseName = "Vampire Breath";
        baseDescription = "A torrent of crimson energy that feeds on your life force. Costs health instead of mana.";
        baseIcon = 99;
        baseDamage = 35;
        damageType = Damage.Type.DARK;
        baseManaCost = 0;
        baseCooldown = 2.5f;
        baseProjectileTrajectory = "straight";
        baseProjectileSpeed = 7f;
        baseProjectileSprite = 99;

        if (data != null)
        {
            if (data["hp_cost_base"] != null)
                float.TryParse(data["hp_cost_base"].ToString(), out hpCostBase);
            if (data["hp_cost_percent"] != null)
                float.TryParse(data["hp_cost_percent"].ToString(), out hpCostPercent);
            if (data["life_steal"] != null)
                float.TryParse(data["life_steal"].ToString(), out lifeStealPercent);
            if (data["damage_per_hp"] != null)
                float.TryParse(data["damage_per_hp"].ToString(), out damagePerHPCost);
        }
    }

    public override int GetManaCost() => 0;

    public string GetHPCostDescription()
    {
        if (ownerHittable == null) return "??? HP";
        int cost = CalculateHPCost();
        return $"{cost} HP";
    }

    public bool CanAfford()
    {
        if (ownerHittable == null)
        {
            Debug.LogError("[VampireBreath] ownerHittable is null! Cannot check HP cost.");
            return false;
        }
        return ownerHittable.hp > CalculateHPCost();
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team,
                                     float spellPower, int waveNumber)
    {
        this.team = team;
        last_cast = Time.time;

        if (ownerHittable == null)
        {
            Debug.LogError("[VampireBreath] Cannot cast: ownerHittable is null!");
            yield break;
        }

        int hpCost = CalculateHPCost();
        ownerHittable.hp -= hpCost;
        ownerHittable.hp = Mathf.Max(ownerHittable.hp, 1);

        float baseDmg = EvalRPN("35 power 3 / +", spellPower, waveNumber);
        float bonusDamage = hpCost * damagePerHPCost;
        float totalDamage = baseDmg + bonusDamage;

        var props = GetProperties();
        float finalDamage = props.GetDamage(totalDamage);
        float finalSpeed = props.GetSpeed(GetProjectileSpeed());
        string trajectory = props.projectileTrajectory ?? GetProjectileTrajectory();
        int sprite = props.projectileSprite ?? GetProjectileSprite();

        FlashCasterRed();

        GameManager.Instance.projectileManager.CreateProjectile(
            sprite, trajectory, where, target - where, finalSpeed,
            (hittable, pos) => OnHit(hittable, pos, finalDamage, hpCost));

        yield return new WaitForEndOfFrame();
    }

    int CalculateHPCost()
    {
        if (ownerHittable == null) return int.MaxValue;
        float cost = hpCostBase + (ownerHittable.max_hp * hpCostPercent);
        return Mathf.RoundToInt(cost);
    }

    void OnHit(Hittable other, Vector3 impact, float damage, int hpSpent)
    {
        if (other.team != team)
        {
            int dmgAmount = Mathf.RoundToInt(damage);
            other.Damage(new Damage(dmgAmount, damageType));

            int heal = Mathf.RoundToInt(damage * lifeStealPercent);
            ApplyHeal(heal);

            SpawnBloodEffect(impact);
        }
    }

    void ApplyHeal(int amount)
    {
        if (ownerHittable == null || amount <= 0) return;

        ownerHittable.hp += amount;
        ownerHittable.hp = Mathf.Min(ownerHittable.hp, ownerHittable.max_hp);

        Debug.Log($"[VampireBreath] Life steal healed {amount} HP. Current: {ownerHittable.hp}/{ownerHittable.max_hp}");
    }

    void FlashCasterRed()
    {
        if (ownerHittable != null && ownerHittable.owner != null)
        {
            var sr = ownerHittable.owner.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Debug.Log("[VampireBreath] Caster flashed crimson");
            }
        }
    }

    void SpawnBloodEffect(Vector3 pos)
    {
        Debug.Log($"[VampireBreath] blood effect at {pos}");
    }
}