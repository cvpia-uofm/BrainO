using Assets.Models.Interfaces;
using UnityEngine;

namespace Assets.Models
{
    public class Region : IRegion
    {
        public string Abbreviation { get; set; }
        public string Region_Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public string Hemisphere { get; set; }
        public string Note { get; set; }

        
    }
}