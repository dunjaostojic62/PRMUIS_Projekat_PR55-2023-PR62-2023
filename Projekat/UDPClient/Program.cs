using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class Program
    {
        private const int SERVER_PORT = 50001;
        private const string SERVER_IP = "127.0.0.1";
        private const int BUFFER_SIZE = 2048;

        static void Main(string[] args)
        {
            Console.WriteLine("1 - TCP");
            Console.WriteLine("2 - UDP");
            string izbor = Console.ReadLine();

            if (izbor == "2")
            {
                // UDP SLANJE (vežbe)
                Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27015);

                string messageUdp = "The Cheese is in The Toaster";
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageUdp);

                try
                {
                    int bytesSentUdp = sendSocket.SendTo(messageBytes, 0, messageBytes.Length,
                        SocketFlags.None, recvEndPoint);

                    Console.WriteLine("Sent {0} bytes to {1}", bytesSentUdp, recvEndPoint);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("sendto failed with error: {0}", ex.Message);
                }

                sendSocket.Close();
                Console.ReadKey();
                return;
            }


            // 1) Kreiranje utičnice:
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 2) Endpoint servera:
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT);

            // 3) Connect:
            clientSocket.Connect(serverEndPoint);

            Console.WriteLine("Povezan na server!");

            Console.Write("Unesi broj stola: ");
            int brojStola = int.Parse(Console.ReadLine());

            Console.Write("Unesi broj gostiju: ");
            int brojGostiju = int.Parse(Console.ReadLine());

            string messageTcp = brojStola + "|" + brojGostiju;

            int bytesSentTcp = clientSocket.Send(Encoding.UTF8.GetBytes(messageTcp));
            Console.WriteLine("Sent {0} bytes", bytesSentTcp);


            // Čekamo potvrdu servera
            byte[] buffer = new byte[BUFFER_SIZE];
            int bytesReceived = clientSocket.Receive(buffer);
            Console.WriteLine("Received: {0}", Encoding.UTF8.GetString(buffer, 0, bytesReceived));

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();

            Console.ReadKey();
        }
    }
}
