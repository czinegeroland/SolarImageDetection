﻿// This file was auto-generated by ML.NET Model Builder.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Transforms.Image;
using Microsoft.ML;

namespace SolarPowerplantSateliteImageDetection
{
    public partial class MLModel1
    {



        /// <summary>
        /// Retrains model using the pipeline generated as part of the training process.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="trainData"></param>
        /// <returns></returns>
        public static ITransformer RetrainModel(MLContext mlContext, IDataView trainData)
        {
            var pipeline = BuildPipeline(mlContext);
            var model = pipeline.Fit(trainData);

            return model;
        }


        /// <summary>
        /// build the pipeline that is used from model builder. Use this function to retrain model.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
        {
            // Data process configuration with pipeline data transformations
            var pipeline = mlContext.Transforms.ResizeImages(outputColumnName:@"input1",inputColumnName:@"input1",imageHeight:600,imageWidth:800,cropAnchor:ImageResizingEstimator.Anchor.Center,resizing:ImageResizingEstimator.ResizingKind.Fill)      
                                    .Append(mlContext.Transforms.ExtractPixels(outputColumnName:@"input1",inputColumnName:@"input1",colorsToExtract:ImagePixelExtractingEstimator.ColorBits.Rgb,orderOfExtraction:ImagePixelExtractingEstimator.ColorsOrder.ARGB,outputAsFloatArray:true))      
                                    .Append(mlContext.Transforms.ApplyOnnxModel(modelFile:@"C:\Users\Home-Asus\source\repos\SolarPowerplantSateliteImageDetection\SolarPowerplantSateliteImageDetection\MLModel1.onnx",fallbackToCpu:true));

            return pipeline;
        }
    }
 }
