using System;
using System.Threading.Tasks;
using AspNetCoreRealtimeBinary.HostedServices;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCoreRealtimeBinary.Hubs
{
    public class ImageStreamHub : Hub<IImageStreamClient>
    {
        private readonly IImageGenerator imageGenerator;

        public ImageStreamHub(IImageGenerator imageGenerator)
        {
            this.imageGenerator = imageGenerator;
        }
        public async Task StartStreaming()
        {
            await imageGenerator.StartStreaming();
        }

        public async Task StopStreaming()
        {
            await imageGenerator.StartStreaming();
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.NotifyStatusChange(imageGenerator.GetCurrentStatus());
        }
    }
}