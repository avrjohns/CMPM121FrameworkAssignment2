using System.Collections.Generic;

public class SpellProperties
{
    public List<ValueModifier> damageModifiers = new List<ValueModifier>();
    public List<ValueModifier> manaCostModifiers = new List<ValueModifier>();
    public List<ValueModifier> cooldownModifiers = new List<ValueModifier>();
    public List<ValueModifier> speedModifiers = new List<ValueModifier>();
    public List<ValueModifier> lifetimeModifiers = new List<ValueModifier>();

    public string projectileTrajectory = null;
    public int? projectileSprite = null;
    public float? projectileLifetime = null;

    // VAMPIRIC
    public bool isVampiric = false;
    public float lifeStealPercent = 0f;

    //PIERCING
    public bool isPiercing = false;
    public int pierceCount = 0;
    public float damageReductionPerPierce = 0.75f;

    // BIG GUY
    public bool isBigGuy = false;
    public float splashMultiplier = 0.5f;

    public float GetDamage(float baseDamage) => ValueModifier.Apply(damageModifiers, baseDamage);
    public int GetManaCost(int baseCost) => ValueModifier.Apply(manaCostModifiers, baseCost);
    public float GetCooldown(float baseCooldown) => ValueModifier.Apply(cooldownModifiers, baseCooldown);
    public float GetSpeed(float baseSpeed) => ValueModifier.Apply(speedModifiers, baseSpeed);
    public float GetLifetime(float baseLifetime) => ValueModifier.Apply(lifetimeModifiers, baseLifetime);
}