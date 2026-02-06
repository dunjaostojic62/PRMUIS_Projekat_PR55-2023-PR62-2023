using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using Common;


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
            Console.WriteLine("3 - Zadatak 5 (Konobar)");
            string izbor = Console.ReadLine();

            if (izbor == "2")
            {
                // UDP SLANJE 
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
            if (izbor == "3")
            {
                Console.WriteLine("KONOBAR (Zadatak 5) povezan na server.");

                // prijava uloge
                clientSocket.Send(Encoding.UTF8.GetBytes("ULOGA|KONOBAR"));

                byte[] okBuf1 = new byte[BUFFER_SIZE];
                int okBr1 = clientSocket.Receive(okBuf1);
                Console.WriteLine(Encoding.UTF8.GetString(okBuf1, 0, okBr1));

                // unos stola
                Console.Write("Unesi broj stola: ");
                int brojStolaZ5 = int.Parse(Console.ReadLine());

                // 5 porudzbina - STRING
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("Unos porudzbine #{0}", i + 1);

                    Console.Write("Unesi naziv artikla: ");
                    string naziv = Console.ReadLine();

                    Console.Write("Unesi cenu: ");
                    string cena = Console.ReadLine();

                    Console.WriteLine("Unesi kategoriju: 1 - Hrana, 2 - Pice");
                    string izborKat = Console.ReadLine();

                    string kategorija = (izborKat == "2") ? "PICE" : "HRANA";
                    string id = (i + 1).ToString();

                    string poruka = "PORUDZBINA|" + id + "|" + brojStolaZ5 + "|" + kategorija + "|" + naziv + "|" + cena;
                    clientSocket.Send(Encoding.UTF8.GetBytes(poruka));

                    Console.WriteLine("Poslata porudzbina.");
                }

                Console.WriteLine("Cekam poruke DOSTAVA od servera...");

                int brojDostava = 0;
                while (brojDostava < 5)
                {
                    byte[] buf = new byte[BUFFER_SIZE];
                    int br = clientSocket.Receive(buf);
                    if (br == 0) break;

                    string msg = Encoding.UTF8.GetString(buf, 0, br);
                    Console.WriteLine("SERVER: " + msg);

                    if (msg.StartsWith("DOSTAVA|"))
                        brojDostava++;
                }

                Console.WriteLine("Stigle su sve dostave (5). Zatvaram konobara.");

                clientSocket.Close();
                return;
            }

            // ovde nastavlja samo zadatak 4
            if (izbor != "1")
            {
                clientSocket.Close();
                return;
            }


            Console.WriteLine("Povezan na server!");

            Console.Write("Unesi broj stola: ");
            int brojStola = int.Parse(Console.ReadLine());

            Console.Write("Unesi broj gostiju: ");
            int brojGostiju = int.Parse(Console.ReadLine());


            // Kreiranje objekta Sto
            Sto sto = new Sto(brojStola, brojGostiju, StatusEnum.ZAUZET, new List<Porudzbina>());

            // SERIJALIZACIJA (BinaryFormatter + MemoryStream)
            byte[] dataBuffer1;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, sto);
                dataBuffer1 = ms.ToArray();
            }

            // SLANJE PREKO TCP
            int bytesSentTcp = clientSocket.Send(dataBuffer1);
            Console.WriteLine("Sent {0} bytes", bytesSentTcp);


            // Čekamo potvrdu servera
            byte[] buffer = new byte[BUFFER_SIZE];
            int bytesReceived = clientSocket.Receive(buffer);
            Console.WriteLine("Received: {0}", Encoding.UTF8.GetString(buffer, 0, bytesReceived));

            // Unos 3 porudzbine i slanje serveru
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("Unos porudzbine #{0}", i + 1);

                Console.Write("Unesi naziv artikla: ");
                string nazivArtikla = Console.ReadLine();

                Console.Write("Unesi cenu: ");
                double cena = double.Parse(Console.ReadLine());

                Console.WriteLine("Unesi kategoriju: 1 - Hrana, 2 - Pice");
                string izborKategorije = Console.ReadLine();

                KategorijaEnum kategorija;
                if (izborKategorije == "2")
                    kategorija = KategorijaEnum.PICE;
                else
                    kategorija = KategorijaEnum.HRANA;

                Porudzbina porudzbina = new Porudzbina(nazivArtikla, kategorija, cena, StatusPorudzbine.PRIPREMA);

                // Serijalizacija porudzbine
                byte[] dataBufferTcp;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, porudzbina);
                    dataBufferTcp = ms.ToArray();
                }

                // Slanje porudzbine
                clientSocket.Send(dataBufferTcp);
                Console.WriteLine("Porudzbina poslata serveru.");
            }

            Console.WriteLine("Unesi 1 za obracun racuna:");
            string obracun = Console.ReadLine().Trim();

            if (obracun == "1")
            {
                clientSocket.Send(Encoding.UTF8.GetBytes(obracun));
                Console.WriteLine("Zahtev za obracun racuna je poslat serveru.");
            }
            

            // Primamo racun od servera (kao tekst)
            byte[] racunBuffer = new byte[BUFFER_SIZE];
            int bytesRacun = clientSocket.Receive(racunBuffer);
            string racun = Encoding.UTF8.GetString(racunBuffer, 0, bytesRacun);

            Console.WriteLine("=== RACUN ===");
            Console.WriteLine(racun);


            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();

            Console.ReadKey();
        }
    }
}
