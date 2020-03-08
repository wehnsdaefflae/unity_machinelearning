using UnityEngine;
using Assets;
using UnityEngine.Assertions;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;
using System;

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

    private static bool ContainsNAN(double[] values) {
        foreach (double v in values) {
            if (double.IsNaN(v)) return true;
        }
        return false;
    }

    public override double[] GetParameters() {
        Vector<double> solution = this.varMatrix.Solve(this.covVector);
        double[] parameters = solution.AsArray();
        if (Regressor.ContainsNAN(parameters)) return new double[parameters.Length];
        return parameters;
    }

    public override double Output(double[] inValues) {
        double[] parameters = this.GetParameters();
        Assert.AreEqual(parameters.Length, this.addends.Length);
        Addend function;

        double output = 0d;
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

        int[] states;
        int[] indicesOccurences;
        int index = 1;
        NDimEnumerator nDimEnumerator;
        for (int i = 1; i < degree + 1; i++) {
            states = Enumerable.Repeat(i + 1, noArguments).ToArray();
            nDimEnumerator = new NDimEnumerator(states);

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
    // parameters
    private readonly float alpha; // somehow drag, no?
    private readonly float discount;
    private readonly float epsilon;
    private int drag;

    // approximators
    private Regressor actor;
    private Regressor critic;

    // memory
    private double[] sensorLast;
    private double actionLast;

    private float[] rangeAction;

    public QLearning(int dimensionSensor, float alpha, float discount, float epsilon) {
        this.alpha = alpha;
        this.discount = discount;
        this.epsilon = epsilon;
        this.drag = 100;

        this.actor = new RegressorPolynomial(dimensionSensor, 4);  // https://towardsdatascience.com/cartpole-introduction-to-reinforcement-learning-ed0eb5b58288
        this.critic = new RegressorPolynomial(dimensionSensor, 4);

        this.sensorLast = new double[dimensionSensor];
        this.actionLast = 0d;

        this.rangeAction = new float[] { -100f, 100f };
    }

    public double React(double[] sensor, double reward) {
        double action = this.Act(sensor);
        this.Fit(sensor, reward);

        this.sensorLast = sensor;
        this.actionLast = action;
        return action;
    }
    private void Fit(double[] sensor, double reward) {
        double qPrev = this.critic.Output(this.sensorLast);
        double qNow = this.critic.Output(sensor);
        double qUpdate = reward + this.discount * qNow;
        if (qPrev < qNow) {
            this.critic.Fit(this.sensorLast, qUpdate, this.drag);
            this.actor.Fit(this.sensorLast, this.actionLast, this.drag);
        }
    }

    private double Act(double[] sensor) {
        double action;
        if (UnityEngine.Random.Range(0f, 1f) >= this.epsilon) {
            action = UnityEngine.Random.Range(this.rangeAction[0], this.rangeAction[1]);
            return action;
        }
        action = Math.Min(Math.Max(this.actor.Output(sensor), this.rangeAction[0]), this.rangeAction[1]);
        return action;
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
