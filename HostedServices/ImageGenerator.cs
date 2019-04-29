using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreRealtimeBinary.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

namespace AspNetCoreRealtimeBinary.HostedServices
{
    public class ImageGenerator : BackgroundService, IImageGenerator
    {
        private readonly IHubContext<ImageStreamHub, IImageStreamClient> hubContext;
        private readonly ManualResetEventSlim manualResetEvent;
        private const string STATUS_STREAMING = "streaming";
        private const string STATUS_IDLE = "idle";
        public ImageGenerator(IHubContext<ImageStreamHub, IImageStreamClient> hubContext)
        {
            this.hubContext = hubContext;
            this.manualResetEvent = new ManualResetEventSlim(false);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                manualResetEvent.Wait(stoppingToken);
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

        private string currentStatus = STATUS_IDLE;
        public async Task StartStreaming()
        {
            string previousValue = Interlocked.CompareExchange(ref currentStatus, STATUS_STREAMING, STATUS_IDLE);
            if (previousValue == STATUS_IDLE)
            {
                manualResetEvent.Set();
                await hubContext.Clients.All.NotifyStatusChange(STATUS_STREAMING);
            }
        }

        public async Task StopStreaming()
        {
            string previousValue = Interlocked.CompareExchange(ref currentStatus, STATUS_IDLE, STATUS_STREAMING);
            if (previousValue == STATUS_STREAMING)
            {
                manualResetEvent.Reset();
                await hubContext.Clients.All.NotifyStatusChange(STATUS_IDLE);
            }
        }

        public string GetCurrentStatus()
        {
            return Interlocked.CompareExchange(ref currentStatus, null, null);
        }
    }
}