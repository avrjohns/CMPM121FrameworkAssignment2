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
        return RPNEvaluatorWrapper.Evaluate(expression, variables);
    }

    public static float Evaluatef(string expression, Dictionary<string, float> variables)
    {
        return RPNEvaluatorWrapper.Evaluatef(expression, variables);
    }

    public static float Evaluatef(string expression, Dictionary<string, int> variables)
    {
        return RPNEvaluatorWrapper.Evaluatef(expression, variables);
    }
}