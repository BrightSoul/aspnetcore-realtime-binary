using System;
using System.Threading.Tasks;

namespace AspNetCoreRealtimeBinary.Hubs
{
    public interface IImageStreamClient
    {
        Task ReceiveImage(byte[] image);
        Task NotifyStatusChange(string newStatus);
    }
}