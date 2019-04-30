using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreRealtimeBinary.Hubs;
using AspNetCoreRealtimeBinary.Models.Services.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

namespace AspNetCoreRealtimeBinary.HostedServices
{
    public class ImageGenerator : BackgroundService
    {
        private readonly IHubContext<ImageStreamHub, IImageStreamClient> hubContext;
        private readonly ITaskStartStop taskStartStop;

        public ImageGenerator(IHubContext<ImageStreamHub, IImageStreamClient> hubContext, ITaskStartStop taskStartStop)
        {
            this.hubContext = hubContext;
            this.taskStartStop = taskStartStop;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(await taskStartStop.ShouldExecute(stoppingToken))
            {
                byte[] imageData = GenerateImage();
                await hubContext.Clients.All.ReceiveImage(imageData);
                await Task.Delay(1000, stoppingToken);
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
    }
}