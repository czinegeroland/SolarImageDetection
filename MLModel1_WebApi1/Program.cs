﻿// This file was auto-generated by ML.NET Model Builder. 
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.OpenApi.Models;
using Microsoft.ML.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;

// Configure app
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput>()
    .FromFile("MLModel1.mlnet");

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Description = "Docs for my API", Version = "v1" });
});
var app = builder.Build();

app.UseRouting();
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

// Define prediction route & handler
app.MapPost("/predict",
    async (PredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput> predictionEnginePool, string imagePath) =>
    {
        var image = MLImage.CreateFromFile(imagePath);
        var input = new MLModel1.ModelInput()
        {
            ImageSource = image,
        };

        return await Task.FromResult(predictionEnginePool.Predict(input));
    });

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});
// Run app
app.Run();
