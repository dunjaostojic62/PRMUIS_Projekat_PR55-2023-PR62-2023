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
            Console.WriteLine("4 - Zadatak 7 (Rezervacije)");
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

                //sendSocket.Close();
                Console.ReadKey();
                return;
            }
         


            // 1) Kreiranje utičnice:
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 2) Endpoint servera:
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT);

            // 3) Connect:
            clientSocket.Connect(serverEndPoint);

            if (izbor == "4")
            {
                Console.WriteLine("KONOBAR – ZADATAK 7 (Rezervacije)");
                Console.WriteLine("--------------------------------");

                // 1) prijava uloge
                clientSocket.Send(Encoding.UTF8.GetBytes("ULOGA|KONOBAR\n"));
                string odgovor = PrimiLiniju(clientSocket);
                Console.WriteLine("SERVER: " + odgovor);

                Console.Write("Unesi broj stola za rezervaciju: ");
                int sto1 = int.Parse(Console.ReadLine());

                Console.Write("Unesi vreme dolaska (npr 19:30): ");
                string vreme = Console.ReadLine();

                Console.Write("Unesi broj gostiju: ");
                int gosti = int.Parse(Console.ReadLine());

                Console.Write("Unesi trajanje rezervacije u minutima: ");
                int trajanje = int.Parse(Console.ReadLine());

                Console.WriteLine("Saljem rezervaciju...");
                clientSocket.Send(Encoding.UTF8.GetBytes($"REZERVACIJA|{sto1}|{vreme}|{gosti}|{trajanje}\n"));

                odgovor = PrimiLiniju(clientSocket);
                Console.WriteLine("SERVER: " + odgovor);


                // 3) zauzimanje stola
                Console.Write("Da li su gosti stigli i zauzimaju sto? (da/ne): ");
                string stigli = Console.ReadLine().Trim().ToLower();

                if (stigli == "da")
                {
                    Console.WriteLine("Javljam da su gosti dosli...");
                    clientSocket.Send(Encoding.UTF8.GetBytes($"ZAUZMI|{sto1}\n"));
                    odgovor = PrimiLiniju(clientSocket);
                    Console.WriteLine("SERVER: " + odgovor);
                }


                // 4) trazenje statusa stolova
                Console.WriteLine("Trazim status stolova...");
                    clientSocket.Send(Encoding.UTF8.GetBytes("STATUS?\n"));
                    odgovor = PrimiLiniju(clientSocket);
                Console.WriteLine("SERVER STATUS:");

                string status = odgovor.Replace("STO|", "")
                                        .Replace(";", "\n");

                Console.WriteLine(status);
              
                Console.WriteLine("Pritisni taster za izlaz...");
                Console.ReadKey();
                return;
            }

            if (izbor == "3")
            {
                Console.WriteLine("KONOBAR (Zadatak 5) povezan na server.");

                // prijava uloge
                clientSocket.Send(Encoding.UTF8.GetBytes("ULOGA|KONOBAR\n"));

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
                    clientSocket.Send(Encoding.UTF8.GetBytes(poruka+"\n"));

                    Console.WriteLine("Poslata porudzbina.");
                }


                Console.WriteLine("Cekam poruke DOSTAVA od servera...");

                int brojDostava = 0;
                string ostatak = "";

                while (brojDostava < 5)
                {
                    byte[] buf = new byte[BUFFER_SIZE];
                    int br = clientSocket.Receive(buf);
                    if (br == 0) break;

                    ostatak += Encoding.UTF8.GetString(buf, 0, br);

                    string[] linije = ostatak.Split('\n');

                    // poslednji deo može biti nepotpun -> čuvamo ga
                    ostatak = linije[linije.Length - 1];

                    for (int i = 0; i < linije.Length - 1; i++)
                    {
                        string msg = linije[i].Trim();
                        if (msg.Length == 0) continue;

                        Console.WriteLine("SERVER: " + msg);

                        if (msg.StartsWith("DOSTAVA|"))
                            brojDostava++;
                    }
                }

                
                Console.WriteLine("Saljem zahtev za racun...");
                clientSocket.Send(Encoding.UTF8.GetBytes("RACUN|" + brojStolaZ5 + "\n"));

                string odgovor;
                while ((odgovor = PrimiLiniju(clientSocket)) != null)
                {
                    Console.WriteLine("SERVER: " + odgovor);
                    if (odgovor.StartsWith("RACUN_OK|"))
                        break;
                }

                clientSocket.Close();
                return;


            }

            // ovde nastavlja samo zadatak 4
            if (izbor != "1")
            {
               // clientSocket.Close();
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
                clientSocket.Send(Encoding.UTF8.GetBytes("1\n"));
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

        private static string PrimiLiniju(Socket s)
        {
            try
            {
                List<byte> bytes = new List<byte>();
                byte[] b = new byte[1];

                while (true)
                {
                    int r = s.Receive(b, 0, 1, SocketFlags.None);
                    if (r == 0) return null;
                    if (b[0] == (byte)'\n') break;
                    bytes.Add(b[0]);
                }

                return Encoding.UTF8.GetString(bytes.ToArray()).Trim();
            }
            catch
            {
                return null;
            }
        }

    }
}
