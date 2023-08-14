using Microsoft.Extensions.ML;
using Microsoft.ML.Data;

namespace MLModel1_WebApi1.Services
{
    public class PredicationService : IPredicationService
    {
        private readonly PredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput> _predictionEnginePool;

        public PredicationService(
            PredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput> predictionEnginePool
            )
        {
            _predictionEnginePool = predictionEnginePool;
        }

        public async Task<Tuple<MLModel1.ModelOutput, MLImage>> Predict(IFormFile file)
        {
            var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            ms.Position = 0;

            var image = MLImage.CreateFromStream(ms);

            var input = new MLModel1.ModelInput()
            {
                ImageSource = image,
            };

            var result =_predictionEnginePool.Predict(input);

            return new Tuple<MLModel1.ModelOutput, MLImage>(result, image);
        }
    }
}
