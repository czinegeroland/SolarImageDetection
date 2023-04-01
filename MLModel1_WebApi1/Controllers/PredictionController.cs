using Microsoft.AspNetCore.Mvc;
using MLModel1_WebApi1.Services;

namespace MLModel1_WebApi1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PredictionController : ControllerBase
    {
        private readonly IPredicationService _predicationService;
        private readonly IDrawingService _drawingService;
        private readonly IDrawingFancyService _drawingFancyService;

        public PredictionController(
            IPredicationService predicationService, 
            IDrawingService drawingService,
            IDrawingFancyService drawingFancyService
            )
        {
            _predicationService = predicationService;
            _drawingService = drawingService;
            _drawingFancyService = drawingFancyService;
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

        [HttpPost("predict-with-image-result-fancy")]
        public async Task<IActionResult> PredictWithImageFancyResult(IFormFile file)
        {
            var predicationResult = await _predicationService.Predict(file);

            var imageBytes = await _drawingFancyService.DrawRectangles(file, predicationResult.Item1.BoundingBoxes.ToList(), 0.01f, predicationResult.Item2.Width, predicationResult.Item2.Height);

            return File(imageBytes, "image/png");
        }
    }
}
