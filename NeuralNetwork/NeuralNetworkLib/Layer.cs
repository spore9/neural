using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.Distributions;

namespace ANN
{
    [Serializable]
    class Layer
    {
        public SparseVector Activations;
        public SparseMatrix Weights;
        Random rnd = new Random();
        public Layer(int countInputs, int countOutput)
        {
            double coefficient = Math.Sqrt(6d) / Math.Sqrt(countInputs + countOutput);
            Weights =  (SparseMatrix)SparseMatrix.Build.SparseOfMatrix(Matrix<float>.Build.Random(countOutput, countInputs + 1, new ContinuousUniform(-coefficient, coefficient)));
            Activations = new SparseVector(countOutput+1);
            Activations[0]=1; //Bias
        }
        private double XavierInitialization(int countInputs, int countOutput)
        {
            return ((rnd.NextDouble() * 2 - 1) * (Math.Sqrt(6d) / Math.Sqrt(countInputs + countOutput)));
        }
    }
}
