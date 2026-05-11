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

    public virtual string GetName() => baseName;
    public virtual string GetDescription() => baseDescription;
    public virtual int GetIcon() => baseIcon;
    public virtual float GetDamage() => baseDamage;
    public virtual int GetManaCost() => baseManaCost;
    public virtual float GetCooldown() => baseCooldown;
    public virtual string GetProjectileTrajectory() => baseProjectileTrajectory;
    public virtual float GetProjectileSpeed() => baseProjectileSpeed;
    public virtual int GetProjectileSprite() => baseProjectileSprite;
    public virtual float? GetProjectileLifetime() => baseProjectileLifetime;
    public virtual Damage.Type GetDamageType() => damageType;

    public bool IsReady() => (last_cast + GetCooldown() < Time.time);

    /// cast method spells implement projectile creation
    public abstract IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team,
                                     float spellPower, int waveNumber);

    /// ModifierSpells override to aggregate inner + their own.

    public virtual SpellProperties GetProperties()
    {
        return new SpellProperties();
    }

    /// load spell specific attributes from JSON.
    public virtual void SetAttributes(JObject attributes) { }

    /// evaluate RPN expressions with power and wave variables.
    protected float EvalRPN(string expression, float spellPower, int waveNumber)
    {
        if (string.IsNullOrEmpty(expression)) return 0;

        var vars = new Dictionary<string, float>
        {
            { "power", spellPower },
            { "wave", waveNumber }
        };
        return RPNEvaluatorWrapper.Evaluatef(expression, vars);
    }

    protected int EvalRPNInt(string expression, float spellPower, int waveNumber)
    {
        return Mathf.RoundToInt(EvalRPN(expression, spellPower, waveNumber));
    }

}
