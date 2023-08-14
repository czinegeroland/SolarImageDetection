using Microsoft.Extensions.ML;
using MLModel1_WebApi1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput>()
    .FromFile("MLModel1.mlnet");

builder.Services.AddMvc();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IPredicationService, PredicationService>();
builder.Services.AddTransient<IDrawingService, DrawingService>();
builder.Services.AddTransient<IDrawingFancyService, DrawingFancyService>();

var app = builder.Build();

app.UseRouting();
app.UseSwagger();

app.UseSwaggerUI();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
           name: "default",
           pattern: "{controller=ObjectDetection}/{action=Index}/{id?}");

    endpoints.MapControllers();

});

app.Run();
