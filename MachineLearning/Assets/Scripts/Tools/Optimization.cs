using Assets;
using System;
using UnityEngine;


class GradientDescent {

    public static float[] Gradient(Func<float[], float> function, float[] arguments, float difference) {
        float[] g = new float[arguments.Length];
        float differenceHalf = difference / 2f;
        
        float argumentEach;
        float[] argumentsLo = new float[arguments.Length];
        float[] argumentsHi = new float[arguments.Length];
        for (int i = 0; i < arguments.Length; i++) {
            argumentEach = arguments[i];
            for (int j = 0; j < arguments.Length; j++) {
                argumentsLo[i] = (i == j) ? argumentEach - differenceHalf : argumentEach;
                argumentsHi[i] = (i == j) ? argumentEach + differenceHalf : argumentEach;
            }
            g[i] = (function(argumentsHi) - function(argumentsLo)) / difference;
        }

        return g;
    }

    public static float Polynomial(float[] parameters, int degree, float[] arguments) {
        int parameterCount = 0;
        float output = parameters[parameterCount++];
        NDimPermutator permutator;
        bool[] argumentSelection;
        float product;
        for (int i = 0; i < degree; i++) {
            permutator = new NDimPermutator(arguments.Length, i + 1);
            while (permutator.MoveNext()) {
                product = parameters[parameterCount++];
                argumentSelection = permutator.Current;
                for (int j = 0; j < argumentSelection.Length; j++) {
                    if (argumentSelection[j]) product *= arguments[j];
                }
                output += product;
            }
        }
        return output;
    }

}



public class Optimization : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
