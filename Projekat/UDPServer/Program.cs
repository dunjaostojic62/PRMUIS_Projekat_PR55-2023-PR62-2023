using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;


namespace Server
{
    internal class Program
    {
        private const int SERVER_PORT = 50001;
        private const int BUFFER_SIZE = 2048;
        private static List<string> stanjeStolova = new List<string>();
        private static List<string> listaPorudzbina = new List<string>();
        private static List<string> informacijeORresursima = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("1 - TCP");
            Console.WriteLine("2 - UDP");
            string izbor = Console.ReadLine();

            if (izbor == "2")
            {
                // UDP PRIJEM (vežbe)
                Socket recvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Any, 27015);
                recvSocket.Bind(recvEndPoint);

                byte[] recvBuf = new byte[1024];
                EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    int bytesReceivedUdp = recvSocket.ReceiveFrom(recvBuf, ref senderEndPoint);
                    string receivedMessage = Encoding.UTF8.GetString(recvBuf, 0, bytesReceivedUdp);

                    Console.WriteLine("Received {0} bytes from {1}: {2}", bytesReceivedUdp,
                        senderEndPoint, receivedMessage);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("recvfrom failed with error: {0}", ex.Message);
                }

                recvSocket.Close();
                Console.ReadKey();
                return;
            }


            // TCP SERVER
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, SERVER_PORT);
            serverSocket.Bind(localEndPoint);

            serverSocket.Listen(10);
            Console.WriteLine("Server je pokrenut. Ceka konekciju...");

            Socket acceptedSocket = serverSocket.Accept();
            Console.WriteLine("Klijent je povezan!");

            byte[] buffer = new byte[BUFFER_SIZE];
            int bytesReceivedTcp = acceptedSocket.Receive(buffer);

            string received = Encoding.UTF8.GetString(buffer, 0, bytesReceivedTcp);
            Console.WriteLine("Received: {0}", received);

            string[] delovi = received.Split('|');
            if (delovi.Length == 2)
            {
                Console.WriteLine("Broj stola: {0}", delovi[0]);
                Console.WriteLine("Broj gostiju: {0}", delovi[1]);
            }

            stanjeStolova.Add(received);

            // prikaz da se vidi da "cuva stanje"
            Console.WriteLine("Stanje stolova (server pamti):");
            for (int i = 0; i < stanjeStolova.Count; i++)
            {
                Console.WriteLine(stanjeStolova[i]);
            }


            acceptedSocket.Send(Encoding.UTF8.GetBytes("OK"));

            acceptedSocket.Shutdown(SocketShutdown.Both);
            acceptedSocket.Close();
            serverSocket.Close();

            Console.ReadKey();
        }
    }
}
