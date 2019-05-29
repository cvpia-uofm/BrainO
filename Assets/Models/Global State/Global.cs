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

        public IDictionary<int, IEnumerable<Regions>> Atlas_Regions_dict_index { get; set; }
        public IDictionary<string, IEnumerable<Regions>> Atlas_Regions_value_pairs { get; set; }

        public IEnumerable<Regions> Current_Region_list { get; set; }
        public IList<Regions> Current_Active_Regions { get; set; }

        public IEnumerable<Corelation> Current_Correlations { get; set; }

        public bool MouseOverUI { get; set; }
    }
}
