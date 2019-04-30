using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace AspNetCoreRealtimeBinary.Models.Services.Infrastructure
{
    public class TaskStartStop : ITaskStartStop
    {
        private string currentStatus = STOPPED;
        private const string STARTED = "started";
        private const string STOPPED = "stopped";
        private readonly AsyncManualResetEvent resetEvent = new AsyncManualResetEvent(false);

        public event EventHandler<string> StatusChanged;

        public (bool Success, string CurrentStatus) Start()
        {
            //Questo servizio TaskStartStop è singleton, quindi il cambiamento di stato deve avvenire in maniera thread-safe
            return TryChangingStatus(fromStatus: STOPPED, toStatus: STARTED, onSuccess: () => resetEvent.Set());
        }

        public (bool Success, string CurrentStatus) Stop()
        {
            //Questo servizio TaskStartStop è singleton, quindi il cambiamento di stato deve avvenire in maniera thread-safe
            return TryChangingStatus(fromStatus: STARTED, toStatus: STOPPED, onSuccess: () => resetEvent.Reset());
        }

        private (bool Success, string CurrentStatus) TryChangingStatus(string fromStatus, string toStatus, Action onSuccess = null)
        {
            //Usiamo la classe Interlocked e il suo metodo CompareExchange per garantire un cambio di stato thread-safe
            //Per info: https://www.codeproject.com/Articles/288150/Thread-Synchronization-with-Interlocked-Class
            string previousStatus = Interlocked.CompareExchange<string>(ref currentStatus, toStatus, fromStatus);
            if (previousStatus == fromStatus)
            {
                StatusChanged?.Invoke(this, toStatus);
                onSuccess?.Invoke();
                return (true, previousStatus);
            }
            return (false, previousStatus);
        }
        
        
        public string GetCurrentStatus()
        {
            //Questo è un trucco per leggere il valore di currentStatus in maniera thread-safe
            //Per info: https://stackoverflow.com/questions/24808291/reading-an-int-thats-updated-by-interlocked-on-other-threads
            return Interlocked.CompareExchange(ref currentStatus, null, null);
        }

        public async Task<bool> ShouldExecute(CancellationToken token) {
            //Should execute restituisce true se il task è in stato di Started
            //Altrimenti, se è in stato di Stopped, causerà un'attesa finché lo stato non torna a Started
            //Se invece il token viene cancellato, allora restituisce subito false ad indicare che il task deve essere arrestato
            await resetEvent.WaitAsync(token);
            return !token.IsCancellationRequested;
        }
    }
}