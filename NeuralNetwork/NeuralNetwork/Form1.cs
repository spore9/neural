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
using MathNet.Numerics.LinearAlgebra.Single;
using ANN;

namespace ArtificialNeuralNetwork.Program
{
    public partial class Form1 : Form
    {
        private NeuralNetwork network;
        private int maxBoxOffice = 59842648;
        private int maxDays = 247;
        private float allowableError = 0.1f;
        private char separator = '\t';
        private BackgroundWorker backgroundWorker1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private DataGridView dataGridView1;
        private DataGridView dataGridView2;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label label10;
        private Label label11;
        private Label label12;
        private OpenFileDialog openFileDialog1;
        private TextBox textBox1;
        private TextBox textBox2;
        private OpenFileDialog openFileDialog2;
        private OpenFileDialog openFileDialog3;
        private OpenFileDialog openFileDialog4;
        private SaveFileDialog saveFileDialog1;
        private TextBox textBox3;
        private TextBox textBox4;
        private TextBox textBox5;
        private TextBox textBox6;
        private TextBox textBox7;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
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
            List<float> Answer = network.GetAswer();
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
                List<List<float>> X = new List<List<float>>();
                List<List<float>> Y = new List<List<float>>();
                if (textBox5.Text != "")
                    a = float.Parse(textBox5.Text);
                if (textBox7.Text != "")
                    lambda = float.Parse(textBox7.Text);
                if (textBox7.Text != "")
                    iterations = int.Parse(textBox6.Text);
                while (!sr.EndOfStream)
                {
                    X.Add(new List<float>());
                    Y.Add(new List<float>());
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
                List<float> Y = new List<float>();
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
                    List<float> h = network.GetAswer();
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
                            List<float> results = network.GetAswer();
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
                    List<float> h = network.GetAswer();
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

        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialog3 = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialog4 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 58);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Create";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 118);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Get Answer";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(103, 58);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "Teach ANN";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(195, 58);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 3;
            this.button4.Text = "Test";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(291, 58);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 4;
            this.button5.Text = "Predict";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(103, 118);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 5;
            this.button6.Text = "SaveANN";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(195, 118);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 6;
            this.button7.Text = "Load ANN";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(291, 118);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 23);
            this.button8.TabIndex = 7;
            this.button8.Text = "Stop";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(515, 63);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(94, 17);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "Predict by one";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(515, 35);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(85, 17);
            this.checkBox2.TabIndex = 9;
            this.checkBox2.Text = "Is regression";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(58, 358);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(240, 150);
            this.dataGridView1.TabIndex = 10;
            // 
            // dataGridView2
            // 
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Location = new System.Drawing.Point(422, 358);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.Size = new System.Drawing.Size(240, 150);
            this.dataGridView2.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Inputs";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(134, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Outputs";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(246, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Layers";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(373, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Neurons";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(385, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Alpha";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(385, 104);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Iterations";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(385, 145);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Lambda";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(512, 13);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Modes";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(55, 335);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(36, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "Inputs";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(387, 202);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(48, 13);
            this.label10.TabIndex = 21;
            this.label10.Text = "Iteration:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(387, 226);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(32, 13);
            this.label11.TabIndex = 22;
            this.label11.Text = "Loss:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(422, 335);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(44, 13);
            this.label12.TabIndex = 23;
            this.label12.Text = "Outputs";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 32);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 24;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(131, 32);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 25;
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            // 
            // openFileDialog3
            // 
            this.openFileDialog3.FileName = "openFileDialog3";
            // 
            // openFileDialog4
            // 
            this.openFileDialog4.FileName = "openFileDialog4";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(249, 32);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 20);
            this.textBox3.TabIndex = 26;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(366, 32);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(100, 20);
            this.textBox4.TabIndex = 27;
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(388, 79);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(100, 20);
            this.textBox5.TabIndex = 28;
            // 
            // textBox6
            // 
            this.textBox6.Location = new System.Drawing.Point(388, 120);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(100, 20);
            this.textBox6.TabIndex = 29;
            // 
            // textBox7
            // 
            this.textBox7.Location = new System.Drawing.Point(388, 161);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(100, 20);
            this.textBox7.TabIndex = 30;
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(645, 29);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series2";
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(300, 300);
            this.chart1.TabIndex = 31;
            this.chart1.Text = "chart1";
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(798, 541);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataGridView2);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
