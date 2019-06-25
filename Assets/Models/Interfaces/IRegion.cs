using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Models.Interfaces
{
    public interface IRegion
    {
        string Abbreviation { get; set; }
        string Region_Name { get; set; }
        double X { get; set; }
        double Y { get; set; }
        double Z { get; set; }
        string Hemisphere { get; set; }
        string Note { get; set; }
    }
}
