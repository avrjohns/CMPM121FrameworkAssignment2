using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine;
public class ValueModifier
{
    public enum Operation { SET, MULTIPLY, ADD}
    public Operation op;
    public float value;

    public ValueModifier(Operation op, float value)
    {
        this.op = op;
        this.value = value;
    }
    public static float Apply(List<ValueModifier> modifiers, float baseValue)
    {
        if (modifiers == null || modifiers.Count == 0)
            return baseValue;

        float result = baseValue;

        // overrides base
        var setMods = modifiers.Where(m => m.op == Operation.SET);
        if (setMods.Any())
            result = setMods.Last().value;

        foreach (var mod in modifiers.Where(m => m.op == Operation.MULTIPLY))
            result *= mod.value;

        // apply all in seq
        foreach (var mod in modifiers.Where(m => m.op == Operation.ADD))
            result += mod.value;

        return result;
    }

    public static int Apply(List<ValueModifier> modifiers, int baseValue)
    {
        return Mathf.RoundToInt(Apply(modifiers, (float)baseValue));
    }

}
