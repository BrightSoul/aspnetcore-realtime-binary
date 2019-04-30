using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreRealtimeBinary.HostedServices;
using AspNetCoreRealtimeBinary.Models.Services.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCoreRealtimeBinary.Hubs
{
    public class ImageStreamHub : Hub<IImageStreamClient>
    {
        private readonly ITaskStartStop taskStartStop;

        public ImageStreamHub(ITaskStartStop taskStartStop)
        {
            this.taskStartStop = taskStartStop;
            this.taskStartStop.StatusChanged += NotifyClients;
        }

        public void Start()
        {
            taskStartStop.Start();
        }

        public void Stop()
        {
            taskStartStop.Stop();
        }

        private static int connectedClientCount = 0;
        public override async Task OnConnectedAsync()
        {
            Interlocked.Increment(ref connectedClientCount);
            await Clients.Caller.NotifyStatusChange(taskStartStop.GetCurrentStatus());
        }

        public override Task OnDisconnectedAsync(Exception exc)
        {
            int newValue = Interlocked.Decrement(ref connectedClientCount);
            if (newValue == 0)
            {
                //Stop automatico se non ci sono più client in ascolto
                Stop();
            }
            return Task.CompletedTask;
        }

        private async void NotifyClients(object sender, string newStatus)
        {
            await Clients.All.NotifyStatusChange(newStatus);
        }

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                this.taskStartStop.StatusChanged -= NotifyClients;
                disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}