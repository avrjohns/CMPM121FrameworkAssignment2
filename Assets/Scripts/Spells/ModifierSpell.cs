using UnityEngine;
using System.Collections;
//public class ModifierSpell
public abstract class ModifierSpell : Spell
{
    protected Spell innerSpell;

    public ModifierSpell(Spell innerSpell, SpellCaster owner) : base(owner)
    {
        this.innerSpell = innerSpell;
        this.owner = owner;
    }
    public override string GetName() => innerSpell.GetName();
    public override string GetDescription() => innerSpell.GetDescription();
    public override int GetIcon() => innerSpell.GetIcon();
    public override float GetDamage() => innerSpell.GetDamage();
    public override int GetManaCost() => innerSpell.GetManaCost();
    public override float GetCooldown() => innerSpell.GetCooldown();
    public override string GetProjectileTrajectory() => innerSpell.GetProjectileTrajectory();
    public override float GetProjectileSpeed() => innerSpell.GetProjectileSpeed();
    public override int GetProjectileSprite() => innerSpell.GetProjectileSprite();
    public override float? GetProjectileLifetime() => innerSpell.GetProjectileLifetime();
    public override Damage.Type GetDamageType() => innerSpell.GetDamageType();

    // pass through to inner spell
    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team,
                                     float spellPower, int waveNumber)
    {
        return innerSpell.Cast(where, target, team, spellPower, waveNumber);
    }

    //aggregate inner properties
    public override SpellProperties GetProperties()
    {
        return innerSpell.GetProperties();
    }

    public override Hittable ownerHittable
    {
        get => innerSpell.ownerHittable;
        set => innerSpell.ownerHittable = value;
    }
}

