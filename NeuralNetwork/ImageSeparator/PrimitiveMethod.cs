using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSeparator
{
    class PrimitiveMethod
    {

        void Main(string[] args)
        {
            const int size = 16;
            bool open = true;
            string path = "C:\\Users\\Kirill\\Documents\\ИТМО\\Parallel\\data\\page1.jpg";
            if (open)
            {
                Bitmap image = ConvertToBitmap(path);
                List<Bitmap> letters = new List<Bitmap>();
                var width = image.Width / size;
                var height = image.Height / size;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        letters.Add(new Bitmap(size, size));
                        for (int x = 0; x < size; x++)
                        {
                            for (int y = 0; y < size; y++)
                            {
                                Color color = image.GetPixel(i * size + x, j * size + y);
                                letters[i * width + j].SetPixel(x, y, color);
                            }
                        }
                    }
                }
                for (int i = 0; i < letters.Count; i++)
                {
                    letters[i].Save("images\\" + i.ToString() + ".png");
                }
            }
        }

        private Bitmap ConvertToBitmap(string fileName)
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

