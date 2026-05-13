using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster
{
    public int mana;
    public int max_mana;
    public int mana_reg;
    public Hittable.Team team;

    public List<Spell> spells = new List<Spell>();
    public int selectedSpellIndex = 0;

    public float spellPower = 10f;

    public Hittable ownerHittable;

    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            mana += mana_reg;
            mana = Mathf.Min(mana, max_mana);
            yield return new WaitForSeconds(1);
        }
    }

    public SpellCaster(int mana, int mana_reg, Hittable.Team team, Hittable ownerHittable = null)
    {
        this.mana = mana;
        this.max_mana = mana;
        this.mana_reg = mana_reg;
        this.team = team;
        this.ownerHittable = ownerHittable;

        Spell starter = new SpellBuilder().Build(this);
        starter.ownerHittable = ownerHittable;
        spells.Add(starter);
    }

    public Spell GetSelectedSpell()
    {
        if (selectedSpellIndex >= 0 && selectedSpellIndex < spells.Count)
            return spells[selectedSpellIndex];
        return null;
    }

    public IEnumerator Cast(Vector3 where, Vector3 target)
    {
        Spell spell = GetSelectedSpell();
        if (spell == null) yield break;

        if (spell is VampireBreath vb)
        {
            if (!vb.CanAfford())
            {
                Debug.Log("[SpellCaster] not enough hp for vampire");
                yield break;
            }
        }
        else
        {
            if (mana < spell.GetManaCost())
            {
                Debug.Log("[SpellCaster] not enough mana");
                yield break;
            }
            mana -= spell.GetManaCost();
        }

        if (!spell.IsReady())
        {
            Debug.Log("[SpellCaster] spell on cooldown now");
            yield break;
        }

        int wave = 1;
        if (EnemySpawner.Instance != null)
            wave = EnemySpawner.Instance.currentWave;

        yield return spell.Cast(where, target, team, spellPower, wave);
    }

    public void AddSpell(Spell spell)
    {
        if (spells.Count >= 4)
        {
            Debug.Log("[SpellCaster] cannot add at max 4 spells");
            return;
        }
        spell.ownerHittable = ownerHittable;
        spells.Add(spell);
    }

    public void RemoveSpell(int index)
    {
        if (index >= 0 && index < spells.Count)
        {
            spells.RemoveAt(index);
            if (selectedSpellIndex >= spells.Count)
                selectedSpellIndex = Mathf.Max(0, spells.Count - 1);
        }
    }

    public void ReplaceSpell(int index, Spell spell)
    {
        spell.ownerHittable = ownerHittable;
        if (index >= 0 && index < spells.Count)
            spells[index] = spell;
        else if (spells.Count < 4)
            spells.Add(spell);
    }

    public void SelectSpell(int index)
    {
        if (index >= 0 && index < spells.Count)
            selectedSpellIndex = index;
    }
}