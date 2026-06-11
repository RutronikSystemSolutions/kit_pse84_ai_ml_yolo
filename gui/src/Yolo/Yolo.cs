using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kit_pse84_ai_streaming.Yolo
{
    public class Yolo
    {
        private static int DIM = 320;
        // 0, 1, 2, ... 9
        private static int NUM_CLASSES = 10;
        // private static float MIN_CONFIDENCE = 0.8f;
        // private static float MIN_CONFIDENCE = 0.5f;
        private static float MIN_CONFIDENCE = 0.4f;
        private static float OVERLAP_THRESHOLD = 0.45f;

        private static string[] LABELS = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];

        /// <summary>
        /// Convert a Bitmap into a Tensor that can be used as input for the model
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static DenseTensor<float> ExtractPixels(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var tensor = new DenseTensor<float>([1, 3, DIM, DIM]);
            // int pixelSize = 3;
            int pixelSize = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int height = bmp.Height;
            int width = bmp.Width;

            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                for (int y = 0; y < height; ++y)
                {
                    // byte* row = (byte*)bmpData.Scan0 + (y * bmpData.Stride);
                    for (int x = 0; x < width; ++x)
                    {
                        Color col = bmp.GetPixel(x, y);

                        tensor[0, 0, y, x] = col.R / 255.0f;
                        tensor[0, 1, y, x] = col.G / 255.0f;
                        tensor[0, 2, y, x] = col.B / 255.0f;
                    }
                }
            }
            else
            {
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
                unsafe
                {
                    for (int y = 0; y < height; ++y)
                    {
                        byte* row = (byte*)bmpData.Scan0 + (y * bmpData.Stride);
                        for (int x = 0; x < width; ++x)
                        {
                            tensor[0, 0, y, x] = row[x * pixelSize + 2] / 255.0f;
                            tensor[0, 1, y, x] = row[x * pixelSize + 1] / 255.0f;
                            tensor[0, 2, y, x] = row[x * pixelSize + 0] / 255.0f;
                        }
                    }
                }
            }

            return tensor;
        }

        //public static DenseTensor<float> ExtractPixels1(Bitmap bmp)
        //{
        //    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        //    var tensor = new DenseTensor<float>([1, 3, DIM, DIM]);
        //    for (int y = 0; y < bmp.Height; ++y)
        //    {
        //        for (int x = 0; x < bmp.Width; ++x)
        //        {
        //            tensor[0, 0, y, x] = bmp.GetPixel(x, y).R / 255.0F; // r
        //            tensor[0, 1, y, x] = bmp.GetPixel(x, y).G / 255.0F; // g
        //            tensor[0, 2, y, x] = bmp.GetPixel(x, y).B / 255.0F; // b
        //        }
        //    }

        //    return tensor;
        //}

        public static List<YoloPrediction> ParseResults(Tensor<float> results)
        {
            var retval = new List<YoloPrediction>();

            // YOLOv5 output is typically [1, 14, 2100] - need to transpose
            // Where 14 = 4 box coords + 10 classes, and 2100 is number of anchor boxes
            int dimensions = 4 + NUM_CLASSES; // 14
            int numBoxes = results.Dimensions[2]; // 2100

            for (int i = 0; i < numBoxes; ++i)
            {
                // YOLOv5 format: output[0, row, column] where row is feature index, column is box index
                // Rows 0-3: x, y, w, h (center coords and dimensions)
                // Rows 4-13: class probabilities for 10 classes
                float x = results[0, 0, i];
                float y = results[0, 1, i];
                float w = results[0, 2, i];
                float h = results[0, 3, i];

                // Get the maximum confidence for this box
                // Find the class with highest probability
                float maxClassScore = float.MinValue;
                int classId = -1;
                for (int c = 0; c < NUM_CLASSES; c++)
                {
                    float score = results[0, 4 + c, i];
                    if (score > maxClassScore)
                    {
                        maxClassScore = score;
                        classId = c;
                    }
                }

                // Enough?
                if (maxClassScore < MIN_CONFIDENCE) continue;

                retval.Add(new YoloPrediction(LABELS[classId],
                    maxClassScore,
                    new RectangleF(x - w / 2, y - h / 2, w, h)));
            }

            return retval;
        }

        public static List<YoloPrediction> Suppress(List<YoloPrediction> items)
        {
            var result = new List<YoloPrediction>(items);

            foreach (var item in items) // iterate every prediction
            {
                foreach (var current in result.ToList().Where(current => current != item)) // make a copy for each iteration
                {
                    var (rect1, rect2) = (item.rectangle, current.rectangle);

                    var intersection = RectangleF.Intersect(rect1, rect2);

                    var intArea = intersection.Width * intersection.Height;
                    var unionArea = (rect1.Width * rect1.Height) + (rect2.Width * rect2.Height) - intArea; // union area
                    var overlap = intArea / unionArea; // overlap ratio

                    if (overlap >= OVERLAP_THRESHOLD)
                    {
                        if (item.confidence >= current.confidence)
                        {
                            result.Remove(current);
                        }
                    }
                }
            }

            return result;
        }
    }
}
