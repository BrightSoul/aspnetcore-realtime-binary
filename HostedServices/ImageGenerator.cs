using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreRealtimeBinary.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

namespace AspNetCoreRealtimeBinary.HostedServices
{
    public class ImageGenerator : IHostedService
    {
        private readonly IHubContext<ImageStreamHub, IImageStreamClient> hubContext;
        private readonly CancellationTokenSource tokenSource;
        private Task imageGenerationTask;
        public ImageGenerator(IHubContext<ImageStreamHub, IImageStreamClient> hubContext)
        {
            this.hubContext = hubContext;
            this.tokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            imageGenerationTask = GenerateImages(hubContext, tokenSource.Token);
            return Task.CompletedTask;
        }

        private static async Task GenerateImages(IHubContext<ImageStreamHub, IImageStreamClient> hubContext, CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                byte[] imageData = GenerateImage();
                await hubContext.Clients.All.ReceiveImage(imageData);
                await Task.Delay(1000, cancellationToken);
            }
        }

        private static byte[] GenerateImage()
        {
            using (SKBitmap bitmap = new SKBitmap(width: 200, height: 200))
            {
                using (SKCanvas canvas = new SKCanvas(bitmap))
                {
                    var random = new Random();
                    canvas.DrawColor(SKColor.FromHsl(random.Next(0, 360), 100, 50));
                    using (var paint = new SKPaint()
                    {
                        TextSize = 25.0f,
                        IsAntialias = true,
                        Color = new SKColor(0, 0, 0),
                        Style = SKPaintStyle.Fill,
                        TextAlign = SKTextAlign.Center
                    })
                    {
                        canvas.DrawText(DateTime.Now.ToString("T"), 100, 100, paint);
                    }
                    using (SKData data = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100))
                    {
                        return data.ToArray();
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            tokenSource.Cancel();
            return imageGenerationTask ?? Task.CompletedTask;
        }
    }
}