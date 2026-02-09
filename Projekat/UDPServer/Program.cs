using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Server
{
    internal class Program
    {
        private const int SERVER_PORT = 50001;
        private const int BUFFER_SIZE = 2048;
        private static List<string> stanjeStolova = new List<string>();
        private static List<string> listaPorudzbina = new List<string>();
        private static List<string> informacijeORresursima = new List<string>();

        // ZADATAK 5 - red i stek + resursi
        private static Queue<string> redPorudzbina = new Queue<string>();
        private static Stack<string> stekPorudzbina = new Stack<string>();

        private static Socket soketKonobar = null;
        private static Socket soketKuvar = null;
        private static Socket soketBarmen = null;

        private static bool kuvarSlobodan = true;
        private static bool barmenSlobodan = true;

        private static object bravaZadatak5 = new object();
        private class Rezervacija
        {
            public int Sto;
            public string VremeDolaska;   
            public int BrojGostiju;
            public DateTime Kreirana;   
            public int TrajanjeMin;      
        }
        private static Dictionary<int, Rezervacija> rezervacije = new Dictionary<int, Rezervacija>();
        private static Dictionary<int, StatusEnum> statusStolova = new Dictionary<int, StatusEnum>();

        static void Main(string[] args)
        {
            Console.WriteLine("1 - TCP");
            Console.WriteLine("2 - UDP");
            Console.WriteLine("3 - TCP(Zadatak 5)");
            Console.WriteLine("4 - TCP (Rezervacije + Vizualizacija + Multipleksiranje)");

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

            if (izbor == "3")
            {
                PokreniZadatak5();
                return;
            }

            if (izbor == "4")
            {
                PokreniZadatak7();
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

            // Uzimamo tacno primljene bajtove (da ne smeta ostatak bafera)
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

            // Prikaz da se vidi da "cuva stanje"
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

            // Cekanje signala za obracun racuna
            int signal = -1;

            string linijaSignala = PrimiLiniju(acceptedSocket);
            if (linijaSignala == null) return;

            signal = int.Parse(linijaSignala);
            if (signal == 1)
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

                Console.ReadKey();
            }
        }
        private static void PokreniZadatak5()
        {
            Console.WriteLine("=== ZADATAK 5 (TCP) ===");
            Console.WriteLine("Cekam 3 konekcije: KONOBAR, KUVAR, BARMEN...");

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, SERVER_PORT));
            serverSocket.Listen(10);

            // PRIHVATAMO 3 KONEKCIJE (blokirajuce)
            Socket s1 = serverSocket.Accept();
            DodeliUlogu(s1, PrimiLiniju(s1));

            Socket s2 = serverSocket.Accept();
            DodeliUlogu(s2, PrimiLiniju(s2));

            Socket s3 = serverSocket.Accept();
            DodeliUlogu(s3, PrimiLiniju(s3));

            Console.WriteLine("Sve uloge su prijavljene. Server radi red/stek.");

            // niti za kuvara i barmena (da server prima SPREMNO)
            Thread nitKuvar = new Thread(() => OsluskujResurs(soketKuvar, "KUVAR"));
            nitKuvar.IsBackground = true;
            nitKuvar.Start();

            Thread nitBarmen = new Thread(() => OsluskujResurs(soketBarmen, "BARMEN"));
            nitBarmen.IsBackground = true;
            nitBarmen.Start();

            // GLAVNA NIT: prima porudzbine od konobara
            while (true)
            {
                string poruka = PrimiLiniju(soketKonobar);
                if (poruka == null)
                {
                    Thread.Sleep(50);
                    continue;
                }

                if (poruka.StartsWith("PORUDZBINA|"))
                {
                    lock (bravaZadatak5)
                    {
                        listaPorudzbina.Add(poruka);

                        redPorudzbina.Enqueue(poruka);
                        Console.WriteLine("Primljena porudzbina (RED): " + poruka);

                        ObradiRedIStek();
                    }
                }
                else if (poruka.StartsWith("RACUN|"))
                {
                    // RACUN|brojStola
                    string[] d = poruka.Split('|');
                    if (d.Length >= 2)
                    {
                        string sto = d[1];

                        double ukupno = 0;
                        string tekst = "RACUN ZA STO " + sto + "|";

                        lock (bravaZadatak5)
                        {
                            for (int i = 0; i < listaPorudzbina.Count; i++)
                            {
                                // PORUDZBINA|id|sto|kategorija|naziv|cena
                                string[] p = listaPorudzbina[i].Split('|');
                                if (p.Length >= 6 && p[2] == sto)
                                {
                                    string naziv = p[4];
                                    string cenaS = p[5];

                                    double cena;
                                    double.TryParse(cenaS, out cena);

                                    ukupno += cena;
                                    tekst += "- " + naziv + " = " + cena + "|";
                                }
                            }
                        }

                        tekst += "UKUPNO: " + ukupno;
                        PosaljiString(soketKonobar, "RACUN_OK|" + tekst);
                        Console.WriteLine("Poslat racun za sto " + sto);
                    }
                }

            }

            Console.WriteLine("Konobar se diskonektovao. Gasim zadatak 5.");
   
        }

        private static void DodeliUlogu(Socket s, string poruka)
        {
            // ULOGA|KONOBAR / ULOGA|KUVAR / ULOGA|BARMEN
            if (string.IsNullOrWhiteSpace(poruka)) return;

            string[] d = poruka.Split('|');
            if (d.Length < 2) return;

            string uloga = d[1];

            if (uloga == "KONOBAR")
            {
                soketKonobar = s;
                PosaljiString(s, "OK|KONOBAR");
                Console.WriteLine("Prijavljen KONOBAR.");
            }
            else if (uloga == "KUVAR")
            {
                soketKuvar = s;
                kuvarSlobodan = true;
                PosaljiString(s, "OK|KUVAR");
                Console.WriteLine("Prijavljen KUVAR.");
            }
            else if (uloga == "BARMEN")
            {
                soketBarmen = s;
                barmenSlobodan = true;
                PosaljiString(s, "OK|BARMEN");
                Console.WriteLine("Prijavljen BARMEN.");
            }
        }

        private static void OsluskujResurs(Socket s, string nazivResursa)
        {
            while (true)
            {
                string poruka = PrimiLiniju(s);
                if (poruka == null) break;

                // SPREMNO|id|brojStola
                if (poruka.StartsWith("SPREMNO|"))
                {
                    string[] d = poruka.Split('|');
                    if (d.Length < 3) continue;

                    string id = d[1];
                    string sto = d[2];

                    lock (bravaZadatak5)
                    {
                        if (nazivResursa == "KUVAR") kuvarSlobodan = true;
                        if (nazivResursa == "BARMEN") barmenSlobodan = true;

                        Console.WriteLine(nazivResursa + " -> SPREMNO: id=" + id + ", sto=" + sto);

                        // javi konobaru da može dostava
                        if (soketKonobar != null)
                        {
                            PosaljiString(soketKonobar, "DOSTAVA|" + id + "|" + sto);
                        }

                        ObradiRedIStek();
                    }
                }
            }
        }

        private static void ObradiRedIStek()
        {
            // 1) iz REDA pokušaj dodelu, ako ne može -> u STEK
            while (redPorudzbina.Count > 0)
            {
                string por = redPorudzbina.Dequeue();
                if (!PokusajDodelu(por))
                {
                    stekPorudzbina.Push(por);
                    Console.WriteLine("Nema resursa -> porudzbina u STEK: " + por);
                }
            }

            // 2) ako ima slobodnog resursa, skidaj sa vrha STEKA
            bool nastavi = true;
            while (nastavi && stekPorudzbina.Count > 0)
            {
                string por = stekPorudzbina.Peek();
                if (PokusajDodelu(por))
                {
                    Console.WriteLine("Povlacim sa STEKA i dodeljujem: " + por);
                    stekPorudzbina.Pop();
                }
                else
                {
                    nastavi = false;
                }
            }
        }

        private static bool PokusajDodelu(string poruka)
        {
            // PORUDZBINA|id|brojStola|kategorija|naziv|cena
            string[] d = poruka.Split('|');
            if (d.Length < 6) return true;

            string id = d[1];
            string sto = d[2];
            string kategorija = d[3];
            string naziv = d[4];
            string cena = d[5];

            if (kategorija == "HRANA")
            {
                if (soketKuvar != null && kuvarSlobodan)
                {
                    kuvarSlobodan = false;
                    PosaljiString(soketKuvar, "DODELA|" + id + "|" + sto + "|" + kategorija + "|" + naziv + "|" + cena);
                    Console.WriteLine("Dodeljeno KUVARU: " + naziv + " (id=" + id + ")");
                    return true;
                }
                return false;
            }
            else // PICE
            {
                if (soketBarmen != null && barmenSlobodan)
                {
                    barmenSlobodan = false;
                    PosaljiString(soketBarmen, "DODELA|" + id + "|" + sto + "|" + kategorija + "|" + naziv + "|" + cena);
                    Console.WriteLine("Dodeljeno BARMENU: " + naziv + " (id=" + id + ")");
                    return true;
                }
                return false;
            }
        }

        private static void PosaljiString(Socket s, string poruka)
        {
            byte[] data = Encoding.UTF8.GetBytes(poruka+"\n");
            s.Send(data);
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
                    if (r == 0) return null;           // diskonekt
                    if (b[0] == (byte)'\n') break;     // kraj poruke
                    bytes.Add(b[0]);
                }

                return Encoding.UTF8.GetString(bytes.ToArray()).Trim();
            }
            catch
            {
                return null;
            }
        }
        private static void InitStoloveAkoTreba(int maxSto = 10)
        {
            if (statusStolova.Count > 0) return;
            for (int i = 1; i <= maxSto; i++)
                statusStolova[i] = StatusEnum.SLOBODAN;
        }

        private static void ProveriIstekRezervacija()
        {
            // poziva se periodicno (npr jednom u sekundi u Select petlji)
            List<int> zaBrisanje = new List<int>();

            foreach (var kv in rezervacije)
            {
                Rezervacija r = kv.Value;
                // Rezervacija vazi TrajanjeMin od trenutka kreiranja
                DateTime istek = r.Kreirana.AddMinutes(r.TrajanjeMin);
                if (DateTime.Now > istek)
                    zaBrisanje.Add(kv.Key);
            }

            foreach (int sto in zaBrisanje)
            {
                rezervacije.Remove(sto);
                // ako nije zauzet, vrati na slobodan
                if (statusStolova.ContainsKey(sto) && statusStolova[sto] == StatusEnum.REZERVISAN)
                    statusStolova[sto] = StatusEnum.SLOBODAN;
            }
        }
        private static void PrikaziStanjeRestorana()
        {
            Console.Clear();
            Console.WriteLine("=== STANJE RESTORANA (Zadatak 7) ===\n");

            Console.WriteLine("Stolovi:");
            foreach (var kv in statusStolova.OrderBy(k => k.Key))
            {
                int sto = kv.Key;
                StatusEnum st = kv.Value;

                string dodatno = "";
                if (st == StatusEnum.REZERVISAN && rezervacije.ContainsKey(sto))
                {
                    var r = rezervacije[sto];
                    dodatno = $" (dolazak {r.VremeDolaska}, gosti {r.BrojGostiju})";
                }
                Console.WriteLine($"- Sto {sto}: {st}{dodatno}");
            }

            Console.WriteLine("\nResursi:");
            Console.WriteLine($"- Kuvar:  {(kuvarSlobodan ? "SLOBODAN" : "ZAUZET")}");
            Console.WriteLine($"- Barmen: {(barmenSlobodan ? "SLOBODAN" : "ZAUZET")}");
            Console.WriteLine("\nAktivne porudzbine (ako koristis listuPorudzbina/red/ste k iz zad.5):");
           
            Console.WriteLine($"- Red: {redPorudzbina.Count}, Stek: {stekPorudzbina.Count}");
        }

        private static void PokreniZadatak7()
        {
            InitStoloveAkoTreba(10);

            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, SERVER_PORT));
            listenSocket.Listen(10);

            List<Socket> clients = new List<Socket>();
            while (true)
            {
                // 1) priprema liste soketa za citanje
                List<Socket> readSockets = new List<Socket>();
                readSockets.Add(listenSocket);
                readSockets.AddRange(clients);

                // 2) multiplex (timeout 1s)
                Socket.Select(readSockets, null, null, 1000000);

                // 3) periodicno: iste k rezervacija + vizualizacija
                ProveriIstekRezervacija();
                PrikaziStanjeRestorana();

                foreach (Socket s in readSockets)
                {
                    if (s == listenSocket)
                    {
                        Socket client = listenSocket.Accept();
                        clients.Add(client);
                        continue;
                    }

                    string poruka = PrimiLiniju(s);
                    if (poruka == null)
                    {
                        // diskonekt
                        clients.Remove(s);
                        try { s.Close(); } catch { }
                        continue;
                    }

                    ObradiPorukuZadatak7(s, poruka);
                }
            }
        }

        private static void ObradiPorukuZadatak7(Socket s, string poruka)
        {
            if (poruka.StartsWith("ULOGA|"))
            {
                DodeliUlogu(s, poruka); 
                return;
            }

            if (poruka.StartsWith("REZERVACIJA|"))
            {
                string[] d = poruka.Split('|');
                if (d.Length < 5) { PosaljiString(s, "ERR|REZERVACIJA_FORMAT"); return; }

                int sto = int.Parse(d[1]);
                string vreme = d[2];
                int gosti = int.Parse(d[3]);
                int trajanje = int.Parse(d[4]);

                // ako je sto zauzet - odbij
                if (statusStolova.ContainsKey(sto) && statusStolova[sto] == StatusEnum.ZAUZET)
                {
                    PosaljiString(s, "REZERVACIJA|ODBIJENO|ZAUZET");
                    return;
                }

                rezervacije[sto] = new Rezervacija
                {
                    Sto = sto,
                    VremeDolaska = vreme,
                    BrojGostiju = gosti,
                    Kreirana = DateTime.Now,
                    TrajanjeMin = trajanje
                };

                statusStolova[sto] = StatusEnum.REZERVISAN;
                PosaljiString(s, "REZERVACIJA|OK");
                return;
            }

            if (poruka.StartsWith("ZAUZMI|")) {
                string[] d = poruka.Split('|');
                if (d.Length < 2) { PosaljiString(s, "ERR|ZAUZMI_FORMAT"); return; }

                int sto = int.Parse(d[1]);

                statusStolova[sto] = StatusEnum.ZAUZET;
                if (rezervacije.ContainsKey(sto))
                    rezervacije.Remove(sto);

                PosaljiString(s, "ZAUZMI|OK");
                return;
            }

            if (poruka.StartsWith("OSLOBODI|")){
                string[] d = poruka.Split('|');
                int sto = int.Parse(d[1]);
                statusStolova[sto] = StatusEnum.SLOBODAN;
                PosaljiString(s, "OSLOBODI|OK");
                return;
            }
            if (poruka == "STATUS?")
            {
                var sb = new StringBuilder();
                foreach (var kv in statusStolova.OrderBy(k => k.Key))
                    sb.Append($"STO|{kv.Key}:{kv.Value};");
                PosaljiString(s, sb.ToString());
                return;
            }
            PosaljiString(s, "ERR|NEPOZNATA_PORUKA");
        }

    }
}
