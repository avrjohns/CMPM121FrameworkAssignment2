using System.Collections.Generic;
using UnityEngine;

using RPNLib = RPNEvaluator.RPNEvaluator;

public class RPNEvaluatorWrapper : MonoBehaviour
{
    public static RPNEvaluatorWrapper Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public static int Evaluate(string expression, Dictionary<string, int> variables)
    {
        return RPNLib.Evaluate(expression, variables);
    }

    public static float Evaluatef(string expression, Dictionary<string, float> variables)
    {
        return RPNLib.Evaluatef(expression, variables);
    }

    public static float Evaluatef(string expression, Dictionary<string, int> variables)
    {
        return RPNLib.Evaluatef(expression, variables);
    }
}