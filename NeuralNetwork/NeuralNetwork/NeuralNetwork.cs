using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ANN
{
    [Serializable]
    class NeuralNetwork
    {
        float lambda = 1; //коэффициент регуляризации
        int m = 0; //количество обучающих примеров
        int butchSize = 0; //размер набора
        int epoch = 0;
        float a = 0.0001f; //скорость обучения
        private List<Layer> _layers; //слои
        public Vector<double> Inputs; //входные значения
        private bool _stop;
        public void ForwardPropagation()
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                if (i == 0)
                {
                    if (Inputs.Count != _layers[i].Weights.ColumnCount)
                    {
                        SparseVector tmp = SparseVector.Create(_layers[i].Weights.ColumnCount, 1);
                        for (int j = 1; j < tmp.Count; j++)
                            tmp[j] = Inputs[j - 1];
                        Inputs = tmp;
                    }
                    _layers[i].Activations = SparseVector.OfVector((_layers[i].Weights * Inputs));
                }
                else
                {
                    if (_layers[i].Weights.ColumnCount != _layers[i - 1].Activations.Count)
                    {
                        SparseVector tmp = SparseVector.Create(_layers[i].Weights.ColumnCount,1);
                        for (int j = 1; j < tmp.Count; j++)
                            tmp[j] = _layers[i - 1].Activations[j - 1];
                        _layers[i - 1].Activations = tmp;
                    }
                    else
                        _layers[i - 1].Activations[0] = 1;
                    _layers[i].Activations = SparseVector.OfVector((_layers[i].Weights * _layers[i - 1].Activations));
                }
                sigmoid(ref _layers[i].Activations);
            }
        } //Прохождение вперёд
        public int LearnNetwork(SparseMatrix X, SparseMatrix Y, int iter, float alpha,float lambd)
        {
            m = X.RowCount;
            a = alpha;
            lambda = lambd;
            float PreviousJ = 0;
            float J = 0;
            for (int i = 0; i < iter; i++)
            {
                List<SparseVector> h = new List<SparseVector>();
                List<List<SparseVector>> activations = new List<List<SparseVector>>();
                SparseMatrix newX = X;
                OnNewIteration(i, J);
                /*if (i%epoch==0)
                {
                    int butchNumbers = (int)Math.Ceiling((double)(m / butchSize));
                    for (int j=0;j< butchNumbers-1; j++)
                    {
                        for (int k = 0; k < butchSize; k++)
                        {
                            if (j==0)
                            {
                                newX.SetRow(k, X.Row(k + (butchNumbers-1) * butchSize));
                            }
                            else
                                newX.SetRow(k,X.Row(k+(j-1)* butchSize));
                        }
                    }
                }*/
                if (_stop)
                {
                    _stop = false;
                    return 2;
                }
                for (int j = 0; j < m; j++) //рассчитываем гипотезы
                {
                    Inputs = newX.Row(j);
                    ForwardPropagation();
                    h.Add(_layers.Last().Activations);
                    activations.Add(new List<SparseVector>());
                    for (int k=0;k<_layers.Count;k++)
                    {
                        activations[j].Add(_layers[k].Activations);
                    }
                }
                PreviousJ = J;
                J = getCostFunction(h, Y);
                if (i != 0)
                    if (J > PreviousJ)
                    {
                        return 1;
                    }
                backPropagation(Y, newX, activations);
            }
            return 0;
        } //обучение
        public double getRegressionError(DenseVector Y, double error)
        {
            double averageError = Y.Subtract(Y.Average()).PointwisePower(2).Sum();
            return 1 - (error / averageError);
        }
        public double getClassificationError(int TP, int FP, int FN)
        {
            double precision = (double)TP / (TP + FP);
            double recall = (double)TP / (TP + FN);
            if (double.IsNaN(precision) || double.IsNaN(recall))
                return 1;
            return (2 * precision * recall) / (precision + recall);
        }
        public NeuralNetwork(int numberOfInputs, int numberOfLayers, int numberOfUnits, int numberOfOutputs)
        {
            _layers = new List<Layer>();
            Inputs = new SparseVector(numberOfInputs + 1);
            _stop = false;
            butchSize = 500;
            epoch = 100;
            Inputs[0] = 1;
            for (int i = 0; i < numberOfLayers+1; i++)
            {
                if (i == 0)
                {
                        _layers.Add(new Layer(numberOfInputs, numberOfUnits));
                }
                else
                {
                    if (i == numberOfLayers)
                    {
                        _layers.Add(new Layer(numberOfUnits, numberOfOutputs));
                    }
                    else
                        _layers.Add(new Layer(numberOfUnits, numberOfUnits));
                }
            }
        } //конструктор
        public List<double> GetAswer()
        {
            return _layers.Last().Activations.ToList();
        } //Получить результат
        public int GetOutputs()
        {
            return _layers.Last().Weights.RowCount;
        } //Получить число выходов
        public int GetInputs()
        {
            return _layers[0].Weights.ColumnCount;
        } //Получить число входов
        public int GetLayersCount()
        {
            return _layers.Count;
        }
        public int GetNeuronsCount()
        {
                return _layers[0].Weights.ColumnCount;
        }
        public SparseMatrix GetWeight(int layer)
        {
            return _layers[layer].Weights;
        }
        private void backPropagation(SparseMatrix Y, SparseMatrix X, List<List<SparseVector>> activations)
        {
            List<SparseMatrix> derivatives = getDerivatives(Y, X,activations);
            gradientDescent(derivatives);
        } //Обратное распространение ошибки
        private float getRegularisation()
        {
            float regularization = (float)(lambda / (2 * m));
            if (regularization != 0)
            {
                for (int L = 0; L < _layers.Count; L++)
                {
                    for (int i = 0; i < _layers[L].Weights.RowCount; i++)
                    {
                        for (int j = 0; j < _layers[L].Weights.ColumnCount; j++)
                        {
                            if (j != 0)
                                regularization += (float)Math.Pow(_layers[L].Weights.Row(i)[j], 2);
                        }
                    }

                }
            }
            return regularization;
        } //Получение коэф регуляризации для J
        private float getCostFunction(List<SparseVector> h, SparseMatrix Y)
        {
            float J = 1f / Y.RowCount;
            float regularization = getRegularisation();
            float sum = 0;
            for (int i = 0; i < Y.RowCount; i++)
                for (int k = 0; k < Y.ColumnCount; k++)
                {
                    sum += (float)(Math.Pow((Y.Row(i)[k] - h[i][k]),2));
                }
            J *= sum + regularization;
            return J;
        } //Получение J для оценки правильности обучения
        private List<SparseMatrix> getDerivatives(SparseMatrix Y, SparseMatrix X, List<List<SparseVector>> activations)
        {
            List<Vector<double>> delta = new List<Vector<double>>();
            List<SparseMatrix> D = new List<SparseMatrix>();
            for (int i = 0; i < _layers.Count; i++)
            {
                D.Add(SparseMatrix.Create(_layers[i].Weights.RowCount, _layers[i].Weights.ColumnCount,0));
            }
            for (int k = 0; k < m; k++)
            {
                delta = new List<Vector<double>>();
                for (int i = 0; i < _layers.Count; i++)
                {
                    delta.Add(new SparseVector(_layers[i].Activations.Count));
                }
                for (int i = delta.Count - 1; i >= 0; i--)
                {
                    if (i == delta.Count - 1)
                        for (int j = 0; j < activations[k][i].Count; j++)
                            delta[delta.Count - 1][j] = (activations[k][i][j] - Y.Row(k)[j]);
                    else
                    {
                        delta[i] = _layers[i + 1].Weights.Transpose().Multiply((delta[i + 1]));
                        delta[i] = SparseVector.OfVector(multiplayOnSigmoidGradient(delta[i], activations[k][i]));
                        D[i + 1] = D[i + 1] + (SparseMatrix)(delta[i + 1].OuterProduct(activations[k][i]));
                    }
                    if (i == 0)
                    {
                        Vector<double> tempVector = SparseVector.Create(X.Row(k).Count, 1);
                        if (tempVector.Count != D[i].ColumnCount)
                        {
                            SparseVector tmp = SparseVector.Create(D[i].ColumnCount, 1);
                            for (int j = 1; j < tmp.Count; j++)
                                tmp[j] = X.Row(k)[j - 1];
                            tempVector = tmp;
                        }
                        Matrix<double> tempMatrix = delta[i].OuterProduct(tempVector);
                        D[i] = D[i] + (SparseMatrix)(tempMatrix);
                    }
                }
            }
            List<SparseMatrix> derivatives = new List<SparseMatrix>();
            for (int i = 0; i < D.Count; i++)
            {
                derivatives.Add(D[i] * (1f / m));
            }
            return derivatives;
        } //Получение частных производных
        private void gradientDescent(List<SparseMatrix> derivatives)
        {
            float regularizaton = 0;
            for (int l = 0; l < _layers.Count; l++)
            {
                if (l > 0)
                {
                    if (_layers[l - 1].Activations[0] == 1)
                        regularizaton = 0;
                    else
                        regularizaton = lambda / m;
                }
                _layers[l].Weights = _layers[l].Weights - a * (derivatives[l] + _layers[l].Weights * regularizaton);
            }
        } //Градиентный спуск
        private void sigmoid(ref SparseVector activation)
        {
            for (int i = 0; i < activation.Count; i++)
            {
                //activation[i] = (float)(1 / (1 + Math.Exp(-activation[i])));  // сигмоида
                //activation[i] = Math.Log(1 + Math.Exp(activation[i]));        // ReLU
                activation[i] = Math.Tanh(activation[i]);                       // Гиперболический тангенс
            }
        } //Получение сигмоды
        private Vector<double> multiplayOnSigmoidGradient(Vector<double> delta, Vector<double> activations)
        {
            Vector<double> tmp = DenseVector.Create(delta.Count-1,0);
            for (int i = 1; i < delta.Count; i++)
                tmp[i-1] = (delta[i] * activations[i] * (1 - activations[i]));
            return tmp;
        } //Умножение вектора на производную сигмоиды
        public void StopLearning()
        {
            _stop = true;
        }
        public delegate void NewIteration(int iteration, float J);
        [field: NonSerialized]
        public event NewIteration OnNewIteration;
    }
}
