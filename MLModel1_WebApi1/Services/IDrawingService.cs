using static MLModel1.ModelOutput;

namespace MLModel1_WebApi1.Services
{
    public interface IDrawingService
    {
        Task<byte[]> DrawRectangles(IFormFile file, List<BoundingBox> boundingBoxes, float iouThreshold = 0.01f, float inputImageWidth = 500f, float inputImageHeight = 500f);
    }
}