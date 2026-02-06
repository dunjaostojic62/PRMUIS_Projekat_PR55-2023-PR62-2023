using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Common;


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

            // uzimamo tacno primljene bajtove (da ne smeta ostatak bafera)
            byte[] tacniBajtovi = new byte[bytesReceivedTcp];
            Array.Copy(buffer, tacniBajtovi, bytesReceivedTcp);

            // DESERIJALIZACIJA (BinaryFormatter + MemoryStream)
            Sto primljeniSto;
            using (MemoryStream ms = new MemoryStream(tacniBajtovi))
            {
                BinaryFormatter bf = new BinaryFormatter();
                primljeniSto = (Sto)bf.Deserialize(ms);
            }

            // TEST ISPIS
            Console.WriteLine("Primljen sto:");
            Console.WriteLine("Broj stola: {0}", primljeniSto.BrojStola);
            Console.WriteLine("Broj gostiju: {0}", primljeniSto.BrojGostiju);
            Console.WriteLine("Status: {0}", primljeniSto.Status);
            Console.WriteLine("Broj porudzbina: {0}", primljeniSto.Porudzbine.Count);

            // ZADATAK 2 
            stanjeStolova.Add(primljeniSto.BrojStola + "|" + primljeniSto.BrojGostiju);

            // prikaz da se vidi da "cuva stanje"
            Console.WriteLine("Stanje stolova (server pamti):");
            for (int i = 0; i < stanjeStolova.Count; i++)
            {
                Console.WriteLine(stanjeStolova[i]);
            }

            acceptedSocket.Send(Encoding.UTF8.GetBytes("OK"));

            // Lista aktivnih zadataka (porudzbina) za ovaj sto
            List<Porudzbina> aktivnePorudzbine = new List<Porudzbina>();

            if (primljeniSto.Porudzbine == null)
                primljeniSto.Porudzbine = new List<Porudzbina>();

            int br_porudzbina = 0;

            while (br_porudzbina < 3)
            {
                byte[] porudzbinaBuffer = new byte[BUFFER_SIZE];
                int bytesPorudzbina = acceptedSocket.Receive(porudzbinaBuffer);

                byte[] tacniBajtoviPorudzbina = new byte[bytesPorudzbina];
                Array.Copy(porudzbinaBuffer, tacniBajtoviPorudzbina, bytesPorudzbina);

                Porudzbina primljenaPorudzbina;
                using (MemoryStream ms = new MemoryStream(tacniBajtoviPorudzbina))
                {
                    BinaryFormatter bf = new BinaryFormatter();  
                    primljenaPorudzbina = (Porudzbina)bf.Deserialize(ms);
                }

                aktivnePorudzbine.Add(primljenaPorudzbina);
                primljeniSto.Porudzbine.Add(primljenaPorudzbina);

                Console.WriteLine("Primljena porudzbina: {0}, {1}, {2}",
                    primljenaPorudzbina.NazivArtikla,
                    primljenaPorudzbina.Kategorija,
                    primljenaPorudzbina.Cena);

                br_porudzbina++;
            }

            //cekanje i primanje signala 
            int signal = -1;
            byte[] bff = new byte[10];
            acceptedSocket.Receive(bff);
            signal = int.Parse(Encoding.UTF8.GetString(bff));
            if(signal == 1)
            {
                // Racunanje ukupnog iznosa
                double ukupno = 0;
                for (int i = 0; i < aktivnePorudzbine.Count; i++)
                {
                    ukupno += aktivnePorudzbine[i].Cena;
                }

                Console.WriteLine("Ukupan iznos racuna je: {0}", ukupno);

                // Formiranje racuna 
                string racun =
                    "Sto: " + primljeniSto.BrojStola + Environment.NewLine +
                    "Broj gostiju: " + primljeniSto.BrojGostiju + Environment.NewLine +
                    "Porudzbine:" + Environment.NewLine;

                for (int i = 0; i < aktivnePorudzbine.Count; i++)
                {
                    Porudzbina p = aktivnePorudzbine[i];
                    racun += "- " + p.NazivArtikla + " (" + p.Kategorija + ") = " + p.Cena + Environment.NewLine;
                }

                racun += "Ukupno: " + ukupno;

                Console.WriteLine("Saljem racun konobaru...");


                // Slanje racuna konobaru
                acceptedSocket.Send(Encoding.UTF8.GetBytes(racun));
                Console.WriteLine("Racun poslat konobaru.");


                acceptedSocket.Shutdown(SocketShutdown.Both);
                acceptedSocket.Close();
                serverSocket.Close();

                Console.ReadKey();
            }
            
        }
    }
}
