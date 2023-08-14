using System.Drawing.Imaging;
using System.Drawing;
using static MLModel1.ModelOutput;
using System.Runtime.Versioning;
using Image = System.Drawing.Image;
using RectangleF = System.Drawing.RectangleF;
using Color = System.Drawing.Color;

namespace MLModel1_WebApi1.Services
{
    [SupportedOSPlatform("windows")]
    public class DrawingService : IDrawingService
    {
        public async Task<byte[]> DrawRectangles(IFormFile file, List<BoundingBox> boundingBoxes, float iouThreshold = 0.01f, float inputImageWidth = 500f, float inputImageHeight = 500f)
        {
            var inputImageStream = new MemoryStream();
            await file.CopyToAsync(inputImageStream);
            inputImageStream.Position = 0;
            
            var inputImage = Image.FromStream(inputImageStream);
            var outputImage = new Bitmap(inputImage.Width, inputImage.Height, PixelFormat.Format32bppArgb);

            var rectangles = new List<RectangleF>();

            using (var graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(inputImage, 0, 0, inputImage.Width, inputImage.Height);
                var pen = new Pen(Color.Red, 2);

                foreach (var box in boundingBoxes)
                {
                    RectangleF rect = BoundingBoxToRectangle(box, inputImageWidth, inputImageHeight);
                    rectangles.Add(rect);
                }

                foreach (var item in NonMaximumSuppression(rectangles, iouThreshold))
                {
                    graphics.DrawRectangle(pen, item);
                }
            }

            using (var outputStream = new MemoryStream())
            {
                outputImage.Save(outputStream, ImageFormat.Jpeg);
                return outputStream.ToArray();
            }
        }

        private RectangleF BoundingBoxToRectangle(BoundingBox boundingBox, float inputImageWidth, float inputImageHeight)
        {
            var left = (boundingBox.Left / 800) * inputImageWidth;
            var top = (boundingBox.Top / 600) * inputImageHeight;
            var right = (boundingBox.Right / 800) * inputImageWidth;
            var bottom = (boundingBox.Bottom / 600) * inputImageHeight;

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        private List<RectangleF> NonMaximumSuppression(List<RectangleF> rectangles, float iouThreshold)
        {
            var outputRectangles = new List<RectangleF>();

            var sortedRectangles = rectangles.OrderByDescending(r => r.Width * r.Height).ToList();

            while (sortedRectangles.Count > 0)
            {
                var currentRect = sortedRectangles[0];
                outputRectangles.Add(currentRect);
                sortedRectangles.RemoveAt(0);

                for (int i = sortedRectangles.Count - 1; i >= 0; i--)
                {
                    if (ComputeIoU(currentRect, sortedRectangles[i]) > iouThreshold)
                    {
                        sortedRectangles.RemoveAt(i);
                    }
                }
            }

            return outputRectangles;
        }

        private float ComputeIoU(RectangleF rect1, RectangleF rect2)
        {
            float xA = Math.Max(rect1.Left, rect2.Left);
            float yA = Math.Max(rect1.Top, rect2.Top);
            float xB = Math.Min(rect1.Right, rect2.Right);
            float yB = Math.Min(rect1.Bottom, rect2.Bottom);

            float intersectionArea = Math.Max(0, xB - xA) * Math.Max(0, yB - yA);
            float rect1Area = rect1.Width * rect1.Height;
            float rect2Area = rect2.Width * rect2.Height;

            float unionArea = rect1Area + rect2Area - intersectionArea;

            return intersectionArea / unionArea;
        }
    }
}
