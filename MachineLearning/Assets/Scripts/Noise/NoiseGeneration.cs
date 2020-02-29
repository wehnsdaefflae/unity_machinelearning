using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets {

    class NoiseGeneration {
        
        private static int[][][] GetBorders(int[] pointA, int[] pointB) {
            // === similar to GetCorners
            int dimension = pointA.Length;
            Assert.AreEqual(dimension, pointB.Length);

            int noDifferences = 0;
            bool[] indicesDifferent = new bool[dimension];
            for (int i = 0; i < dimension; i++) {
                if (pointA[i] != pointB[i]) {
                    indicesDifferent[i] = true;
                    noDifferences++;
                }
            }

            Assert.IsTrue(0 < noDifferences);
            // === end similar

            if (noDifferences == 1) return new int[][][] { new int[][] { pointA, pointB } };

            // initialize border structure (2*d-array of borders (2-array of points (d-array of integers)))
            int[][][] borders = new int[noDifferences * 2][][];
            int[][] eachBorder;
            for (int i = 0; i < borders.Length; i++) {
                eachBorder = new int[2][];
                borders[i] = eachBorder;
                for (int j = 0; j < eachBorder.Length; j++) eachBorder[j] = new int[dimension];
            }

            // declare loop variables
            int[] pointBorder;
            int[][] borderLo, borderHi;

            int index = 0;
            // for each dimension
            for (int i = 0; i < dimension; i++) {
                if (!indicesDifferent[i]) continue;

                // get low border
                borderLo = borders[index];

                // point a is original point a, point b is original point b EXCEPT along one dimension
                Array.Copy(pointA, borderLo[0], dimension);
                pointBorder = borderLo[1];
                for (int j = 0; j < dimension; j++) {
                    pointBorder[j] = i == j ? pointA[j] : pointB[j];
                }

                // get high border
                borderHi = borders[index + noDifferences];

                // point a is original point a EXCEPT along one dimension, point b is original point b
                Array.Copy(pointB, borderHi[1], dimension);
                pointBorder = borderHi[0];
                for (int j = 0; j < dimension; j++) {
                    pointBorder[j] = i == j ? pointB[j] : pointA[j];
                }

                index++;
            }

            return borders;
        }

        private static int[][] GetCorners(int[] pointA, int[] pointB) {
            // === similar to GetBorders
            int dimension = pointA.Length;
            Assert.AreEqual(dimension, pointB.Length);

            int[] indicesDifferent = new int[dimension];
            int noDifferences = 0;
            for (int i = 0; i < dimension; i++) {
                if (pointA[i] == pointB[i]) indicesDifferent[i] = 1;
                else {
                    indicesDifferent[i] = 2;
                    noDifferences++;
                }
            }

            Assert.IsTrue(0 < noDifferences);
            // == end similar

            if (noDifferences == 1) return new int[][] { pointA, pointB };

            NDimEnumerator nDimEnumerator = new NDimEnumerator(indicesDifferent);
            int[][] corners = new int[nDimEnumerator.noIterations][];
            int[] iterator, corner;
            int index = 0;
            while (nDimEnumerator.MoveNext()) {
                iterator = nDimEnumerator.Current;
                corner = new int[dimension];
                corners[index] = corner;
                for (int i = 0; i < dimension; i++) corner[i] = iterator[i] == 0 ? pointA[i] : pointB[i];
                index++;
            }

            return corners;

        }

        private static int[] GetMidpoint(int[][] points) {
            int dim = -1;
            foreach (int[] eachPoint in points) {
                if (dim < 0) dim = eachPoint.Length;
                else Assert.AreEqual(eachPoint.Length, dim);
            }

            int[] midpoint = new int[dim];
            foreach (int[] eachPoint in points) {
                for (int i = 0; i < dim; i++) midpoint[i] += eachPoint[i];
            }
            for (int i = 0; i < dim; i++) midpoint[i] /= points.Length;

            return midpoint;
        }

        private static void Scaffold(NoiseContainer container, float minValue, float maxValue) {
            int[] states = new int[container.dimensionality];
            for (int i = 0; i < states.Length; i++) states[i] = 2;
            NDimEnumerator nDimEnumerator = new NDimEnumerator(states);

            int[] coordinates;
            while (nDimEnumerator.MoveNext()) {
                coordinates = nDimEnumerator.Current;
                for (int i = 0; i < coordinates.Length; i++) coordinates[i] *= container.size;
                container.Set(UnityEngine.Random.Range(minValue, maxValue), coordinates);
            }

        }


        private static void Interpolate(NoiseContainer noiseContainer, int[] pointA, int sizeWindow, float randomness, float minValue, float maxValue) {
            // set border points
            int dim = pointA.Length;
            int[] pointB = new int[dim];
            for (int i = 0; i < dim; i++) pointB[i] = pointA[i] + sizeWindow;

            int[][][] borders = NoiseGeneration.GetBorders(pointA, pointB);
            int[][] corners = NoiseGeneration.GetCorners(pointA, pointB);

            Assert.IsTrue(corners.Length >= 2);

            float a = MathTools.AverageCartesian(from eachCorner in corners select noiseContainer.Get(eachCorner));
            int[] midpoint = NoiseGeneration.GetMidpoint(corners);
            noiseContainer.Set(MathTools.Randomize(a, randomness, minValue, maxValue), midpoint);

            Queue<int[][]> queueBorders = new Queue<int[][]>(borders);

            int[][] eachBorder;
            int[] pointBorderA, pointBorderB;
            while (queueBorders.Count >= 1) {
                eachBorder = queueBorders.Dequeue();
                pointBorderA = eachBorder[0];
                pointBorderB = eachBorder[1];

                corners = NoiseGeneration.GetCorners(pointBorderA, pointBorderB);

                Assert.IsTrue(corners.Length >= 2);

                a = MathTools.AverageCartesian(from eachCorner in corners select noiseContainer.Get(eachCorner));
                midpoint = NoiseGeneration.GetMidpoint(corners);
                noiseContainer.Set(MathTools.Randomize(a, randomness, minValue, maxValue), midpoint);

                borders = NoiseGeneration.GetBorders(pointBorderA, pointBorderB);
                if (1 < borders.Length) foreach (int[][] everyBorder in borders) queueBorders.Enqueue(everyBorder);
            }
        }

        public static void Generate(NoiseContainer noiseContainer, float randomness, int granularity, float minValue, float maxValue) {
            NoiseGeneration.Scaffold(noiseContainer, minValue, maxValue);

            int sizeCube = noiseContainer.size;
            // sizeCube must be power of 2

            int noTiles;
            NDimEnumerator generatorCoordinates;
            int[] coordinates;
            int[] tile_c = new int[noiseContainer.dimensionality];

            float randomEffective;

            while (sizeCube >= 2) {
                randomEffective = sizeCube >= granularity ? randomness : 0;

                noTiles = noiseContainer.size / sizeCube;
                for (int i = 0; i < tile_c.Length; i++) tile_c[i] = noTiles;
                generatorCoordinates = new NDimEnumerator(tile_c);

                while (generatorCoordinates.MoveNext()) {  // do this in parallel
                    coordinates = generatorCoordinates.Current;
                    for (int i = 0; i < coordinates.Length; i++) coordinates[i] *= sizeCube;
                    NoiseGeneration.Interpolate(noiseContainer, coordinates, sizeCube, randomEffective, minValue, maxValue);
                }
                sizeCube /= 2;
            }

            noiseContainer.Bake();
        }
    }
}
