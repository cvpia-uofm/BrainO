using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Func_Area_Model
{
    public class Regions
    {
        public string Abbreviation { get; set; }
        public string Region_Name { get; set; }
        public Vector3 Region_Co_Ord { get; set; }
        public enum Hemisphere { Right, Left }
        public string Note;
    }
}
