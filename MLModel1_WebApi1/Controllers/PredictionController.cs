using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using System.Drawing;
using System.Drawing.Imaging;
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

        public static byte[] DrawRectangles(MemoryStream inputImageStream, List<BoundingBox> boundingBoxes)
        {
            Image inputImage = Image.FromStream(inputImageStream);
            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(inputImage, 0, 0, inputImage.Width, inputImage.Height);

                Pen pen = new Pen(Color.Red, 2);

                foreach (var box in boundingBoxes)
                {
                    Rectangle rect = BoundingBoxToRectangle(box);
                    graphics.DrawRectangle(pen, rect);
                }
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                outputImage.Save(outputStream, ImageFormat.Jpeg);
                return outputStream.ToArray();
            }
        }

            //public static byte[] DrawRectangles(MemoryStream inputImageStream, List<MLModel1.ModelOutput.BoundingBox> boundingBoxes)
            //{
            //    System.Drawing.Image image = System.Drawing.Image.FromStream(inputImageStream);

            //    using (Graphics graphics = Graphics.FromImage(image))
            //    {
            //        Pen pen = new Pen(Color.Red, 2);

            //        foreach (var box in boundingBoxes)
            //        {
            //            Rectangle rect = BoundingBoxToRectangle(box);

            //            graphics.DrawRectangle(pen, rect);
            //        }
            //    }

            //    using (MemoryStream outputStream = new MemoryStream())
            //    {
            //        image.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            //        return outputStream.ToArray();
            //    }
            //}

        public static Rectangle BoundingBoxToRectangle(BoundingBox boundingBox)
        {
            int x = (int)boundingBox.Left;
            int y = (int)boundingBox.Top;
            int width = (int)(boundingBox.Right - boundingBox.Left);
            int height = (int)(boundingBox.Bottom - boundingBox.Top);

            var x1 = (int)boundingBox.Top;
            var y1 = (int)boundingBox.Left;
            var x2 = (int)boundingBox.Right;
            var y2 = (int)boundingBox.Bottom;

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);


        }

        //public System.Drawing.Image DrawBoundingBox(MemoryStream ms, List<MLModel1.ModelOutput.BoundingBox> filteredBoxes)
        //{
        //    System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
        //    var originalHeight = image.Height;
        //    var originalWidth = image.Width;
        //    foreach (var box in filteredBoxes)
        //    {
        //        //// process output boxes
        //        var x = (uint)Math.Max(box.Dimensions.X, 0);
        //        var y = (uint)Math.Max(box.Dimensions.Y, 0);
        //        var width = (uint)Math.Min(originalWidth - x, box.Dimensions.Width);
        //        var height = (uint)Math.Min(originalHeight - y, box.Dimensions.Height);

        //        // fit to current image size
        //        x = (uint)originalWidth * x / ImageSettings.imageWidth;
        //        y = (uint)originalHeight * y / ImageSettings.imageHeight;
        //        width = (uint)originalWidth * width / ImageSettings.imageWidth;
        //        height = (uint)originalHeight * height / ImageSettings.imageHeight;

        //        using (Graphics thumbnailGraphic = Graphics.FromImage(image))
        //        {
        //            thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
        //            thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
        //            thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

        //            // Define Text Options
        //            Font drawFont = new Font("Arial", 12, FontStyle.Bold);
        //            SizeF size = thumbnailGraphic.MeasureString(box.Description, drawFont);
        //            SolidBrush fontBrush = new SolidBrush(Color.Black);
        //            Point atPoint = new Point((int)x, (int)y - (int)size.Height - 1);

        //            // Define BoundingBox options
        //            Pen pen = new Pen(box.BoxColor, 3.2f);
        //            SolidBrush colorBrush = new SolidBrush(box.BoxColor);

        //            // Draw text on image 
        //            thumbnailGraphic.FillRectangle(colorBrush, (int)x, (int)(y - size.Height - 1), (int)size.Width, (int)size.Height);
        //            thumbnailGraphic.DrawString(box.Description, drawFont, fontBrush, atPoint);

        //            // Draw bounding box on image
        //            thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
        //        }
        //    }
        //    return image;
        //}

        //private void DrawLabel(string label, Image<Rgba32> img, float x1, float y1)
        //{
        //    float fontSize = 12; // Adjust the font size if needed
        //    var font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", fontSize);

        //    var rendererOptions = new RendererOptions(font)
        //    {
        //        HorizontalAlignment= HorizontalAlignment.Left,
        //        VerticalAlignment = VerticalAlignment.Top,
        //    };
        //    var labelColor = SixLabors.ImageSharp.Color.White;
        //    img.Mutate(ctx => ctx.DrawText(label, font, labelColor, new SixLabors.ImageSharp.PointF(x1, y1 - fontSize)));
        //}

        //private List<int> NonMaximumSuppression(List<SixLabors.ImageSharp.RectangleF> boxes, float[] scores, float threshold)
        //{
        //    List<int> indices = Enumerable.Range(0, boxes.Count).ToList();
        //    indices.Sort((i, j) => scores[j].CompareTo(scores[i]));

        //    List<int> keep = new List<int>();

        //    while (indices.Count > 0)
        //    {
        //        int current = indices[0];
        //        keep.Add(current);
        //        indices.RemoveAt(0);

        //        List<int> removeIndices = new List<int>();
        //        for (int i = 0; i < indices.Count; i++)
        //        {
        //            int index = indices[i];
        //            float iou = ComputeIoU(boxes[current], boxes[index]);
        //            if (iou >= threshold)
        //                removeIndices.Add(i);
        //        }

        //        indices = indices.Where((_, i) => !removeIndices.Contains(i)).ToList();
        //    }

        //    return keep;
        //}

        //private float ComputeIoU(SixLabors.ImageSharp.RectangleF rect1, SixLabors.ImageSharp.RectangleF rect2)
        //{
        //    float xA = Math.Max(rect1.Left, rect2.Left);
        //    float yA = Math.Max(rect1.Top, rect2.Top);
        //    float xB = Math.Min(rect1.Right, rect2.Right);
        //    float yB = Math.Min(rect1.Bottom, rect2.Bottom);

        //    float interArea = Math.Max(0, xB - xA) * Math.Max(0, yB - yA);
        //    float unionArea = rect1.Width * rect1.Height + rect2.Width * rect2.Height - interArea;

        //    return interArea / unionArea;
        //}
    }
}
