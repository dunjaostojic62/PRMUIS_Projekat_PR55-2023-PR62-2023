using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Serializable]
    public class Porudzbina
    {
        public string NazivArtikla {  get; set; }
        public KategorijaEnum Kategorija { get; set; }
        public double Cena { get; set; }
        public StatusPorudzbine Status {  get; set; }

        public Porudzbina(string nazivArtikla, KategorijaEnum kategorija, double cena, StatusPorudzbine status)
        {
            NazivArtikla = nazivArtikla;
            Kategorija = kategorija;
            Cena = cena;
            Status = status;
        }
    }
}
