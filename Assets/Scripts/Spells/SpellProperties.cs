using UnityEngine;
using System.Collections.Generic;

/// container for all modifiable
/// 
/// /// for all spell props
/// base spells create empty one and ModifierSpells add to it.
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

    // behavior flags for modifiers
    public bool isPiercing = false;
    public int pierceCount = 0;
    public bool leavesTrail = false;
    public bool isBouncing = false;
    public int bounceCount = 0;

    // apply modifier chains
    public float GetDamage(float baseDamage) => ValueModifier.Apply(damageModifiers, baseDamage);
    public int GetManaCost(int baseCost) => ValueModifier.Apply(manaCostModifiers, baseCost);
    public float GetCooldown(float baseCooldown) => ValueModifier.Apply(cooldownModifiers, baseCooldown);
    public float GetSpeed(float baseSpeed) => ValueModifier.Apply(speedModifiers, baseSpeed);
    public float GetLifetime(float baseLifetime) => ValueModifier.Apply(lifetimeModifiers, baseLifetime);
}
