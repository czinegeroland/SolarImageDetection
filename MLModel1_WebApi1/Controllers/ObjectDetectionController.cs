using Microsoft.AspNetCore.Mvc;
using MLModel1_WebApi1.Services;

namespace MLModel1_WebApi1.Controllers
{
    public class ObjectDetectionController : Controller
    {
        private readonly IPredicationService _predicationService;
        private readonly IDrawingFancyService _drawingFancyService;

        public ObjectDetectionController(
            IPredicationService predicationService,
            IDrawingFancyService drawingFancyService
            )
        {
            _predicationService = predicationService;
            _drawingFancyService = drawingFancyService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a file.");
                return View("Index");
            }

            var predicationResult = await _predicationService.Predict(file);
            var imageBytes = await _drawingFancyService.DrawRectangles(file, predicationResult.Item1.BoundingBoxes.ToList(), 0.01f, predicationResult.Item2.Width, predicationResult.Item2.Height);
            
            var imageUrl = $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
            
            ViewBag.ImageUrl = imageUrl;
           
            return View("Index");
        }
    }
}
