using System.Collections.Generic;

namespace Assets.Models.Interfaces
{
    public interface IGlobal
    {
        bool CorrelationActivated { get; set; }
        bool ROIActivated { get; set; }
        IDictionary<int, IEnumerable<Regions>> Atlas_Regions_dict_index { get; set; }
        IDictionary<string, IEnumerable<Regions>> Atlas_Regions_value_pairs { get; set; }

    }
}
