using UnityEngine;
using Assets;
using UnityEngine.Assertions;
using MathNet.Numerics.LinearAlgebra;

public delegate double Addend(double[] inValues);

public abstract class Approximator {
    abstract public double Output(double[] inValues);
    abstract public void Fit(double[] inValues, double targetValue, int drag);
    abstract public double[] GetParameters();
}

public class Regressor : Approximator {
    private readonly int noAddends;
    private readonly Addend[] addends;
    private readonly Matrix<double> varMatrix;
    private readonly Vector<double> covVector;

    public Regressor(Addend[] addends) {
        this.noAddends = addends.Length;
        this.addends = addends;
        this.covVector = Vector<double>.Build.Dense(this.noAddends);
        this.varMatrix = Matrix<double>.Build.Dense(this.noAddends, this.noAddends);

    }

    public override void Fit(double[] inValues, double targetValue, int drag) {
        Assert.IsTrue(drag >= 0);
        double[] components = new double[this.noAddends];
        Addend function;
        for (int i = 0; i < components.Length; i++) {
            function = this.addends[i];
            components[i] = function(inValues);
        }

        double componentA, componentB, value;
        for (int i = 0; i < components.Length; i++) {
            componentA = components[i];

            for (int j = 0; j < i + 1; j++) {
                componentB = components[j];
                value = MathTools.Smear(this.varMatrix[i, j], componentA * componentB, drag);
                this.varMatrix[i, j] = value;

                if (j < 1) {
                    continue;
                }
                this.varMatrix[j, i] = value;
            }

            this.covVector[i] = MathTools.Smear(this.covVector[i], componentA * targetValue, drag);
        }
    }

    public override double[] GetParameters() {
        Vector<double> parameters = this.varMatrix.Solve(this.covVector);
        return parameters.AsArray();
    }

    public override double Output(double[] inValues) {
        double[] parameters = this.GetParameters();
        Addend function;

        double output = 0f;
        for (int i = 0; i < this.noAddends; i++) {
            function = this.addends[i];
            output += parameters[i] * function(inValues);
        }

        return output;
    }
}

public class RegressorPolynomial : Regressor {
    public RegressorPolynomial(int noArguments, int degree) : base(RegressorPolynomial.AddendsPolynomial(noArguments, degree)) {
    }

    private static Addend CreateProduct(int noArguments, int[] indices) {
        Addend productSelect = delegate (double[] inValues) {
            int l = inValues.Length;
            Assert.AreEqual(l, noArguments);
            double[] factors = new double[indices.Length];
            foreach (int i in indices) {
                Assert.IsTrue(i < l);
                factors[i] = inValues[i];
            }
            return MathTools.Product(factors);
        };

        return productSelect;

    }

    private static Addend[] AddendsPolynomial(int noArguments, int degree) {
        int[] noCombinations = new int[degree];
        for (int i = 0; i < degree; i++) {
            noCombinations[i] = MathTools.Over(noArguments + i, i + 1);
        }
        int noAppends = 1 + MathTools.Sum(noCombinations);
        Addend[] addends = new Addend[noAppends];

        addends[0] = delegate (double[] inValues) {
            return 1d;
        };

        bool[] indicesBool;
        int[] indices;
        int index = 1, indexSub;
        for (int i = 0; i < degree; i++) {
            NDimPermutator nDimPermutator = new NDimPermutator(noArguments, i + 1);

            while (nDimPermutator.MoveNext()) {
                indicesBool = nDimPermutator.Current;
                indexSub = 0;
                indices = new int[i + 1];
                for (int j = 0; j < indicesBool.Length; j++) {
                    if (indicesBool[j]) {
                        indices[indexSub++] = j;
                    }
                }
                addends[index++] = RegressorPolynomial.CreateProduct(noArguments, indices);
            }
        }
        return addends;
    }

}


public class NewBehaviourScript : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
