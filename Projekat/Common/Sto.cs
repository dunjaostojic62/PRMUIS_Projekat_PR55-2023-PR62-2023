using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Serializable]
    public class Sto
    {
        public int BrojStola {  get; set; }
        public int BrojGostiju { get; set; }
        public StatusEnum Status { get; set; }
        public List<Porudzbina> Porudzbine {  get; set; }

        public Sto(int brojStola, int brojGostiju, StatusEnum status, List<Porudzbina> porudzbine)
        {
            BrojStola = brojStola;
            BrojGostiju = brojGostiju;
            Status = status;
            Porudzbine = porudzbine;
        }
    }
}
