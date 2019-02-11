using Assets.Func_Area_Model;
using ExcelFactoryLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLib
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Regions> regions= ExcelFactory<Regions>.Map("..//..//..//Assets//Regions//RegionData.xls").ToList();
            Console.WriteLine(regions.Count);
        }
    }
}
