# ASP.NET Core Realtime Binary demo
Questa è un'applicazione ASP.NET Core che dimostra come usare ASP.NET Core SignalR per fare push di contenuti binari dal server verso i client connessi.

## Requisiti
È necessario avere installato .NET Core SDK 2.2. L'applicazione è stata creata con Visual Studio Code ma può essere eseguita anche da Visual Studio 2017 o superiore.

## Panoramica
Nell'applicazione c'è un _hosted service_ che ogni secondo genera un'immagine PNG che reca l'ora corrente e un colore di sfondo casuale. L'immagine così generata viene inviata ai client connessi grazie a un _hub_ di ASP.NET Core SignalR, configurato per sfruttare il trasporto binario [MessagePack](https://msgpack.org/).

![demo.gif](demo.gif)

Questa demo prende spunto da questa pagina della documentazione ufficiale.
[https://docs.microsoft.com/it-it/aspnet/core/signalr/messagepackhubprotocol?view=aspnetcore-2.2](https://docs.microsoft.com/it-it/aspnet/core/signalr/messagepackhubprotocol?view=aspnetcore-2.2)

L'esecuzione di attività in background per mezzo di _hosted service_ è anche descritto qui:
[https://docs.microsoft.com/it-it/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2](https://docs.microsoft.com/it-it/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2)

## Guida passo-passo
 1. L'applicazione è stata creata con il comando `dotnet new mvc`. Sono state rimosse parti superflue ai fini della demo, tipo jQuery, la barra dei cookie e la pagina privacy;
 2. Aggiunto il pacchetto NuGet `Microsoft.AspNetCore.SignalR.Protocols.MessagePack` che si porta dietro come dipendenze anche i necessari pacchetti di SignalR;
 3. (Opzionale) Aggiunto il pacchetto NuGet `SkiaSharp` usato per generare l'immagine (nel vostro caso potrebbe non essere necessario generare l'immagine, ad esempio se viene prodotta da qualche altra fonte, come una webcam);
 4. Creata la classe [Hubs/ImageStreamHub.cs](Hubs/ImageStreamHub.cs) che è l'_hub_ di ASP.NET Core SignalR usato per inviare dati binari ai client;
 5. Creata anche l'interfaccia [Hubs/IImageStreamClient.cs](Hubs/IImageStreamClient.cs) che viene implementata dall'_hub_ in modo che si possa invocare la funzione JavaScript sul client in maniera fortemente tipizzata;
 6. Creato l'_hosted service_ [HostedServices/ImageGenerator.cs](HostedServices/ImageGenerator.cs) che in questa demo serve a generare un'immagine ogni secondo.
     > Un _hosted service_ è un componente che va in esecuzione all'avvio dell'applicazione ASP.NET Core e viene arrestato al termine, consentendo di eseguire logica in background e indipendentemente dalle richieste HTTP in ingresso.
    * L'_hosted service_ di questa demo deriva da `BackgroundService` che ci semplifica quando dobbiamo eseguire attività in background come in questo caso. Inoltre, nel suo costruttore riceve il servizio `IHubContext<ImageStreamHub, IImageStreamClient>` che rappresenta l'_hub_ di ASP.NET Core SignalR;
    * Il metodo `ExecuteAsync` dell'_hosted service_ viene richiamato automaticamente all'avvio dell'applicazione e resta in funzione finché l'applicazione è in esecuzione. Ogni secondo genera un'immagine grazie alla libreria `SkiaSharp` ([documentata qui da Microsoft](https://docs.microsoft.com/en-us/dotnet/api/skiasharp));
    * L'immagine così generata viene salvata in memoria in formato PNG e i suoi byte sono inviati al client grazie al suddetto servizio `IHubContext<ImageStreamHub, IImageStreamClient>`. 
 7. Nella view [Views/Home/Index.cshtml](Views/Home/Index.cshtml) è stato aggiunto del codice JavaScript:
    * Sono stati aggiunti i riferimenti a 3 librerie JavaScript (signalr, msgpack5, signalr-protocol-msgpack). Si è configurata la connessione all'hub usando MessagePack come tipo di trasporto per poter ricevere dati binari con ASP.NET Core SignalR in maniera ottimizzata;
    * Alla ricezione dei dati binari dell'immagine, che si manifestano nel client con un [oggetto Uint8Array](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Uint8Array), è stato usato un [oggetto FileReader](https://developer.mozilla.org/en-US/docs/Web/API/FileReader) per leggerli e ottenere un [data URL](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/Data_URIs);
    * Il _data URL_ viene assegnato all'attributo `src` di un tag `img`, in modo che l'immagine possa essere visualizzata all'utente.
 8. Infine, si sono registrati il middleware e i servizi dalla classe [Startup.cs](Startup.cs):
    * Dal metodo `Configure` si è aggiunto il middleware di ASP.NET Core SignalR, indicando il tipo e l'endpoint dell'_hub_. Inoltre, è stato indicato il peso massimo che l'immagine può avere dall'opzione `ApplicationMaxBufferSize`;
    * Dal metodo `ConfigureServices` si è aggiunto il servizio di ASP.NET Core SignalR, indicando MessagePack come trasporto;
    * Sempre dal metodo `ConfigureServices` è stato aggiunto l'_hosted service_ responsabile della generazione dell'immagine.