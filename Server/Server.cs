using StreamJsonRpc;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace JsonRpcServer
{
    class Server
    {
        private static int clientId = 0;
        private const int Port = 54321;

        static void Main(string[] args)
        {
            //StartListeningAsync().GetAwaiter().GetResult();
            _ = StartListeningAsync();
            Console.ReadLine();
        }

        static async Task StartListeningAsync()
        {
            Console.WriteLine("StartListeningAsync() starting listening.");

            // get the local end point
            var ipHostInfo = await Dns.GetHostEntryAsync(Dns.GetHostName());
            IPAddress ipAddress = null;

            // find first ipv4 address to bind to
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
            {
                if (ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ipHostInfo.AddressList[i];
                    break;
                }
            }

            if (ipAddress == null)
            {
                Console.WriteLine("Couldn't find IPV4 address.");
                return;
            }


            var localEndPoint = new IPEndPoint(ipAddress, Port);

            // main loop
            while (true)
            {
                try
                {
                    using (var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                    {
                        // bind the socket to the local endpoint and listen for incoming connections  
                        Console.WriteLine($"Binding to: {localEndPoint.Address}");
                        listener.Bind(localEndPoint);
                        listener.Listen(10);

                        Console.WriteLine($"Waiting for a connection on port: {Port}");
                        Socket handler = await listener.AcceptAsync();
                        clientId++;

                        Console.WriteLine($"Accepted connection from: {handler.RemoteEndPoint}");

                        using (var stream = new NetworkStream(handler, true))
                        {
                            var jsonRpc = JsonRpc.Attach(stream, new Server());
                            Console.WriteLine($"Attached to client: {clientId}, waiting for JSON-RPC request.");
                            await jsonRpc.Completion;
                            Console.WriteLine("JSON-RPC request completed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("StartListening() exception occurred: " + ex.Message);
                }
            }
        }

        public int Add(int i1, int i2)
        {
            return i1 + i2;
        }
    }
}
