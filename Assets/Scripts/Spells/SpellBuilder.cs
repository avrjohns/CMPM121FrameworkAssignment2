using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SpellBuilder
{
    JObject spellsData;
    System.Random rng;

    List<string> baseSpellKeys;
    List<string> modifierKeys;

    public SpellBuilder()
    {
        LoadSpells();
        rng = new System.Random();

        baseSpellKeys = new List<string> { "arcane_bolt", "vampire_breath" };

        modifierKeys = new List<string> {
            "damage_amp", "speed_amp", "doubler", "splitter", "chaos", "homing",
            "vampiric", "piercing", "big_guy"
        };
    }

    void LoadSpells()
    {
        TextAsset spellsText = Resources.Load<TextAsset>("spells");
        if (spellsText != null)
        {
            spellsData = JObject.Parse(spellsText.text);
            Debug.Log($"[SpellBuilder] Loaded {spellsData.Count} spell definitions");
        }
        else
        {
            Debug.LogError("[SpellBuilder] spells.json not found in Resources!");
            spellsData = new JObject();
        }
    }

    public Spell Build(SpellCaster owner)
    {
        return CreateBaseSpell("arcane_bolt", owner);
    }

    public Spell CreateBaseSpell(string key, SpellCaster owner)
    {
        if (spellsData == null || !spellsData.ContainsKey(key))
        {
            Debug.LogWarning($"[SpellBuilder] Spell '{key}' not found, falling back to arcane_bolt");
            key = "arcane_bolt";
        }

        JObject spellData = (JObject)spellsData[key];

        switch (key)
        {
            case "arcane_bolt": return new ArcaneBolt(owner, spellData);
            case "vampire_breath": return new VampireBreath(owner, spellData);
            default:
                Debug.LogWarning($"[SpellBuilder] Unknown base spell '{key}', using ArcaneBolt");
                return new ArcaneBolt(owner, spellData);
        }
    }

    public Spell BuildRandomSpell(SpellCaster owner, int waveNumber)
    {
        string baseKey = baseSpellKeys[rng.Next(baseSpellKeys.Count)];
        Spell spell = CreateBaseSpell(baseKey, owner);

        float probability = 0.7f;
        int maxModifiers = 5;
        int count = 0;

        while (rng.NextDouble() < probability && count < maxModifiers)
        {
            string modKey = modifierKeys[rng.Next(modifierKeys.Count)];
            spell = WrapModifier(modKey, spell, owner);
            probability *= 0.6f;
            count++;
        }

        return spell;
    }

    Spell WrapModifier(string key, Spell innerSpell, SpellCaster owner)
    {
        if (spellsData == null || !spellsData.ContainsKey(key))
        {
            Debug.LogError($"[SpellBuilder] Modifier '{key}' not found");
            return innerSpell;
        }

        JObject modData = (JObject)spellsData[key];

        switch (key)
        {
            case "damage_amp": return new DamageAmp(innerSpell, owner, modData);
            case "speed_amp": return new SpeedAmp(innerSpell, owner, modData);
            case "doubler": return new Doubler(innerSpell, owner, modData);
            case "splitter": return new Splitter(innerSpell, owner, modData);
            case "chaos": return new Chaos(innerSpell, owner, modData);
            case "homing": return new Homing(innerSpell, owner, modData);
            case "vampiric": return new Vampiric(innerSpell, owner, modData);
            case "piercing": return new Piercing(innerSpell, owner, modData);
            case "big_guy": return new BigGuy(innerSpell, owner, modData);
            default:
                Debug.LogWarning($"[SpellBuilder] Unknown modifier '{key}'");
                return innerSpell;
        }
    }

    public Spell GenerateRewardSpell(SpellCaster owner, int waveNumber)
    {
        return BuildRandomSpell(owner, waveNumber);
    }
}