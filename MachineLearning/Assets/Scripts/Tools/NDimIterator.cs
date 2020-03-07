using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace Assets {
    class NDimEnumerator : IEnumerator<int[]> {
        public readonly int dimensions;
        private readonly int[] current;
        private readonly int[] target;
        public readonly int noIterations;

        public NDimEnumerator(int[] noStatesPerIndex) {
            this.dimensions = noStatesPerIndex.Length;
            this.target = noStatesPerIndex;
            this.current = new int[this.dimensions];
            for (int i = 0; i < this.dimensions; i++) Assert.IsTrue(this.target[i] >= 1);
            this.noIterations = 1;
            foreach (int states in noStatesPerIndex) this.noIterations *= states;
            this.Reset();
        }

        public int[] Current {
            get {
                int[] returnArray = new int[this.dimensions];
                Array.Copy(current, 0, returnArray, 0, this.dimensions);
                return returnArray;
            }
        }

        object IEnumerator.Current {
            get {
                int[] returnArray = new int[this.dimensions];
                Array.Copy(current, 0, returnArray, 0, this.dimensions);
                return returnArray;
            }
        }

        public void Dispose() {
        }

        public void Reset() {
            for (int i = 0; i < this.dimensions - 1; i++) this.current[i] = 0;
            this.current[this.dimensions - 1] = -1;
            // TODO: not usable after reset
        }
        
        public bool MoveNext() {
            int c;
            for (int i = this.dimensions - 1; i >= 0; i--) {
                c = current[i];

                // below target
                if (c < this.target[i] - 1) {
                    this.current[i] = c + 1;
                    break;

                // at target 
                } else {
                    this.current[i] = 0;
                    if (i == 0) return false;
                }
            }
            return true;
        }
    }

    /**
    class NDimCombinatorRepetitions : NDimEnumerator {
        private readonly int noTokens;

        public NDimCombinatorRepetitions(int noTypes, int noTokens) : base(Enumerable.Repeat(noTokens + 1, noTypes).ToArray()) {
            this.noTokens = noTokens;
        }

        new public bool MoveNext() {
            bool r ;
            {
                r = base.MoveNext();
                if (!r) {
                    return false;
                }
            } while (base.Current.Sum() != this.noTokens);
            return true;
        }
    }
    **/


    class NDimCombinator : IEnumerator<bool[]> {
        private readonly bool[] current;
        private readonly int size;
        private readonly int selection;
        private bool initial;


        public NDimCombinator(int size, int selection) {
            Assert.IsTrue(size >= selection);
            this.size = size;
            this.selection = selection;
            this.current = new bool[this.size];
            this.initial = true;
        }

        public bool[] Current {
            get {
                bool[] returnArray = new bool[this.size];
                Array.Copy(current, 0, returnArray, 0, this.size);
                return returnArray;
            }
        }

        object IEnumerator.Current {
            get {
                bool[] returnArray = new bool[this.size];
                Array.Copy(current, 0, returnArray, 0, this.size);
                return returnArray;
            }
        }

        public void Dispose() {
        }

        public void Reset() {
            for (int i = 0; i < this.size - this.selection; i++) this.current[i] = false;
            for (int i = this.size - this.selection; i < this.size; i++) this.current[i] = true;
            this.initial = true;
            // TODO: usable after reset

        }

        public bool MoveNext() {
            if (this.initial) {
                this.Reset();
                this.initial = false;
                return true;
            }

            int noPrev = 0;

            for (int i = this.size - 1; i >= 0; i--) {
                if (this.current[i]) {
                    if (0 >= i) {
                        this.Reset();
                        return false;

                    } else if (!this.current[i - 1]) {
                        this.current[i] = false;
                        this.current[i - 1] = true;

                        int border = this.size - noPrev;
                        for (int j = i + 1; j < border; j++) this.current[j] = false;
                        for (int j = border; j < this.size; j++) this.current[j] = true;

                        noPrev = 0;
                        break;

                    }

                    noPrev += 1;
                }
            }
            
            return true;
        }
    }

}
