// WARNING! This is just a simple HTTP Server for testing and is not final or optimized for large loads!

using System;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using VoidwyrmCoreAPI.core.logger;
using VoidwyrmCoreAPI.core.cogs;
using VoidwyrmCoreAPI.core.interfaces;
using VoidwyrmCoreAPI.core.events.models;
using VoidwyrmCoreAPI.core.events;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static VoidwyrmCoreAPI.core.logger.LogObject;

namespace Voidwyrm_Core.server
{
    using VoidwyrmCoreAPI.core;

    class HttpServer
    {
        private EventManager eventManager;

        public HttpServer(EventManager eventManager)
        {
            this.eventManager = eventManager;
        }
        public static HttpListener Listener;
        public static string Url = "http://localhost:4951/";
        public static string WelcomePageData =
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>Voidwyrm Core API</title>" +
            "  </head>" +
            "  <body>" +
            "  <pre style=\"color:#ff1111;\">" +
            "   <>=======() " +
            @"<br>  (/\___   /|\\          ()==========<>_     __      __   _     _                               " +
            @"<br>        \_/ | \\        //|\   ______/ \)    \ \    / /  (_)   | |                              " +
            @"<br>         \_|  \\      // | \_/                \ \  / /__  _  __| |_      ___   _ _ __ _ __ ___  " +
            @"<br>          \|\/|\_   //  /\/                    \ \/ / _ \| |/ _` \ \ /\ / / | | | '__| '_ ` _ \ " +
            @"<br>           (oo)\ \_//  /                        \  / (_) | | (_| |\ V  V /| |_| | |  | | | | | |" +
            @"<br>          //_/\_\/ /  |                          \/ \___/|_|\__,_| \_/\_/  \__, |_|  |_| |_| |_|" +
            @"<br>         @@/  |=\  \  |                                                     __/ |               " +
            @"<br>              \_=\_ \ |                                                    |___/                " +
            @"<br>               \==\ \|\_                                                                        " +
            @"<br>             __(\===\(  )\                   Voidwyrm Core API v1.0.2 (Codename: RedDragon)     " +
            @"<br>            (((~) __(_/   |                  ARK API was no dragon. Fire cannot kill a dragon.  " +
            @"<br>                 (((~) \  /" +
            @"<br>                 ______/ /" +
            @"<br>                 '------'" +
            "  </pre>" +
            "    <p><strong>Voidwyrm Server API</strong><br>This API is work in progress and should not be used in a production environment!</p>" +
            "  </body>" +
            "</html>";

        public async Task HandleIncomingConnections()
        {
            while (true)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await Listener.GetContextAsync();
                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                //VoidLogger.Log(LogType.Info, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, $"[{req.HttpMethod}] - {req.Url.ToString()}");

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/api/overrides"))
                {
                    
                    //VoidLogger.Log(LogType.Warn, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, "This path is not implemented (OVERRIDES).");
                }

                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/api/configs"))
                {
                    //VoidLogger.Log(LogType.Warn, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, "This path is not implemented.");
                }

                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath.StartsWith("/api/events")))
                {
                    EventRouter eventRouter = new EventRouter(eventManager);


                    //Get Event Data from HTTP
                    var eventData = new StreamReader(req.InputStream).ReadToEnd();

                    CoreEvent json  = JsonConvert.DeserializeObject<CoreEvent>(eventData);

                    //Determine if EventType is Custom and Route Event
                    if (json != null)
                    {
                        switch (json.DataType)
                        {
                            case "CustomCommand":
                            case "CustomEvent":
                            case "CustomOverride":
                                eventRouter.RouteCustomEvent(json);
                                break;
                            case "Configuration":
                            case "Event":
                            case "Override":
                                eventRouter.RouteCoreEvent(json);
                                break;
                            default:
                                VoidLogger.Log(LogType.Error, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, "The DataType is invalid, please check the running events if they match the required type.");
                                break;
                        }
                    }

                    VoidLogger.Log(LogType.Warn, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, eventData);
               
                }

                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/api/ping"))
                {
                    //VoidLogger.Log(LogType.Warn, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, "This path is not implemented (PING).");
                }


                // Write the response info
                byte[] data = Encoding.UTF8.GetBytes(String.Format(WelcomePageData));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }

        public void StartServer()
        {
            VoidLogger.Log(LogObject.LogType.Info, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, $"Enabled SuchSpeedHTTP.");
            VoidLogger.Log(LogObject.LogType.Info, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, $"Listening for connections at 4951");
            //VoidLogger.Log(LogType.Info, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, "Listening for connections.");
            // Create a Http server and start listening for incoming connections
            Listener = new HttpListener();
            Listener.Prefixes.Add(Url);
            Listener.Start();

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();
        }
    }
}