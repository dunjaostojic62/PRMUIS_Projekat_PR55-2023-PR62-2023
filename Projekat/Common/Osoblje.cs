using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Serializable]
    public class Osoblje
    {
        public TipEnum Tip { get; set; }
        public StatusEnum Status { get; set; }

        public Osoblje(TipEnum tip, StatusEnum status)
        {
            Tip = tip;
            Status = status;
        }
    }
}
