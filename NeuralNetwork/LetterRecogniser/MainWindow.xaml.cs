using ANN;
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
                var trainDirectory = Directory.GetDirectories(directories[dir]).Where(x => x.Contains("train")).FirstOrDefault();
                if (trainDirectory != null)
                {
                    var fileNames = Directory.GetFiles(trainDirectory);
                    for (int files = 0; files < 15; files++)//fileNames.Count(); files++)
                    {
                        byte[] image = getInputFromFile(fileNames[files]);
                        //byte[] image = File.ReadAllBytes(fileNames[files]);
                        X = X.InsertRow(files, DenseVector.OfEnumerable(image.Select(x => (float)x / byte.MaxValue))) as SparseMatrix;
                        string letter = fileNames[files].Split('_')[1];
                        var answer = getOutputVector(letter);
                        Y = Y.InsertRow(files, DenseVector.OfEnumerable(answer)) as SparseMatrix;
                    }
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
        private List<float> getOutputVector(string symbol)
        {
            List<float> result = new List<float>();
            for (int i=0;i<62;i++)
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            var result =  saveFileDialog.ShowDialog();
            if (result!=null && result==true)
            {
                BinaryFormatter binFormat = new BinaryFormatter();
                using (Stream sw = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    binFormat.Serialize(sw, neuralNetwork);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            var isOpen = openFileDialog.ShowDialog();
            if (isOpen!=null && isOpen==true)
            {
                byte[] image = getInputFromFile(openFileDialog.FileName);
                DenseVector X = DenseVector.OfEnumerable(image.Select(x => (float)x / byte.MaxValue));
                neuralNetwork.Inputs = X;
                neuralNetwork.ForwardPropagation();
                var answer = neuralNetwork.GetAswer();
                AnswerTextBox.Text = "";
                foreach (var item in answer)
                {
                    AnswerTextBox.Text += item.ToString() + "\n ";
                }
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
    }
}
