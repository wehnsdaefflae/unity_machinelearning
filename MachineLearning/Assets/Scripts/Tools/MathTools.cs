using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets {
    public class MathTools {
        public static int Modulo(int a, int b) {
            return ((a % b) + b) % b;
        }

        public static float Randomize(float value, float randomness) {
            return MathTools.Randomize(value, randomness, 0f, 1f);
        }

        public static float Randomize(float value, float randomness, float minValue, float maxValue) {
            return Mathf.Max(minValue, Mathf.Min(maxValue, value + Random.Range(-randomness, randomness)));
        }

        public static float AverageCartesian(IEnumerable<float> values) {
            int length = 0;
            float sum = 0f;
            foreach (float value in values) {
                sum += value;
                length += 1;
            }
            return sum / length;
        }

        public static float AverageGeometric(IEnumerable<float> values) {
            int length = 0;
            float product = 1f;
            foreach (float value in values) {
                product *= value;
                length += 1;
            }
            return Mathf.Pow(product, 1f / length);
        }

        public static float AverageSigmoid(IEnumerable<float> values) {
            float[] array = values.ToArray();
            float minValue = Mathf.Min(array);
            float maxValue = Mathf.Max(array);

            float a = AverageCartesian(values);

            return InterpolationSigmoid(minValue, maxValue, a);
        }

        public static float Sigmoid(float x) {
            return 1f / (1f + Mathf.Exp(-x));
        }

        public static float Normalize(float value, float valueMin, float valueMax) {
            Assert.IsTrue(value >= valueMin);
            Assert.IsTrue(valueMax >= value);
            Assert.IsTrue(valueMax >= valueMin);
            return (value - valueMin) / (valueMax - valueMin);
        }

        public static float InterpolationSigmoid(float a, float b, float t) {
            Assert.IsTrue(1f >= t && t >= 0f);
            float factor = Sigmoid(t * 20f - 10f);
            return a * (1f - factor) + b * factor;
        }

        public static double Smear(double average, double update, double inertia) {
            return (inertia * average + update) / (inertia + 1);
        }


        public static double Sum(double[] values) {
            double result = 0d;
            foreach (double v in values) {
                result += v;
            }
            return result;
        }

        public static float Sum(float[] values) {
            float result = 0f;
            foreach (float v in values) {
                result += v;
            }
            return result;
        }

        public static int Sum(int[] values) {
            int result = 0;
            foreach (int v in values) {
                result += v;
            }
            return result;
        }

        public static int Factorial(int n) {
            int f = 1;
            for (; n > 1; n--) {
                f *= n;
            }
            return f;
        }

        public static int Over(int choose, int from) {
            return Factorial(choose) / (Factorial(from) * Factorial(choose - from));
        }

        public static int Product(int[] values) {
            int result = 1;
            foreach (int v in values) {
                result *= v;
            }
            return result;
        }

        public static float Product(float[] values) {
            float result = 1f;
            foreach (float v in values) {
                result *= v;
            }
            return result;
        }

        public static double Product(double[] values) {
            double result = 1d;
            foreach (double v in values) {
                result *= v;
            }
            return result;
        }


    }
}
