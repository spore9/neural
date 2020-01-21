using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageSeparator
{
    class Program
    {
        static void Main(string[] args)
        {
            bool open = true;
            createLearnSet();
            string path = "C:\\Users\\Kirill\\Documents\\ИТМО\\Parallel\\data\\page1.jpg";
            if (open)
            {
                Bitmap image = ConvertToBitmap(path);
                CharacterDetector characterDetector = new CharacterDetector();
                characterDetector.DetectCharacters(image);
            }
        }

        static private void createLearnSet()
        {
            string path = "C:\\Users\\Kirill\\Documents\\ИТМО\\Parallel\\data\\learn";
            string chars = ".,!-1234567890абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
            FontFamily timesNewRoman = new FontFamily("Times New Roman");
            Font fontTimesNewRoman = new Font(timesNewRoman, 32, FontStyle.Regular, GraphicsUnit.Pixel);
            FontFamily arial = new FontFamily("Arial");
            Font fontArial = new Font(arial, 32, FontStyle.Regular, GraphicsUnit.Pixel);
            FontFamily calibri = new FontFamily("Calibri");
            Font fontCalibri = new Font(calibri, 32, FontStyle.Regular, GraphicsUnit.Pixel);
            for (int i=0;i< chars.Length;i++)
            {
                for (int j=0;j<3;j++)
                {
                    Font currentFont;
                    string fontName;
                    switch (j)
                    {
                        case 0:
                            currentFont = fontTimesNewRoman;
                            fontName = "_tnr";
                            break;
                        case 1:
                            currentFont = fontArial;
                            fontName = "_ari";
                            break;
                        default:
                            currentFont = fontCalibri;
                            fontName = "_clb";
                            break;
                    }
                    Bitmap bitmap = new Bitmap(32, 32);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    //graphics.DrawString(chars[i].ToString(), currentFont, new SolidBrush(Color.FromArgb(0, 0, 0)), 0, 0);
                    TextRenderer.DrawText(graphics, chars[i].ToString(), currentFont,new Point(0,0), Color.Black);
                    graphics.Flush();
                    graphics.Dispose();
                    string charName = chars[i].ToString();
                    if (char.IsLower(chars[i]))
                    {
                        charName = chars[i] + "!";
                    }
                    if (!Directory.Exists(path + "\\" + charName))
                    {
                        Directory.CreateDirectory(path + "\\" + charName);
                    }
                    string newPath = path + "\\" + charName + "\\_" + charName + fontName + ".png";
                    bitmap.Save(newPath);
                }

            }
        }

        static private Bitmap ConvertToBitmap(string fileName)
        {
            Bitmap bitmap;
            using (Stream bmpStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);

            }
            return bitmap;
        }
    }
}
