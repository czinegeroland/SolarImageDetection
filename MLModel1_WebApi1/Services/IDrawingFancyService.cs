namespace MLModel1_WebApi1.Services
{
    public interface IDrawingFancyService
    {
        Task<byte[]> DrawRectangles(IFormFile file, List<MLModel1.ModelOutput.BoundingBox> boundingBoxes, float iouThreshold = 0.01F, float inputImageWidth = 500, float inputImageHeight = 500);
    }
}