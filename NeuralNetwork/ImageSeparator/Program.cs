using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSeparator
{
    class Program
    {
        static void Main(string[] args)
        {
            const int size = 16;
            bool open = true;
            string path = "C:\\Users\\Kirill\\Documents\\ИТМО\\Parallel\\data\\2.1-1.jpg";
            if (open)
            {
                Bitmap image = ConvertToBitmap(path);
                CharacterDetector characterDetector = new CharacterDetector();
                characterDetector.DetectCharacters(image);
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
