using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCoreRealtimeBinary.Hubs
{
    public class ImageStreamHub : Hub<IImageStreamClient>
    {
        //In questa demo il client non invoca mai metodi nel server
        //quindi in questo Hub non viene definito alcun metodo
    }
}