﻿using Assets.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Models.Global_State
{
    public class Global : IGlobal
    {
        public bool CorrelationActivated { get; set; }
        public bool ROIActivated { get; set; }
        public bool AnyRegionSelected { get; set; }
        public bool Settings_Activated { get; set; }
        public bool DoubleEscape_ROI_Deactivation { get; set; }
        public string Current_atlas { get; set; }

        public IDictionary<int, IEnumerable<Region>> Atlas_Regions_dict_index { get; set; }
        public IDictionary<string, IEnumerable<Region>> Atlas_Regions_value_pairs { get; set; }

        public IEnumerable<Region> Current_Region_list { get; set; }
        public IList<Region> Current_Active_Regions { get; set; }

        public IEnumerable<Corelation> Current_Correlations { get; set; }

        public IList<ROI> Current_rOIs { get; set; }

        public bool MouseOverUI { get; set; }

        public Color Back_col { get; set; }
        public Color RH_reg_col { get; set; }
        public Color LH_reg_col { get; set; }
        public Color Lbl_col { get; set; }
        public Color Mesh_col { get; set; }
        public Color High_rOI_col { get; set; }
        public Color Mid_rOI_col { get; set; }
        public Color Low_rOI_col { get; set; }
    }
}
