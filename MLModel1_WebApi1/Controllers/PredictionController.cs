using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using static MLModel1.ModelOutput;
using Color = System.Drawing.Color;

namespace MLModel1_WebApi1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PredictionController : ControllerBase
    {
        [HttpPost("predict-image")]
        public async Task<IActionResult> Predict([FromServices] PredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput> predictionEnginePool, IFormFile file)
        {
            var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            ms.Position = 0;

            var image = MLImage.CreateFromStream(ms);
            var input = new MLModel1.ModelInput()
            {
                ImageSource = image,
            };

            var result = predictionEnginePool.Predict(input);

            return Ok(result);
        }

        [HttpPost("predict-with-image-result")]
        public async Task<IActionResult> PredictWithImageResult([FromServices] PredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput> predictionEnginePool, IFormFile file)
        {
            var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            ms.Position = 0;

            var image = MLImage.CreateFromStream(ms);

            var input = new MLModel1.ModelInput()
            {
                ImageSource = image,
            };

            var result = predictionEnginePool.Predict(input);

            var ms2 = new MemoryStream();
            await file.CopyToAsync(ms2);

            ms2.Position = 0;

            byte[] imageBytes = DrawRectangles(ms2, result.BoundingBoxes.ToList());

          
            return File(imageBytes, "image/jpeg");
        }

        //public static byte[] DrawRectangles(MemoryStream inputImageStream, List<BoundingBox> boundingBoxes)
        //{
        //    Image inputImage = Image.FromStream(inputImageStream);
        //    Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height, PixelFormat.Format32bppArgb);

        //    var rectangles = new List<RectangleF>();

        //    using (Graphics graphics = Graphics.FromImage(outputImage))
        //    {
        //        graphics.DrawImage(inputImage, 0, 0, inputImage.Width, inputImage.Height);

        //        Pen pen = new Pen(Color.Red, 2);

        //        foreach (var box in boundingBoxes)
        //        {
        //            RectangleF rect = BoundingBoxToRectangle(box);
        //            graphics.DrawRectangle(pen, rect);
        //        }
        //    }

        //    using (MemoryStream outputStream = new MemoryStream())
        //    {
        //        outputImage.Save(outputStream, ImageFormat.Jpeg);
        //        return outputStream.ToArray();
        //    }
        //}

        public static byte[] DrawRectangles(MemoryStream inputImageStream, List<BoundingBox> boundingBoxes)
        {
            Image inputImage = Image.FromStream(inputImageStream);
            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height, PixelFormat.Format32bppArgb);

            var rectangles = new List<RectangleF>();

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(inputImage, 0, 0, inputImage.Width, inputImage.Height);

                Pen pen = new Pen(Color.Red, 2);

                foreach (var box in boundingBoxes)
                {
                    RectangleF rect = BoundingBoxToRectangle(box);
                    rectangles.Add(rect);
                }

                foreach (var item in NonMaximumSuppression(rectangles, 0.1f))
                {
                    graphics.DrawRectangle(pen, item);
                }
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                outputImage.Save(outputStream, ImageFormat.Jpeg);
                return outputStream.ToArray();
            }
        }

        public static RectangleF BoundingBoxToRectangle(BoundingBox boundingBox)
        {
            var left = (boundingBox.Left / 800) * 500;
            var top = (boundingBox.Top / 600) * 500;
            var right = (boundingBox.Right / 800) * 500;
            var bottom = (boundingBox.Bottom / 600) * 500;

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        public static List<RectangleF> NonMaximumSuppression(List<RectangleF> rectangles, float iouThreshold)
        {
            List<RectangleF> outputRectangles = new List<RectangleF>();

            // Sort rectangles by area in descending order
            var sortedRectangles = rectangles.OrderByDescending(r => r.Width * r.Height).ToList();

            while (sortedRectangles.Count > 0)
            {
                RectangleF currentRect = sortedRectangles[0];
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

        public static float ComputeIoU(RectangleF rect1, RectangleF rect2)
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

        //public static List<BoundingBox> NonMaximumSuppression(List<BoundingBox> boundingBoxes, float iouThreshold)
        //{
        //    List<BoundingBox> outputBoxes = new List<BoundingBox>();

        //    // Sort bounding boxes by score in descending order
        //    var sortedBoxes = boundingBoxes.OrderByDescending(b => b.Score).ToList();

        //    while (sortedBoxes.Count > 0)
        //    {
        //        BoundingBox currentBox = sortedBoxes[0];
        //        outputBoxes.Add(currentBox);
        //        sortedBoxes.RemoveAt(0);

        //        for (int i = sortedBoxes.Count - 1; i >= 0; i--)
        //        {
        //            if (ComputeIoU(currentBox, sortedBoxes[i]) > iouThreshold)
        //            {
        //                sortedBoxes.RemoveAt(i);
        //            }
        //        }
        //    }

        //    return outputBoxes;
        //}

        //public static float ComputeIoU(BoundingBox box1, BoundingBox box2)
        //{
        //    float xA = Math.Max(box1.Left, box2.Left);
        //    float yA = Math.Max(box1.Top, box2.Top);
        //    float xB = Math.Min(box1.Right, box2.Right);
        //    float yB = Math.Min(box1.Bottom, box2.Bottom);

        //    float intersectionArea = Math.Max(0, xB - xA) * Math.Max(0, yB - yA);
        //    float box1Area = (box1.Right - box1.Left) * (box1.Bottom - box1.Top);
        //    float box2Area = (box2.Right - box2.Left) * (box2.Bottom - box2.Top);

        //    float unionArea = box1Area + box2Area - intersectionArea;

        //    return intersectionArea / unionArea;
        //}
    }
}
