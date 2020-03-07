using UnityEngine;
using Assets;
using UnityEngine.Assertions;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;

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

    private static Addend CreateProduct(int[] factors) {
        Addend productSelect = delegate (double[] inValues) {
            int l = inValues.Length;
            Assert.AreEqual(l, factors.Length);
            double product = 1d;
            double factor;
            for (int i = 0; i < l; i++) {
                factor = inValues[i];
                for (int j = 0; j < factors[i]; j++)
                    product *= factor;
            }
            return product;
        };

        return productSelect;

    }

    private static Addend[] AddendsPolynomial(int noArguments, int degree) {
        int noCombinations = 1;
        for (int i = 0; i < degree; i++) {
            noCombinations += MathTools.Over(noArguments + i, i + 1);
        }
        Addend[] addends = new Addend[noCombinations];

        addends[0] = delegate (double[] inValues) {
            return 1d;
        };

        int[] indicesOccurences;
        int index = 1;
        NDimEnumerator nDimEnumerator;
        for (int i = 1; degree >= i; i++) {
            nDimEnumerator = new NDimEnumerator(Enumerable.Repeat(i, noArguments).ToArray());

            while (nDimEnumerator.MoveNext()) {
                indicesOccurences = nDimEnumerator.Current;
                if (indicesOccurences.Sum() == i) {
                    addends[index++] = RegressorPolynomial.CreateProduct(indicesOccurences);
                }
            }
        }

        return addends;
    }

}

class QLearning {
    private readonly float alpha; // somehow drag, no?
    private readonly float discount;
    private readonly float epsilon;
    private Regressor regressorAction;
    private Regressor regressorQ;

    private double qLast;
    private double[] sensorLast;
    private double actionLast;
    private int drag;

    public QLearning(int dimensionSensor, float alpha, float discount, float epsilon) {
        this.alpha = alpha;
        this.discount = discount;
        this.epsilon = epsilon;
        this.regressorAction = new RegressorPolynomial(dimensionSensor, 4);  // https://towardsdatascience.com/cartpole-introduction-to-reinforcement-learning-ed0eb5b58288
        this.regressorQ = new RegressorPolynomial(dimensionSensor, 4);
        this.drag = 100;
    }

    public void fit(double reward) {
        double qThis = this.qLast * (1d - discount) + reward * discount;
        double qKnown = this.regressorQ.Output(this.sensorLast);
        if (qKnown < this.qLast) {
            this.regressorAction.Fit(this.sensorLast, this.actionLast, this.drag);
            this.regressorQ.Fit(this.sensorLast, qThis, this.drag);
        }

        this.qLast = qThis;
    }

    public double act(double[] sensor) {
        this.sensorLast = sensor;
        float noiseNormal = Random.Range(-this.epsilon, this.epsilon) * Random.Range(-this.epsilon, this.epsilon);
        this.actionLast = this.regressorAction.Output(sensor) + noiseNormal;
        return this.actionLast;
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
