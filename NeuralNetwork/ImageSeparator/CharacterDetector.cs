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
    public class CharacterDetector
    {
        private Tesseract characterRecognizer;
        public void DetectCharacters(Bitmap imageBitmap)
        {
            characterRecognizer = new Tesseract();
            characterRecognizer.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ-1234567890");
            characterRecognizer.Init(@"./tessdata", "rus", OcrEngineMode.TesseractLstmCombined);
            Image<Bgr, Byte> imageConverted = new Image<Bgr, Byte>(imageBitmap).Copy();
            Mat mat = imageConverted.Mat;
            Pix image = new Pix(mat);
            characterRecognizer.SetImage(image);
            var rec = characterRecognizer.Recognize();
            var characters = characterRecognizer.GetCharacters();
            for (int i=0;i<characters.Length;i++)
            {
                var region = characters[i].Region;
                if (region.Height == 0 || region.Width == 0)
                {
                    Bitmap space = new Bitmap(32, 32);
                    for (int x = 0; x < space.Width; x++)
                    {
                        for (int y = 0; y < space.Height; y++)
                        {
                            space.SetPixel(x, y, Color.White);
                        }
                    }
                    space.Save("images\\" + i.ToString());
                    continue;
                }
                Bitmap letterBitmap = new Bitmap(region.Width, region.Height);
                using (Graphics g = Graphics.FromImage(letterBitmap))
                {
                    g.DrawImage(imageBitmap, new Rectangle(0, 0, letterBitmap.Width, letterBitmap.Height),
                                     region,
                                     GraphicsUnit.Pixel);
                }
                letterBitmap.Save("images\\" + i.ToString());
            }
        }
    }
}
