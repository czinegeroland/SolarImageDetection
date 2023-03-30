using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Drawing;

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

            // Load the image using ImageSharp
            ms2.Position = 0;
            using var img = SixLabors.ImageSharp.Image.Load<Rgba32>(ms2);


            List<SixLabors.ImageSharp.RectangleF> boxList = new List<SixLabors.ImageSharp.RectangleF>();

            // Draw bounding boxes on the image
            for (int i = 0; i < result.Scores.Length; i++)
            {
                //if (result.Scores[i] < 0.87)
                //    continue;

                // You can customize the appearance of the bounding box
                //var pen = new SixLabors.ImageSharp.Drawing.Processing.Pen(SixLabors.ImageSharp.Color.Green, 1);
                //var pen = new SixLabors.ImageSharp.Drawing.Processing.Pen(SixLabors.ImageSharp.Color.FromRgba(0, 255, 0, 128), 2);

                var x1 = result.Boxes[i * 4];
                var y1 = result.Boxes[i * 4 + 1];
                var x2 = result.Boxes[i * 4 + 2];
                var y2 = result.Boxes[i * 4 + 3];

                var rectangle = new SixLabors.ImageSharp.RectangleF(x1, y1, x2 - x1, y2 - y1);
                boxList.Add(rectangle);

                // Draw the rectangle on the image
                //img.Mutate(ctx => ctx.Draw(new DrawingOptions { GraphicsOptions = new GraphicsOptions() }, pen, rectangle));
                //img.Mutate(ctx => ctx.Fill(new DrawingOptions { GraphicsOptions = new GraphicsOptions() }, new SolidBrush(SixLabors.ImageSharp.Color.FromRgba(0, 255, 0, 12)), rectangle));
                //DrawLabel(Math.Round(result.Scores[0], 2).ToString(), img, x1, y1);
            }


            float nmsThreshold = 0.5f; // You can adjust this value based on your needs
            List<int> nmsIndices = NonMaximumSuppression(boxList, result.Scores, nmsThreshold);

            var pen = new SixLabors.ImageSharp.Drawing.Processing.Pen(SixLabors.ImageSharp.Color.FromRgba(0, 255, 0, 128), 3);

            foreach (int i in nmsIndices)
            {

                img.Mutate(ctx => ctx.Draw(new DrawingOptions { GraphicsOptions = new GraphicsOptions() }, pen, boxList[i]));
                img.Mutate(ctx => ctx.Fill(new DrawingOptions { GraphicsOptions = new GraphicsOptions() }, new SixLabors.ImageSharp.Drawing.Processing.SolidBrush(SixLabors.ImageSharp.Color.FromRgba(0, 255, 0, 12)), boxList[i]));
                DrawLabel(Math.Round(result.Scores[i], 2).ToString(), img, boxList[i].X, boxList[i].Y);
            }

            // Save the image with bounding boxes to a MemoryStream
            var outputStream = new MemoryStream();
            img.Save(outputStream, new JpegEncoder());
            outputStream.Position = 0;

            // Return the modified image
            return File(outputStream.ToArray(), "image/jpeg");
        }

        private void DrawLabel(string label, Image<Rgba32> img, float x1, float y1)
        {
            float fontSize = 12; // Adjust the font size if needed
            var font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", fontSize);
          
            var rendererOptions = new RendererOptions(font)
            {
                HorizontalAlignment= HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            var labelColor = SixLabors.ImageSharp.Color.White;
            img.Mutate(ctx => ctx.DrawText(label, font, labelColor, new SixLabors.ImageSharp.PointF(x1, y1 - fontSize)));
        }

        private List<int> NonMaximumSuppression(List<SixLabors.ImageSharp.RectangleF> boxes, float[] scores, float threshold)
        {
            List<int> indices = Enumerable.Range(0, boxes.Count).ToList();
            indices.Sort((i, j) => scores[j].CompareTo(scores[i]));

            List<int> keep = new List<int>();

            while (indices.Count > 0)
            {
                int current = indices[0];
                keep.Add(current);
                indices.RemoveAt(0);

                List<int> removeIndices = new List<int>();
                for (int i = 0; i < indices.Count; i++)
                {
                    int index = indices[i];
                    float iou = ComputeIoU(boxes[current], boxes[index]);
                    if (iou >= threshold)
                        removeIndices.Add(i);
                }

                indices = indices.Where((_, i) => !removeIndices.Contains(i)).ToList();
            }

            return keep;
        }

        private float ComputeIoU(SixLabors.ImageSharp.RectangleF rect1, SixLabors.ImageSharp.RectangleF rect2)
        {
            float xA = Math.Max(rect1.Left, rect2.Left);
            float yA = Math.Max(rect1.Top, rect2.Top);
            float xB = Math.Min(rect1.Right, rect2.Right);
            float yB = Math.Min(rect1.Bottom, rect2.Bottom);

            float interArea = Math.Max(0, xB - xA) * Math.Max(0, yB - yA);
            float unionArea = rect1.Width * rect1.Height + rect2.Width * rect2.Height - interArea;

            return interArea / unionArea;
        }
    }
}
