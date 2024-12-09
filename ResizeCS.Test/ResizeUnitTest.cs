using SkiaSharp;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using Xunit.Abstractions;

namespace ResizeCS.Test
{
    public class ResizeUnitTest(ITestOutputHelper testOutputHelper)
    {
        static readonly Resize resize = new Resize();
        [Theory]
        [InlineData("input.png","output.png",3000,3000,1400,300)]
        [InlineData("input.png", "output.png", 3000, 3000, 1500, 1500)]
        [InlineData("input.png", "output.png", 3001, 3000, 1500, 1500)]
        [InlineData("input.png", "output.png", 3000, 3001, 1500, 1500)]
        [InlineData("input.png", "output.png", 3000, 3000, 1501, 1500)]
        [InlineData("input.png", "output.png", 3000, 3000, 1500, 1501)]
        [InlineData("input.png", "output.png", 1500, 1500, 3000, 3000)]
        [InlineData("input.png", "output.png", 1500, 1500, 3000, 3001)]
        [InlineData("input.png", "output.png", 1500, 1500, 3000, 1000)]
        [InlineData("input.png", "output.png", 1500, 1500, 1000, 3000)]


        public void Test1(string inputFileName,string outputFileName,int inputWidth,int inputHeight,int outputWidth,int outputHeight)
        {
            SHA256  sHA256 = SHA256.Create();
            string goalHash = "";
            string resultHash = "";
            string current = Directory.GetCurrentDirectory();

            string inputPath = Path.Combine(current, inputFileName);
            string outputPath = Path.Combine(current, outputFileName);

            double Ration = (double)inputWidth / (double)inputHeight;
            double desiredRatio = (double)outputWidth / (double)outputHeight;
            int originalSizeOutputWidth, originalSizeOutputHeight;
            if (desiredRatio < Ration)
            {
                originalSizeOutputHeight = inputHeight;
                originalSizeOutputWidth = (int)(inputHeight * desiredRatio);
            }
            else
            {
                originalSizeOutputHeight = (int)(inputWidth / desiredRatio);
                originalSizeOutputWidth = inputWidth;
            }
            var outputExtension = Path.GetExtension(outputFileName);
            var outputType = StringExtensionToSKEncodedImageFormat(outputExtension.ToLower());
            var inputExtension = Path.GetExtension(outputFileName);

            var inputType = StringExtensionToSKEncodedImageFormat(inputExtension.ToLower());

            using (SKBitmap originalSizeOutputBitmap = new SKBitmap(originalSizeOutputWidth, originalSizeOutputHeight))
            {
                using(SKCanvas canvas = new SKCanvas(originalSizeOutputBitmap))
                {
                    Random random = new Random();
                    SKPaint paint = new SKPaint();
                    paint.Style = SKPaintStyle.Fill;
                    paint.Color = new SKColor((byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255));
                    SKRect sKRect = new SKRect(1,1,originalSizeOutputWidth-2,originalSizeOutputHeight-2);
                    canvas.DrawRect(sKRect, paint);
                    sKRect = new SKRect(0, 0, random.Next(originalSizeOutputWidth), random.Next(originalSizeOutputHeight));
                    paint.Color = new SKColor((byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255));
                    canvas.DrawRect(sKRect, paint);


                }
                using(SKBitmap outputBitmap = originalSizeOutputBitmap.Resize(new SKImageInfo(outputWidth, outputHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)))
                {

                    var span = outputBitmap.Encode(outputType.Value, 100).AsSpan();
                    goalHash = Convert.ToBase64String(sHA256.ComputeHash(span.ToArray()));
                    using (Stream stream = outputBitmap.Encode(outputType.Value, 100).AsStream())
                    {
                        using(FileStream fs = new FileStream(Path.Combine(current,"result"+outputExtension),FileMode.OpenOrCreate))
                        {
                            fs.SetLength(0);
                            stream.CopyTo(fs);
                        }
                    }
                }
                using(SKBitmap inputBitmap = new SKBitmap(inputWidth, inputHeight))
                {
                    using(SKCanvas canvas = new SKCanvas(inputBitmap))
                    {
                        int left = (inputWidth - originalSizeOutputWidth) / 2;
                        int top = (inputHeight - originalSizeOutputHeight) / 2;

                        SKRect rect = new SKRect(left, top,originalSizeOutputWidth + left, originalSizeOutputHeight + top);
                        canvas.DrawBitmap(originalSizeOutputBitmap, new SKRect(0,0,originalSizeOutputWidth,originalSizeOutputHeight), rect);
                    }
                    using (Stream stream = inputBitmap.Encode(inputType.Value, 100).AsStream())
                    using(FileStream fs = new FileStream(inputPath, FileMode.OpenOrCreate))
                    {
                        stream.CopyTo(fs);
                    }
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    resize.Root(inputPath,outputPath,outputWidth,outputHeight);
                    stopwatch.Stop();
                    testOutputHelper.WriteLine($"time:{stopwatch.ElapsedMilliseconds}ms");
                    using (FileStream fs2 = new FileStream(outputPath,FileMode.OpenOrCreate))
                    {
                        resultHash = Convert.ToBase64String(sHA256.ComputeHash(fs2).AsSpan());
                    }
                    Assert.Equal(goalHash, resultHash);
                    if(resultHash != goalHash)
                    {
                        return;
                    }
                    File.Delete(inputPath);
                    File.Delete(outputPath);

                }

            }
            
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