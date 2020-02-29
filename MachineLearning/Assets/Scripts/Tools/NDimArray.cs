using UnityEngine;
using UnityEngine.Assertions;

namespace Assets {
    public class NDimArray {
        private readonly float[] array;
        public readonly int[] shape;

        public NDimArray(int[] shape, float[] initialArray) {
            this.shape = shape;
            int length = 1;
            foreach (int d in shape) length *= d;
            Assert.AreEqual(length, initialArray.Length);
            array = initialArray;
        }

        public NDimArray Copy() {
            return new NDimArray(this.shape, this.array);
        }

        public NDimArray(int[] shape) {
            this.shape = shape;
            int length = 1;
            foreach (int d in shape) length *= d;
            array = new float[length];
        }

        public float[] GetArray() {
            return this.array;
        }

        public override string ToString() {
            return this.array.ToString();
        }

        private int Linearize(int[] coordinates) {
            int noCoordinates = coordinates.Length;
            Assert.AreEqual(noCoordinates, this.shape.Length);
            int index = 0;
            int c, d;
            int j = 1;
            for (int i = 0; i < noCoordinates; i++) {
                c = coordinates[i];
                d = this.shape[i];
                index += Mathf.RoundToInt((c % d) * j);
                j *= d;
            }

            return index;
        }

        public void Set(float value, params int[] coordinates) {
            int index = this.Linearize(coordinates);
            this.array[index] = value;
        }

        public float Get(params int[] coordinates) {
            int index = this.Linearize(coordinates);
            return this.array[index];
        }

        public NDimArray Slice(int[] coordinatesSlice) {
            int[] offsets = new int[this.shape.Length - coordinatesSlice.Length];
            for (int i = 0; i < offsets.Length; i++) offsets[i] = 0;
            return this.Slice(coordinatesSlice, offsets);
        }

        public NDimArray Slice(int[] coordinatesSlice, params int[] offsets) {
            int dimTarget = this.shape.Length - coordinatesSlice.Length;
            Assert.AreEqual(dimTarget, offsets.Length);
            if (coordinatesSlice.Length < 1) return this.Copy();

            Assert.IsTrue(dimTarget >= 1);
            for (int i = 0; i < coordinatesSlice.Length; i++) Assert.IsTrue(this.shape[i] >= coordinatesSlice[i]);

            int[] subShape = new int[dimTarget];
            for (int i = 0; i < dimTarget; i++) subShape[i] = this.shape[i];
            NDimArray slice = new NDimArray(subShape);

            NDimEnumerator nDimEnumerator = new NDimEnumerator(subShape);
            float value;
            int[] coordinatesSource = new int[this.shape.Length];
            int[] coordinatesTarget;
            while (nDimEnumerator.MoveNext()) {
                coordinatesTarget = nDimEnumerator.Current;
                for (int i = 0; i < dimTarget; i++) coordinatesSource[i] = coordinatesTarget[i];
                for (int i = 0; i < coordinatesSlice.Length; i++) coordinatesSource[dimTarget + i] = coordinatesSlice[i];
                value = this.Get(coordinatesSource);
                for (int i = 0; i < dimTarget; i++) coordinatesTarget[i] = MathTools.Modulo(coordinatesTarget[i] + offsets[i], subShape[i]);
                slice.Set(value, coordinatesTarget);
            }

            return slice;
        }



        public static NDimArray operator +(NDimArray a, NDimArray b) {
            int noDimensions = a.shape.Length;
            Assert.AreEqual(noDimensions, b.shape.Length);

            int[] newShape = new int[noDimensions];
            for (int i = 0; i < noDimensions; i++) newShape[i] = a.shape[i] < b.shape[i] ? a.shape[i] : b.shape[i];

            NDimArray result = new NDimArray(newShape);

            NDimEnumerator nDimEnumerator = new NDimEnumerator(newShape);
            int[] coordinates;
            float value;
            while (nDimEnumerator.MoveNext()) {
                coordinates = nDimEnumerator.Current;
                value = a.Get(coordinates) + b.Get(coordinates);
                result.Set(value, coordinates);
            }

            return result;
        }

        public static NDimArray operator*(NDimArray a, float b) {
            for (int i = 0; i < a.array.Length; i++) a.array[i] *= b;           
            return a;
        }
    }
}
