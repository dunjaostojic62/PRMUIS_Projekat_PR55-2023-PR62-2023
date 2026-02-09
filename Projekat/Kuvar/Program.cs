using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.IO;


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

            s.Send(Encoding.UTF8.GetBytes("ULOGA|KUVAR\n"));

            byte[] buffer = new byte[2048];
            int br = s.Receive(buffer);
            Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, br));

            while (true)
            {
                
                string poruka = PrimiLiniju(s);
                if (poruka == null) break;

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

                    string spremno = "SPREMNO|" + id + "|" + sto + "\n";
                    s.Send(Encoding.UTF8.GetBytes(spremno));
                    Console.WriteLine("Poslato SPREMNO za id={0}", id);
                }
            }
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
