using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreRealtimeBinary.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

namespace AspNetCoreRealtimeBinary
{
    public class ImageGenerator : IHostedService
    {
        private readonly IHubContext<ImageStreamHub> hub;
        private readonly CancellationTokenSource tokenSource;
        public ImageGenerator(IHubContext<ImageStreamHub> hub)
        {
            this.hub = hub;
            this.tokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(GenerateImages, tokenSource.Token);
            return Task.CompletedTask;
        }

        private async Task GenerateImages()
        {
            while(!tokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(1000);
                byte[] image = GenerateImage();
                await hub.Clients.All.SendAsync("ReceiveImage", image);
            }
        }

        private byte[] GenerateImage()
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
            return Task.CompletedTask;
        }
    }
}