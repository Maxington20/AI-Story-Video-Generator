using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGptStoryGenerator
{
    public static class AddTextToImage
    {
        public static void AddTextToStaticImage(string content, string imageFilePath, string imageWithTextFilePath)
        {
            // Add text to the existing image and resave the image
            System.Drawing.Image image = System.Drawing.Image.FromFile(imageFilePath);

            // Create a font object
            Font font = new Font("Arial", 24, FontStyle.Bold);

            // Calculate the size of the text that can fit on the image
            SizeF textSize = new SizeF();
            using (Graphics graphics = Graphics.FromImage(image))
            {
                textSize = graphics.MeasureString(content, font);
            }

            // Check if the text fits on the image
            if (textSize.Width <= image.Width && textSize.Height <= image.Height)
            {
                // Draw the text on the image
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    graphics.DrawString(content, font, Brushes.Black, new PointF(0, 0));
                }
            }
            else
            {
                // Calculate the maximum width and height of the text that can fit on the image
                float maxWidth = image.Width;
                float maxHeight = image.Height;

                // Decrease the font size until the text fits on the image
                while (textSize.Width > maxWidth || textSize.Height > maxHeight)
                {
                    font = new Font(font.FontFamily, font.Size - 1, font.Style);
                    using (Graphics graphics = Graphics.FromImage(image))
                    {
                        textSize = graphics.MeasureString(content, font);
                    }
                }

                // Draw the text on the image
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    graphics.DrawString(content, font, Brushes.Black, new PointF(0, 0));
                }
            }

            // save the new image
            image.Save(imageWithTextFilePath);
        }
    }
}
