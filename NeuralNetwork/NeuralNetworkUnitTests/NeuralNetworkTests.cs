using System;
using System.Collections.Generic;
using ANN;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.LinearAlgebra.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeuralNetworkUnitTests
{
    [TestClass]
    public class NeuralNetworkTests
    {
        [TestMethod]
        public void ExistanceTest()
        {
            NeuralNetwork network = new NeuralNetwork(2,1,4,2);
            Assert.IsNotNull(network);
        }
        [TestMethod]
        public void XorTest()
        {
            NeuralNetwork network = new NeuralNetwork(2, 1, 1, 1);
            float[,] xorInput = new float[4, 2]
            {
                { 0, 0},
                { 0, 1},
                { 1, 0},
                { 1, 1}
            };

            float[,] xorOutput = new float[4, 1]
            {
                {1},
                {0},
                {0},
                {1 }
            };
            SparseMatrix X = new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfArray(xorInput));
            SparseMatrix Y = new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfArray(xorOutput));
            network.LearnNetwork(X,Y,5000,0.001f,0);
            network.Inputs = DenseVector.OfArray(new float[2] { 0, 0 });
            network.ForwardPropagation();
            var test = network.GetAswer();
            Assert.IsNotNull(test);
            Assert.AreEqual(Math.Round(test[0]),1);
        }
    }
}
