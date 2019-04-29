using System.Threading.Tasks;

namespace AspNetCoreRealtimeBinary.HostedServices
{
    public interface IImageGenerator
    {
        Task StartStreaming();
        Task StopStreaming();
        string GetCurrentStatus();
    }
}