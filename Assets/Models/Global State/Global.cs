using Assets.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Models.Global_State
{
    public class Global : IGlobal
    {
        public bool CorrelationActivated { get; set; }
        public bool ROIActivated { get; set; }
    }
}
