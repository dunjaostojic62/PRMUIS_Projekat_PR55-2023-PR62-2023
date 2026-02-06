using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Kuvar
{
    internal class Program
    {
        private const int SERVER_PORT = 50001;
        private const string SERVER_IP = "127.0.0.1";

        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT));
            Console.WriteLine("KUVAR povezan.");

            // prijava uloge
            s.Send(Encoding.UTF8.GetBytes("ULOGA|KUVAR"));

            byte[] buffer = new byte[2048];
            int br = s.Receive(buffer);
            Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, br));

            while (true)
            {
                br = s.Receive(buffer);
                string poruka = Encoding.UTF8.GetString(buffer, 0, br);

                // DODELA|id|sto|kategorija|naziv|cena
                string[] d = poruka.Split('|');
                if (d.Length >= 6 && d[0] == "DODELA")
                {
                    string id = d[1];
                    string sto = d[2];
                    string naziv = d[4];

                    Console.WriteLine("DODELJENO: id={0}, sto={1}, {2}", id, sto, naziv);
                    Console.WriteLine("Pritisni ENTER kada je spremno...");
                    Console.ReadLine();

                    string spremno = "SPREMNO|" + id + "|" + sto;
                    s.Send(Encoding.UTF8.GetBytes(spremno));
                    Console.WriteLine("Poslato SPREMNO za id={0}", id);
                }
            }
        }
    }
}
