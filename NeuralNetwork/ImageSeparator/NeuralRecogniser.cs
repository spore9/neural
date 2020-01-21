using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSeparator
{
    public class NeuralRecogniser
    {
        Tesseract ocr;
        public NeuralRecogniser()
        {
            ocr = new Tesseract();
            ocr.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ-1234567890");
            ocr.Init(@"C:/Users/Kirill/source/repos/neural/NeuralNetwork/ImageSeparator/bin/Debug/tessdata", "rus", OcrEngineMode.TesseractLstmCombined);
        }
        public string Recognise(Bitmap imageBitmap)
        {
            Image<Bgr, Byte> imageConverted = new Image<Bgr, Byte>(imageBitmap).Copy();
            Mat mat = imageConverted.Mat;
            Pix image = new Pix(mat);
            ocr.SetImage(image);
            ocr.Recognize();
            return ocr.GetUTF8Text();
        }
    }
}
