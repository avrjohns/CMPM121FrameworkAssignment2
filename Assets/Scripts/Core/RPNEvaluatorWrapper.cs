using System.Collections.Generic;
using UnityEngine;
using RPNEvaluator;

public class RPNEvaluatorWrapper : MonoBehaviour
{
    public static RPNEvaluatorWrapper Instance;

    void Awake()
    {
        Instance = this;
    }

    public static int Evaluate(string expression, Dictionary<string, int> variables)
    {
        return RPNEvaluator.RPNEvaluator.Evaluate(expression, variables);
    }

    public static float Evaluatef(string expression, Dictionary<string, float> variables)
    {
        return RPNEvaluator.RPNEvaluator.Evaluatef(expression, variables);
    }

    public static float Evaluatef(string expression, Dictionary<string, int> variables)
    {
        return RPNEvaluator.RPNEvaluator.Evaluatef(expression, variables);
    }
}