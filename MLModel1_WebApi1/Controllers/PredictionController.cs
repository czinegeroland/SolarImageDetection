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
