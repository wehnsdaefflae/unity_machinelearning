using System.Linq;
using UnityEngine.Assertions;

namespace Assets {
    abstract class NoiseContainer {
        public readonly int[] shape;
        public readonly int size;
        public readonly int dimensionality;
        public readonly int[] wrappedDimensions;


        protected NoiseContainer(int size, int dimensionality, params int[] wrappedDimensions) {
            Assert.IsTrue(size != 0 && ((size & (size - 1)) == 0));     // check if size is power of 2
            this.shape = new int[dimensionality];
            this.size = size;
            for (int i = 0; i < dimensionality; i++) this.shape[i] = wrappedDimensions.Contains(i) ? size : size + 1;
            this.dimensionality = dimensionality;
            this.wrappedDimensions = wrappedDimensions;
        }

        private int[] Wrap(int[] coordinates) {
            Assert.AreEqual(coordinates.Length, this.dimensionality);

            int[] wrappedCoordinates = new int[this.dimensionality];
            for (int i = 0; i < this.dimensionality; i++) {
                wrappedCoordinates[i] = coordinates[i] % this.shape[i];
            }
            return wrappedCoordinates;
        }

        protected abstract void _Set(float value, params int[] coordinates);

        public void Set(float value, params int[] coordinates) {
            int[] wrapped = this.Wrap(coordinates);
            this._Set(value, wrapped);
        }

        protected abstract float _Get(params int[] coordinates);

        public float Get(params int[] coordinates) {
            int[] wrapped = this.Wrap(coordinates);
            return this._Get(wrapped);
        }

        public abstract NoiseContainer Copy();

        public abstract NDimArray GetSlice(int[] coordinatesSlice);

        public abstract void Bake();
    }

    class NoiseVolume : NoiseContainer {
        private readonly NDimArray array;

        public NoiseVolume(int size, int dimensionality, params int[] wrappedDimensions) : base(size, dimensionality, wrappedDimensions) {
            this.array = new NDimArray(this.shape);
        }

        public NoiseVolume(NDimArray initialArray, int size, int dimensionality, params int[] wrappedDimensions) : base(size, dimensionality, wrappedDimensions) {
            Assert.AreEqual(initialArray.shape, this.shape);
            this.array = initialArray;
        }

        public override void Bake() { }

        public override NoiseContainer Copy() {
            NoiseVolume noiseVolume = new NoiseVolume(this.array.Copy(), this.size, this.dimensionality, this.wrappedDimensions);
            return noiseVolume;
        }

        public override NDimArray GetSlice(int[] coordinatesSlice) {
            return this.array.Slice(coordinatesSlice);
        }

        protected override float _Get(params int[] coordinates) {
            return this.array.Get(coordinates);
        }

        protected override void _Set(float value, params int[] coordinates) {
            this.array.Set(value, coordinates);
        }
        
    }
}
