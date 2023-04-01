using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using MLModel1_WebApi1.Services;
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
        private readonly IPredicationService _predicationService;
        private readonly IDrawingService _drawingService;

        public PredictionController(IPredicationService predicationService, IDrawingService drawingService)
        {
            _predicationService = predicationService;
            _drawingService = drawingService;
        }

        //[HttpPost("predict-image")]
        //public async Task<IActionResult> Predict([FromServices] PredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput> predictionEnginePool, IFormFile file)
        //{
        //    var ms = new MemoryStream();
        //    await file.CopyToAsync(ms);

        //    ms.Position = 0;

        //    var image = MLImage.CreateFromStream(ms);
        //    var input = new MLModel1.ModelInput()
        //    {
        //        ImageSource = image,
        //    };

        //    var result = predictionEnginePool.Predict(input);

        //    return Ok(result);
        //}

        [HttpPost("predict-with-image-result")]
        public async Task<IActionResult> PredictWithImageResult(IFormFile file)
        {
            var predicationResult = await _predicationService.Predict(file);

            var imageBytes = await _drawingService.DrawRectangles(file, predicationResult.Item1.BoundingBoxes.ToList(), 0.01f, predicationResult.Item2.Width, predicationResult.Item2.Height);
          
            return File(imageBytes, "image/jpeg");
        }
    }
}
