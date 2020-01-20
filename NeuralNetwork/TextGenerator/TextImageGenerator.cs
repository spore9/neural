using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextGenerator
{
    public class TextImageGenerator
    {
        public void GenerateText(int textSize, string fileName)
        {
            string text = LoremIpsum(textSize / 2, textSize * 2, textSize / 7, textSize / 3, textSize / 50);
            Font font = new Font(new FontFamily("Arial"), 16, FontStyle.Regular, GraphicsUnit.Pixel);
            Image textImage = getImage(text, font, Color.Black, Color.White);
            textImage.Save(fileName, ImageFormat.Png);
        }
        private Image getImage(String text, Font font, Color textColor, Color backColor)
        {
            float width = 1024;
            float height = 4096;            
            var size = TextRenderer.MeasureText(text, font,new Size((int)width,(int)height));
            //create a new image of the right size
            Image img = new Bitmap((int)size.Width, (int)size.Height);

            Graphics drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            TextRenderer.DrawText(drawing, text, font, new Point(0, 0), textColor, TextFormatFlags.Default);
            //drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();
            return img;
        }
        string LoremIpsum(int minWords, int maxWords,
    int minSentences, int maxSentences,
    int numParagraphs)
        {

            var words = new[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
        "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
        "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};

            var rand = new Random();
            int numSentences = rand.Next(maxSentences - minSentences)
                + minSentences + 1;
            int numWords = rand.Next(maxWords - minWords) + minWords + 1;

            StringBuilder result = new StringBuilder();

            for (int p = 0; p < numParagraphs; p++)
            {
                result.Append("\n");
                for (int s = 0; s < numSentences; s++)
                {
                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0) { result.Append(" "); }
                        result.Append(words[rand.Next(words.Length)]);
                    }
                    result.Append(". ");
                }
            }

            return result.ToString();
        }
    }
}
