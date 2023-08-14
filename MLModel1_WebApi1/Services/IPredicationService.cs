using Microsoft.ML.Data;

namespace MLModel1_WebApi1.Services
{
    public interface IPredicationService
    {
        Task<Tuple<MLModel1.ModelOutput, MLImage>> Predict(IFormFile file);
    }
}