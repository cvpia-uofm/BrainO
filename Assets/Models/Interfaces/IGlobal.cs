﻿using System.Collections.Generic;

namespace Assets.Models.Interfaces
{
    public interface IGlobal
    {
        bool CorrelationActivated { get; set; }
        bool ROIActivated { get; set; }
        bool AnyRegionSelected { get; set; }

        string Current_atlas { get; set; }
        IDictionary<int, IEnumerable<Region>> Atlas_Regions_dict_index { get; set; }
        IDictionary<string, IEnumerable<Region>> Atlas_Regions_value_pairs { get; set; }

        IEnumerable<Region> Current_Region_list { get; set; }
        IList<Region> Current_Active_Regions { get; set; }

        IEnumerable<Corelation> Current_Correlations { get; set; }

        IList<ROI> Current_rOIs { get; set; }

        bool MouseOverUI { get; set; }


    }
}
