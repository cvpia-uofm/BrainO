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
        public bool AnyRegionSelected { get; set; }

        public string Current_atlas { get; set; }

        public IDictionary<int, IEnumerable<Region>> Atlas_Regions_dict_index { get; set; }
        public IDictionary<string, IEnumerable<Region>> Atlas_Regions_value_pairs { get; set; }

        public IEnumerable<Region> Current_Region_list { get; set; }
        public IList<Region> Current_Active_Regions { get; set; }

        public IEnumerable<Corelation> Current_Correlations { get; set; }

        public IList<ROI> Current_rOIs { get; set; }

        public bool MouseOverUI { get; set; }
    }
}
