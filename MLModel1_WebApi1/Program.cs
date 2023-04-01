using Microsoft.Extensions.ML;
using MLModel1_WebApi1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput>()
    .FromFile("MLModel1.mlnet");

builder.Services.AddMvc();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddTransient<IPredicationService, PredicationService>();
builder.Services.AddTransient<IDrawingService, DrawingService>();
builder.Services.AddTransient<IDrawingFancyService, DrawingFancyService>();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Description = "Docs for my API", Version = "v1" });
//});
var app = builder.Build();

app.UseRouting();
//app.UseSwagger();

//app.UseSwaggerUI(c =>
//{
//    c.RoutePrefix = "";
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
//});

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapDefaultControllerRoute();
//});

//app.Use(async (context, next) =>
//{
//    if (context.Request.Path == "/" || context.Request.Path == "/index.html")
//    {
//        context.Request.Path = "/ObjectDetection";
//    }
//    await next.Invoke();
//});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
           name: "default",
           pattern: "{controller=ObjectDetection}/{action=Index}/{id?}");

    endpoints.MapControllers();

});

app.Run();
