using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

//public class Spell 
public abstract class Spell
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;
    protected string baseName = "Unknown Spell";
    protected string baseDescription = "";
    protected int baseIcon = 0;
    protected float baseDamage = 0;
    protected Damage.Type damageType = Damage.Type.ARCANE;
    protected int baseManaCost = 0;
    protected float baseCooldown = 0;
    protected string baseProjectileTrajectory = "straight";
    protected float baseProjectileSpeed = 10f;
    protected int baseProjectileSprite = 0;
    protected float? baseProjectileLifetime = null;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
    }

    public string GetName()
    {
        return "Bolt";
    }

    public int GetManaCost()
    {
        return 10;
    }

    public int GetDamage()
    {
        return 100;
    }

    public float GetCooldown()
    {
        return 0.75f;
    }

    public virtual int GetIcon()
    {
        return 0;
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, 15f, OnHit);
        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }

    }

}
