using Assets.Func_Area_Model;
using Assets.Models;
using AutoMapperFactory;
using ExcelFactory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLib
{
    class Program
    {
        static string data_1;
        static string[] data;
        static void Main(string[] args)
        {
            
            using (var reader = new StreamReader(@"C:\Users\KAM\OneDrive - The University of Memphis\DK_atlas_Visualize.csv"))
            {

                data_1 = reader.ReadToEnd();
                data = data_1.Split('\n');
            }
            var regions = MapperFactory<ROI>.Map_CSV(data,MapperEnums.Inputs.Regions);

        }
    }
}
