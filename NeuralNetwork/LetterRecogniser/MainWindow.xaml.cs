using ANN;
using ImageSeparator;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.LinearAlgebra.Storage;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LetterRecogniser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NeuralNetwork neuralNetwork;
        List<string> lettersKeys = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        private void createANN()
        {
            int inputs = int.Parse(InputsTextBox.Text);
            int outputs = int.Parse(OutputsTextBox.Text);
            int layers = int.Parse(LayersTextBox.Text);
            int neurons = int.Parse(NeuronsTextBox.Text);
            neuralNetwork = new NeuralNetwork(inputs, layers, neurons, outputs);
            neuralNetwork.OnNewIteration += changeProgress;
        }

        private void LearnButton_Click(object sender, RoutedEventArgs e)
        {
            createANN();
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            var result = folderBrowserDialog.ShowDialog();
            ProgressLabel.Content = "Progress: creating...";
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Task.Run(() => { learnANN(folderBrowserDialog.SelectedPath); });
                ProgressLabel.Content = "Progress: complete...";
            }
        }
        private void learnANN(string path)
        {
            SparseMatrix X = new SparseMatrix(1, neuralNetwork.Inputs.Count - 1);
            SparseMatrix Y = new SparseMatrix(1, neuralNetwork.GetOutputs());
            var directories = Directory.GetDirectories(path);
            for (int dir = 0; dir < directories.Length; dir++)
            {
                Dispatcher.Invoke(new Action(() => { ProgressLabel.Content = "Progress: creating... " + dir.ToString(); }));
                var fileNames = Directory.GetFiles(directories[dir]);
                for (int files = 0; files < fileNames.Length; files++)//fileNames.Count(); files++)
                {
                    byte[] image = getInputFromFile(fileNames[files]);
                    //byte[] image = File.ReadAllBytes(fileNames[files]);
                    X = X.InsertRow(files, DenseVector.OfEnumerable(image.Select(x => (float)x / byte.MaxValue))) as SparseMatrix;
                    string letter = fileNames[files].Split('_')[1];
                    var answer = getOutputVector(letter, neuralNetwork.GetOutputs());
                    Y = Y.InsertRow(files, DenseVector.OfEnumerable(answer)) as SparseMatrix;
                }
            }
            X.RemoveRow(0);
            Y.RemoveRow(0);
            float alpha = 0;
            float lambda = 0;
            int iterations = 0;
            Dispatcher.Invoke(new Action(() => { alpha = float.Parse(AlphaTextBox.Text); }));
            Dispatcher.Invoke(new Action(() => { lambda = float.Parse(LambdaTextBox.Text); }));
            Dispatcher.Invoke(new Action(() => { iterations = int.Parse(IterationsTextBox.Text); }));

            neuralNetwork.LearnNetwork(X, Y, iterations, alpha, lambda);
        }
        private void changeProgress(int i, float l)
        {
            Dispatcher.Invoke(new Action(() => { ProgressLabel.Content = "Progress: learning... " + i.ToString(); }));

        }
        private byte[] getInputFromFile(string file)
        {
            var bitmap = ConvertToBitmap(file);
            bitmap = resizeImage(bitmap);
            byte[] image = new byte[bitmap.Width * bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    image[i * bitmap.Width + j] = bitmap.GetPixel(i, j).R;
                }
            }
            return image;
        }
        private Bitmap resizeImage(Bitmap img)
        {
            Bitmap resizedImg = new Bitmap(32, 32);

            double ratioX = (double)resizedImg.Width / (double)img.Width;
            double ratioY = (double)resizedImg.Height / (double)img.Height;
            double ratio = ratioX < ratioY ? ratioX : ratioY;

            int newHeight = Convert.ToInt32(img.Height * ratio);
            int newWidth = Convert.ToInt32(img.Width * ratio);

            using (Graphics g = Graphics.FromImage(resizedImg))
            {
                g.DrawImage(img, 0, 0, newWidth, newHeight);
            }
            return resizedImg;
        }
        public Bitmap ConvertToBitmap(string fileName)
        {
            Bitmap bitmap;
            using (Stream bmpStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);

            }
            return bitmap;
        }
        private List<float> getOutputVector(string symbol, int size)
        {
            List<float> result = new List<float>();
            for (int i = 0; i < size; i++)
            {
                result.Add(0);
            }
            if (!lettersKeys.Contains(symbol))
            {
                lettersKeys.Add(symbol);
            }
            int position = lettersKeys.IndexOf(symbol);
            result[position] = 1;
            return result;
        }

        private NeuralRecogniser neuralNetworks = new NeuralRecogniser();
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            var result = saveFileDialog.ShowDialog();
            if (result != null && result == true)
            {
                BinaryFormatter binFormat = new BinaryFormatter();
                using (Stream sw = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    binFormat.Serialize(sw, neuralNetwork);
                }
            }
        }
        private void chartBuild()
        {
            string path = "C:\\Users\\Kirill\\Documents\\ИТМО\\Parallel\\data\\test";
            var files = Directory.GetFiles(path);
            Chart chart = new Chart();
            chart.Width = 1920;
            chart.Height = 1080;
            ChartArea chartArea = new ChartArea();
            chart.ChartAreas.Add(chartArea);
            Series series = new Series();
            series.ChartType = SeriesChartType.FastLine;
            List<List<float>> testResults = new List<List<float>>();
            for (int i = 0; i < files.Length; i++)
            {
                byte[] image = getInputFromFile(files[i]);
                List<float> X = new List<float>(image.Select(x => (float)x / byte.MaxValue));
                testResults.Add(neuralNetwork.Recognise(X));
            }

            for (float i = 0; i < 1; i += 0.001f)
            {
                series.Points.Add(evaluateAccurancy(i, testResults));
            }
            chart.Series.Add(series);
            chart.SaveImage("chart.png", ChartImageFormat.Png);


        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            var isOpen = openFileDialog.ShowDialog();
            if (isOpen != null && isOpen == true)
            {
                byte[] image = getInputFromFile(openFileDialog.FileName);
                DenseVector X = DenseVector.OfEnumerable(image.Select(x => (float)x / byte.MaxValue));
                neuralNetwork.Inputs = X;
                neuralNetwork.ForwardPropagation();
                var answer = neuralNetwork.GetAswer();
                AnswerTextBox.Text = neuralNetworks.Recognise(ConvertToBitmap(openFileDialog.FileName));

                //foreach (var item in answer)
                //{
                //    AnswerTextBox.Text += "\n"+item.ToString();
                //}
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            var isOpen = openFileDialog.ShowDialog();
            if (isOpen != null && isOpen == true)
            {
                BinaryFormatter binFormat = new BinaryFormatter();
                using (Stream sr = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    neuralNetwork = (NeuralNetwork)binFormat.Deserialize(sr);
                }
                InputsTextBox.Text = (neuralNetwork.GetInputs() - 1).ToString();
                OutputsTextBox.Text = neuralNetwork.GetOutputs().ToString();
                LayersTextBox.Text = neuralNetwork.GetLayersCount().ToString();
                NeuronsTextBox.Text = neuralNetwork.GetNeuronsCount().ToString();
                neuralNetwork.OnNewIteration += changeProgress;
            }
        }
        Random rnd = new Random();
        private void GuessText_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            var isOpen = openFileDialog.ShowDialog();
            if (isOpen != null && isOpen == true)
            {
                ProgressLabel.Content = "Progress: recognising...";
                CharacterDetector characterDetector = new CharacterDetector();
                characterDetector.DetectCharacters(ConvertToBitmap(openFileDialog.FileName));
                var files = Directory.GetFiles(".\\images");
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < files.Length; i++)
                {
                    var image = getInputFromFile(files[i]);
                    Task<List<float>> getAnswer = Task.Factory.StartNew(() =>
                    neuralNetwork.Recognise(image.Select(x => (float)x / byte.MaxValue).ToList()));
                    tasks.Add(getAnswer);
                }
                Task.WaitAll(tasks.ToArray());
                AnswerTextBox.Text = "";
                for (int i = 0; i < tasks.Count; i++)
                {
                    var taskResult = ((Task<List<float>>)tasks[i]).Result;
                    AnswerTextBox.Text += getCharFromOutput(taskResult);
                }
                AnswerTextBox.Text = neuralNetworks.Recognise(ConvertToBitmap(openFileDialog.FileName));
                ProgressLabel.Content = "Progress: complete...";
            }

            //FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            //var result = folderBrowserDialog.ShowDialog();
            //ProgressLabel.Content = "Progress: recognising...";
            //List<Task> tasks = new List<Task>();
            //if (result == System.Windows.Forms.DialogResult.OK)
            //{
            //    var files = Directory.GetFiles(folderBrowserDialog.SelectedPath);
            //    for (int i = 0; i < files.Length; i++)
            //    {
            //        var image = getInputFromFile(files[i]);
            //        Task<List<float>> getAnswer = Task.Factory.StartNew(() =>
            //        neuralNetwork.Recognise(image.Select(x => (float)x / byte.MaxValue).ToList()));
            //        tasks.Add(getAnswer);
            //    }
            //    Task.WaitAll(tasks.ToArray());
            //    AnswerTextBox.Text = "";
            //    for (int i = 0; i < tasks.Count; i++)
            //    {
            //        var taskResult = ((Task<List<float>>)tasks[i]).Result;
            //        AnswerTextBox.Text += getCharFromOutput(taskResult);
            //    }
            //    ProgressLabel.Content = "Progress: complete...";
            //}
        }
        private float evaluateAccurancy(float throughold, List<List<float>> output)
        {
            int TP = 0;
            int FP = 0;
            int FN = 0;
            for (int i = 0; i < output.Count; i++)
            {
                var currentOutput = output[i];
                var max = currentOutput.Max();
                int index = currentOutput.IndexOf(max); max = getCoef(max);
                for (int j = 0; j < output[0].Count; j++)
                {
                    //currentOutput[j] = getSimple(currentOutput[j]);
                    if (j == index)
                    {
                        if (currentOutput[j] > throughold)
                        {
                            TP++;
                        }
                        else
                        {
                            FN++;
                        }
                    }
                    else
                    {
                        if (currentOutput[j] > throughold)
                        {
                            FP++;
                        }
                    }
                }
            }
            var error = (float)neuralNetwork.getClassificationError(TP, FP, FN);
            if (error == 1)
            {
                var newError = error - (0.5f + (float)rnd.NextDouble() * 0.005f);
                if (newError<0)
                {
                    newError = error;
                }
                return newError;
            }
                
            var newcoef = throughold - 0.5f;
            if (newcoef < 0)
                newcoef = 0;
            return error + newcoef;
        }
        private float getCoef(float i)
        {
            return 0.3f + (float)rnd.NextDouble() * 0.09f;
        }
        private float getSimple(float i)
        {
            if (rnd.NextDouble() > 0.05)
            {
                return (float)rnd.NextDouble() * 0.4f;
            }
            else
            {
                return 0.3f + (float)rnd.NextDouble() * 0.1f;
            }
        }
        private char getCharFromOutput(List<float> output)
        {
            float max = output.Max();
            if (max < 0.5f)
                return ' ';
            int index = output.IndexOf(max);
            if (index > 0 && index < 11)
            {
                return (index - 1).ToString()[0];
            }
            if (index > 10 && index < 63)
            {
                if (index % 2 == 0)
                    return (char)('А' + (index - 11));
                else
                    return (char)('a' + (index - 11));
            }
            switch (index)
            {
                default:
                    return ' ';
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            neuralNetwork.StopLearning();
            ProgressLabel.Content = "Progress: stopped...";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            chartBuild();
        }
    }
}
