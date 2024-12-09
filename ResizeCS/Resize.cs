using ConsoleAppFramework;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResizeCS
{
    public class Resize
    {
        /// <summary>
        /// resize
        /// </summary>
        /// <param name="inputPath">-i,Specifies the input path for the image.</param>
        /// <param name="outputPath">-o,Specify the output path for the image.</param>
        /// <param name="width">-w,The output image height size (px).</param>
        /// <param name="height">-h,The output image height size (px).</param>
        [Command("")]
        public void Root(string inputPath,string outputPath,int width = 400,int height = 400)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine("File does not exist.");
                return;
            }
            var extensions = Path.GetExtension(outputPath).ToLower();
            SKEncodedImageFormat? sKEncoded = StringExtensionToSKEncodedImageFormat(extensions);

            if (!sKEncoded.HasValue)
            {
                Console.Error.WriteLine($"The output format \"{extensions}\" is not supported.");
                return;
            }
            double desiredRatio = (double)width / (double)height;
            using (SKBitmap sKBitmap = SKBitmap.Decode(inputPath))
            {
                double Ration = (double)sKBitmap.Width / (double)sKBitmap.Height;
                SKBitmap outputBitmap; 
                if(desiredRatio !=  Ration)
                {
                    using (SKBitmap trimedBitmap = Trim(sKBitmap, desiredRatio))
                    {
                        outputBitmap=SameAspectResize(trimedBitmap,width,height);
                    }
                }
                else
                {
                    outputBitmap = SameAspectResize(sKBitmap, width, height);
                }
                using (outputBitmap)
                {
                    using (var stream = outputBitmap.Encode(sKEncoded.Value, 100).AsStream())
                    {
                        if (File.Exists(outputPath))
                        {
                            File.Delete(outputPath);
                        }
                        using (FileStream fs = new FileStream(outputPath, FileMode.CreateNew))
                        {
                            stream.CopyTo(fs);

                        }
                    }
                }
                
                Console.WriteLine($"output:{outputPath}");
                

            }
            stopwatch.Stop();
            Console.WriteLine($"time:{stopwatch.ElapsedMilliseconds}ms");

        }

        SKBitmap Trim(SKBitmap sKBitmap,double desiredRatio)
        {
            double Ration = (double)sKBitmap.Width / (double)sKBitmap.Height;
            int cutHeight, cutWidth;

            if (desiredRatio < Ration)
            {
                cutHeight = sKBitmap.Height;
                cutWidth = (int)(sKBitmap.Height * desiredRatio);
            }
            else
            {
                cutHeight = (int)(sKBitmap.Width / desiredRatio);
                cutWidth = sKBitmap.Width;
            }
            SKBitmap cutBitmap = new SKBitmap(cutWidth, cutHeight);
            using (SKCanvas canvas = new SKCanvas(cutBitmap))
            {
                int left = (sKBitmap.Width - cutWidth) / 2;
                int top = (sKBitmap.Height - cutHeight) / 2;

                SKRect sKRect = new SKRect(left, top, left + cutWidth, top + cutHeight);
                SKRect destRect = new SKRect(0, 0, cutWidth, cutHeight);
                canvas.DrawBitmap(sKBitmap, sKRect, destRect);
            }
            return cutBitmap;
            
        }

        SKBitmap SameAspectResize(SKBitmap sKBitmap, int outWidth,int outHeight)
        {
            return sKBitmap.Resize(new SKImageInfo(outWidth,outHeight),new  SKSamplingOptions(SKFilterMode.Linear,SKMipmapMode.Linear)) ;
        }
        SKEncodedImageFormat? StringExtensionToSKEncodedImageFormat(string extensionStr)
        {
            extensionStr = extensionStr.ToLower();
            SKEncodedImageFormat? sKEncodedImageFormat;
            switch (extensionStr)
            {
                case ".jpg":
                case ".jpeg":
                    sKEncodedImageFormat = SKEncodedImageFormat.Jpeg;
                    break;
                case ".png":
                    sKEncodedImageFormat = SKEncodedImageFormat.Png;
                    break;
                case ".bmp":
                    sKEncodedImageFormat = SKEncodedImageFormat.Bmp;
                    break;
                case ".gif":
                    sKEncodedImageFormat = SKEncodedImageFormat.Gif;
                    break;
                default:
                    sKEncodedImageFormat = null;
                    break;
            }

            return sKEncodedImageFormat;
        }

    }
}
