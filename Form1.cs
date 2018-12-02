using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ANN
{
    public partial class Form1 : Form
    {
        private NeuralNetwork network;
        private int maxBoxOffice = 59842648;
        private int maxDays = 247;
        private float allowableError = 0.1f;
        private char separator = '\t';
        private bool regression = false;
        public Form1()
        {
            InitializeComponent();
            button8.Visible = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            chart1.Size = new Size(1024,1024);
            chart1.Series[1].Color = Color.Gray;
            chart1.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series[0].Color = Color.Red;
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series[0].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chart1.Series[0].BorderWidth = 2;
            chart1.Series[1].BorderWidth = 2;
            label10.Text = "";
            label11.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int inputs = int.Parse(textBox1.Text);
            int outputs = int.Parse(textBox2.Text);
            int layers = int.Parse(textBox3.Text);
            int neurons = int.Parse(textBox4.Text);
            network = new NeuralNetwork(inputs, layers, neurons, outputs);
            network.OnNewIteration += increaseIteration;
            OnStopLearning += network.StopLearning;
            if (dataGridView1.RowCount < inputs)
                for (int i = 0; i < inputs; i++)
                    dataGridView1.Rows.Add();
            if (dataGridView2.RowCount < outputs)
                for (int i = 0; i < outputs; i++)
                    dataGridView2.Rows.Add();
            button1.Visible = false;
            button7.Visible = false;
            button2.Visible = true;
            button3.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            button6.Visible = true;
            textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
            textBox3.ReadOnly = true;
            textBox4.ReadOnly = true;
            checkBox1.Visible = true;
            checkBox2.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < network.Inputs.Count; i++)
            {
                if (dataGridView1[0, i - 1].Value != null)
                    network.Inputs[i] = float.Parse((string)dataGridView1[0, i - 1].Value);
                else
                {
                    MessageBox.Show("Введите данные");
                    return;
                }
            }
            network.ForwardPropagation();
            List<double> Answer = network.GetAswer();
            for (int i = 0; i < network.GetOutputs(); i++)
                dataGridView2[0, i].Value = Answer[i];
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.ShowDialog();
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
            {
                int iterations = 0;
                int m = 0;
                float a = 0;
                float lambda = 0;
                List<List<double>> X = new List<List<double>>();
                List<List<double>> Y = new List<List<double>>();
                if (textBox5.Text != "")
                    a = float.Parse(textBox5.Text);
                if (textBox7.Text != "")
                    lambda = float.Parse(textBox7.Text);
                if (textBox7.Text != "")
                    iterations = int.Parse(textBox6.Text);
                while (!sr.EndOfStream)
                {
                    X.Add(new List<double>());
                    Y.Add(new List<double>());
                    string[] str = sr.ReadLine().Split(separator);
                    for (int j = 0; j < network.GetInputs() - 1; j++)
                    {
                        if (str[j] != "")
                            X[m].Add(float.Parse(str[j]));
                        else
                            X[m].Add(0);
                    }
                    for (int j = 0; j < network.GetOutputs(); j++)
                    {
                        if (str[network.GetInputs() - 1 + j] != "")
                            Y[m].Add(float.Parse(str[network.GetInputs() - 1 + j]));
                        else
                            Y[m].Add(0);
                    }
                    m++;
                }
                SparseMatrix newX = new SparseMatrix(X.Count,X[0].Count);
                for (int i = 0; i < X.Count; i++)
                    newX.SetRow(i, X[i].ToArray());
                SparseMatrix newY = new SparseMatrix(Y.Count, Y[0].Count);
                for (int i = 0; i < Y.Count; i++)
                    newY.SetRow(i, Y[i].ToArray());
                learnProcess(newX, newY, a,lambda,iterations);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog2.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog2.ShowDialog();
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            regression = checkBox2.Checked;
            using (StreamReader sr = new StreamReader(openFileDialog2.FileName))
            {
                int m = 0;
                int errors = 0;
                double error = 0;
                int TruePositive = 0;
                int FalsePositive = 0;
                int FalseNegative = 0;
                List<double> Y = new List<double>();
                while (!sr.EndOfStream)
                {
                    m++;
                    DenseVector X = new DenseVector(network.GetInputs() - 1);
                    string[] str = sr.ReadLine().Split(separator);
                    for (int j = 0; j < network.GetInputs() - 1; j++)
                    {
                        X[j]=float.Parse(str[j]);
                    }
                    network.Inputs = X;
                    network.ForwardPropagation();
                    List<double> h = network.GetAswer();
                    double diff = 0;
                    for (int j = 0; j < network.GetOutputs(); j++)
                    {
                        Y.Add(float.Parse(str[network.GetInputs() - 1 + j]));
                        diff += Math.Abs(Y.Last() - h[j]);
                        if (!regression)
                        {
                            if (diff  > allowableError)
                            {
                                errors++;
                                if (Math.Round(diff) == 1)
                                    FalseNegative++;
                                else
                                    FalsePositive++;
                            }
                            else
                            {
                                if (Math.Round(h[j]) == 1)
                                    TruePositive++;
                            }
                        }
                    }
                    if (regression)
                        error += Math.Pow(diff, 2);
                }
                if (regression)
                {
                    DenseVector Yvector = new DenseVector(Y.ToArray());
                    double middleError = network.getRegressionError(Yvector, error);
                    MessageBox.Show("Нормированная среднеквадратичная ошибка = " + middleError.ToString());
                }
                else
                {
                    double F1 = 1;
                    if (FalseNegative+FalsePositive!=0)
                        F1 = network.getClassificationError(TruePositive, FalsePositive, FalseNegative);
                    MessageBox.Show("F1 мера = " + F1.ToString());
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            openFileDialog3.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog3.ShowDialog();
        }

        private void openFileDialog3_FileOk(object sender, CancelEventArgs e)
        {
            using (StreamReader sr = new StreamReader(openFileDialog3.FileName))
            {
                bool onlyOneRecord = checkBox1.Checked;
                int m = 0;
                int errors = 0;
                float result = 0;
                float sumActually = 0;
                float sumPredicted = 0;
                double error = 0;
                while (!sr.EndOfStream)
                {
                    DenseVector X = new DenseVector(network.GetInputs() - 1);
                    string[] str = sr.ReadLine().Split(separator);
                    for (int j = 0; j < network.GetInputs() - 1; j++)
                    {
                        switch (j)
                        {
                            case 6:
                                if (m != 0)
                                    X[j] =result;
                                else
                                    X[j] =float.Parse(str[j]);
                                break;
                            default:
                                X[j] =float.Parse(str[j]);
                                break;
                        }

                    }
                    network.Inputs = X;
                    if (onlyOneRecord)
                    {
                        int dayWeek = (int)Math.Round(1/X[2]);
                        int month = (int)Math.Round(X[3] * 12);
                        for (int i=1;i< maxDays; i++)
                        {
                            network.Inputs = X;
                            network.ForwardPropagation();
                            List<double> results = network.GetAswer();
                            X[X.Count - 10] = (float)(i) / maxDays;
                            dayWeek++;
                            if (dayWeek > 6)
                                dayWeek = 1;
                            if (i % 31 == 0)
                                month++;
                            X[X.Count - 9] = (float)(1f / (dayWeek));
                            if (month > 12)
                                month = 0;
                            X[X.Count-8] = (float)(month) / 12;
                            X[X.Count-1] = results[0];
                            sumPredicted += (float)(results[0] * maxBoxOffice) ;
                            chart1.Series[0].Points.Add((results[0]));
                        }
                        MessageBox.Show("Суммарные предсказанные сборы: " + Math.Round(sumPredicted).ToString());
                        chart1.SaveImage("Chart.png", System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Png);
                        chart1.Series[0].Points.Clear();
                        return;
                    }
                    network.ForwardPropagation();
                    List<double> h = network.GetAswer();
                    float diff = 0;
                    for (int j = 0; j < network.GetOutputs(); j++)
                    {
                        float Y = float.Parse(str[network.GetInputs() - 1 + j]);
                        result = (float)h[j];
                        diff += (float)Math.Abs(Y - h[j]);
                        sumPredicted += (result * maxBoxOffice);
                        sumActually += (Y * maxBoxOffice);
                        chart1.Series[0].Points.Add((result));
                        chart1.Series[1].Points.Add((Y));
                    }
                    error += Math.Pow(diff, 2);
                    if (diff / network.GetOutputs() > allowableError)
                        errors++;
                    m++;
                }
                float success = (1 - ((float)errors / m)) * 100;
                double middleError = Math.Sqrt(error / (network.GetOutputs() * m));
                chart1.SaveImage("Chart.png", System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Png);
                chart1.Series[0].Points.Clear();
                chart1.Series[1].Points.Clear();
                MessageBox.Show("Суммарные предсказанные сборы: "+ Math.Round(sumPredicted).ToString()+Environment.NewLine+ "Суммарные сборы  на самом деле: " + Math.Round(sumActually).ToString());
            }
        }
        private void increaseIteration(int iteration, float J)
        {
            if (label10.InvokeRequired)
            {
                label10.Invoke(
                  new ThreadStart(delegate {
                      label10.Text = "Текущая итерация: " + iteration.ToString();
                  }));
            }
            else
            {
                label10.Text = "Текущая итерация: " + iteration.ToString();
            }
            if (label11.InvokeRequired)
            {
                label11.Invoke(
                  new ThreadStart(delegate {
                      label11.Text = "Функция ошибки: " + J.ToString();
                  }));
            }
            else
            {
                label11.Text = "Функция ошибки: " + J.ToString();
            }

        }
        //Сохранение
        private void button6_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }
        // Загрузка
        private void button7_Click(object sender, EventArgs e)
        {
            button1.Visible = false;
            button7.Visible = false;
            button2.Visible = true;
            button3.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            button6.Visible = true;
            openFileDialog4.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            BinaryFormatter binFormat = new BinaryFormatter();
            using (Stream sw = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                binFormat.Serialize(sw, network);
            }
        }

        private void openFileDialog4_FileOk(object sender, CancelEventArgs e)
        {
            BinaryFormatter binFormat = new BinaryFormatter();
            using (Stream sr = new FileStream(openFileDialog4.FileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                network = (NeuralNetwork)binFormat.Deserialize(sr);
            }
            network.OnNewIteration += increaseIteration;
            OnStopLearning += network.StopLearning;
            textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
            textBox3.ReadOnly = true;
            textBox4.ReadOnly = true;
            checkBox1.Visible = true;
            checkBox2.Visible = true;
            textBox1.Text = (network.GetInputs()-1).ToString();
            textBox2.Text = network.GetOutputs().ToString();
            textBox3.Text = network.GetLayersCount().ToString();
            textBox4.Text = network.GetNeuronsCount().ToString();
        }

        private void learnProcess(SparseMatrix X, SparseMatrix Y,float a, float lambda, int iterations)
        {
            List<object> tmpContainer = new List<object>();
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button8.Visible = true;
            tmpContainer.Add(X); tmpContainer.Add(Y); tmpContainer.Add(a); tmpContainer.Add(lambda); tmpContainer.Add(iterations);
            Cursor = Cursors.WaitCursor;
            backgroundWorker1.RunWorkerAsync(tmpContainer);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> data = (List <object>)e.Argument;
            SparseMatrix X = (SparseMatrix)data[0]; SparseMatrix Y = (SparseMatrix)data[1]; float a = (float)data[2]; float lambda = (float)data[3]; int iterations = (int)data[4];
            bool endLearning = false;
            while (!endLearning)
            {
                int ret = network.LearnNetwork(X, Y, iterations, a, lambda);
                if (ret == 0)
                {
                    endLearning = true;
                    MessageBox.Show("Сеть обучена.");
                }
                else
                {
                    if (ret == 2)
                    {
                        MessageBox.Show("Обучение остановлено");
                        endLearning = true;
                    }
                    else
                    {
                        DialogResult willContinue = MessageBox.Show("Сеть расходиться, обучение прервано, продолжить с уменьшением LR?", "Внимание", MessageBoxButtons.YesNo);
                        if (willContinue == DialogResult.Yes)
                        {
                            a /= 10f;
                        }
                        else
                        {
                            endLearning = true;
                        }
                    }
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor = Cursors.Default;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button8.Visible = false;
            label10.Text = "";
            label11.Text = "";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            OnStopLearning();
        }
        public delegate void StopLearning();
        public event StopLearning OnStopLearning;
    }
}
