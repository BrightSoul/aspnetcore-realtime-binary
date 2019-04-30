using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRealtimeBinary.Models.Services.Infrastructure
{
    public interface ITaskStartStop
    {
        (bool Success, string CurrentStatus) Start();

        (bool Success, string CurrentStatus) Stop();
        
        string GetCurrentStatus();

        Task<bool> ShouldExecute(CancellationToken token);

        event EventHandler<string> StatusChanged;

    }
}