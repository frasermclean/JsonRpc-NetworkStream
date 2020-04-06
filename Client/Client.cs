using StreamJsonRpc;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace JsonRpcClient
{
    class Client
    {
        private const int Port = 54321;

        static void Main(string[] args)
        {
            //RequestRpcAsync().GetAwaiter().GetResult();
            _ = RequestRpcAsync();
            Console.WriteLine("Main() request RPC task finished. Press enter to exit.");
            Console.ReadLine();
        }

        static async Task RequestRpcAsync()
        {
            // get the local end point
            var ipHostInfo = await Dns.GetHostEntryAsync(Dns.GetHostName());

            // find first ipv4 address to bind to
            IPAddress ipAddress = null;
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
            {
                if (ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ipHostInfo.AddressList[i];
                    Console.WriteLine("RequestRpcAsync() Using ip address: " + ipAddress);
                    break;
                }
            }

            // check if we didnt find an address
            if (ipAddress == null)
            {
                Console.WriteLine("Couldn't find IPV4 address.");
                return;
            }

            // Create a TCP/IP  socket.  
            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await sender.ConnectAsync(ipAddress, Port);
                Console.WriteLine("Connected to: " + sender.RemoteEndPoint);

                using (var stream = new NetworkStream(sender, true))
                {
                    var jsonRpc = JsonRpc.Attach(stream);

                    for (int i = 1; i <= 3; i++)
                    {
                        int sum = await jsonRpc.InvokeAsync<int>("Add", i, i + 1);
                        Console.WriteLine($"Sum: {i} + {i + 1} = {sum}");
                    }

                    Console.WriteLine("Terminating stream...");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: " + e.Message);
            }
        }
    }
}
