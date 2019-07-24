using System.Collections.Generic;
using UnityEngine;

namespace Assets.Models.Interfaces
{
    public interface IGlobal
    {
        bool CorrelationActivated { get; set; }
        bool ROIActivated { get; set; }
        bool AnyRegionSelected { get; set; }
        bool Settings_Activated { get; set; }

        string Current_atlas { get; set; }
        IDictionary<int, IEnumerable<Region>> Atlas_Regions_dict_index { get; set; }
        IDictionary<string, IEnumerable<Region>> Atlas_Regions_value_pairs { get; set; }

        IEnumerable<Region> Current_Region_list { get; set; }
        IList<Region> Current_Active_Regions { get; set; }

        IEnumerable<Corelation> Current_Correlations { get; set; }

        IList<ROI> Current_rOIs { get; set; }

        bool MouseOverUI { get; set; }

        Color Back_col { get; set; }
        Color RH_reg_col { get; set; }
        Color LH_reg_col { get; set; }
        Color Lbl_col { get; set; }
        Color Mesh_col { get; set; }
        Color High_rOI_col { get; set; }
        Color Mid_rOI_col { get; set; }
        Color Low_rOI_col { get; set; }


    }
}
