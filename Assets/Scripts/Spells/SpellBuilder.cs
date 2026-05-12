using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

/// Builds spells from JSON

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

        // from spells.json
        //baseSpellKeys = new List<string> { "arcane_bolt", "magic_missile", "arcane_blast", "arcane_spray" };
        baseSpellKeys = new List<string> { "arcane_bolt","vampire_breath"};

        // from spells.json
        modifierKeys = new List<string> {
                "arcane_bolt", "magic_missile", "arcane_blast", "arcane_spray",
    "vampire_breath"
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
           // case "magic_missile": return new MagicMissile(owner, spellData);
          //  case "arcane_blast": return new ArcaneBlast(owner, spellData);
          //  case "arcane_spray": return new ArcaneSpray(owner, spellData);
            case "vampire_breath": return new VampireBreath(owner, spellData);
            //case "partner_spell": return new PartnerSpell(owner, spellData); 
            default:
                Debug.LogWarning($"[SpellBuilder] Unknown base spell '{key}', using ArcaneBolt");
                return new ArcaneBolt(owner, spellData);
        }
    }

    // 70 chance per modifier

    public Spell BuildRandomSpell(SpellCaster owner, int waveNumber)
    {
        // Pick random base spell
        string baseKey = baseSpellKeys[rng.Next(baseSpellKeys.Count)];
        Spell spell = CreateBaseSpell(baseKey, owner);

        //biased modifiers
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

        Debug.Log($"[SpellBuilder] Generated spell: {spell.GetName()} with {count} modifiers");
        return spell;
    }

    // Wrap with a modifier by key.

    Spell WrapModifier(string key, Spell inner, SpellCaster owner)
    {
        if (spellsData == null || !spellsData.ContainsKey(key))
        {
            Debug.LogError($"[SpellBuilder] Modifier '{key}' not found");
            return inner;
        }

        JObject modData = (JObject)spellsData[key];

        switch (key)
        {
            case "damage_amp": return new DamageAmp(inner, owner, modData);
            case "speed_amp": return new SpeedAmp(inner, owner, modData);
           // case "doubler": return new Doubler(inner, owner, modData);
           // case "splitter": return new Splitter(inner, owner, modData);
           // case "chaos": return new Chaos(inner, owner, modData);
            //case "homing": return new Homing(inner, owner, modData);
            default:
                Debug.LogWarning($"[SpellBuilder] Unknown modifier '{key}'");
                return inner;
        }
    }
    public Spell GenerateRewardSpell(SpellCaster owner, int waveNumber)
    {
        return BuildRandomSpell(owner, waveNumber);
    }
}