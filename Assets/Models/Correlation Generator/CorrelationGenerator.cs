using Assets.Func_Area_Model;
using AutoMapperFactory;
using ExcelFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Models.Correlation_Generator
{
    public static class CorrelationGenerator
    {
        public static IEnumerable<Corelation> GenerateRandomCorrelation(IEnumerable<Regions> atlas)
        {
            string[] corr_matrix = new string[atlas.Count() + 1];
            StringBuilder regions = new StringBuilder(" ");
            Random random = new Random();
            GenerateColumns(atlas, corr_matrix, regions);

            for (int i = 1; i < atlas.Count(); i++)
            {
                regions.Remove(0, regions.Length);
                regions.Append(atlas.ElementAt(i-1).Abbreviation);
                regions.Append(",");

                for (int j = 0; j < atlas.Count(); j++)
                {
                    float r = random.Next(0, 150) / 151f;
                    var random_weight = r.ToString("n2");
                    regions.Append(random_weight);

                    if (j != atlas.Count() - 1)
                        regions.Append(",");
                }
                corr_matrix[i + 1] = regions.ToString();
            }
            return MapperFactory<Corelation>.Map_CSV(corr_matrix, MapperEnums.Inputs.Correlations);
        }

        private static void GenerateColumns(IEnumerable<Regions> atlas, string[] corr_matrix, StringBuilder regions)
        {
            foreach (var region in atlas)
            {
                regions.Append(",");
                regions.Append(region.Abbreviation);

                corr_matrix[0] = regions.ToString();
            }
        }
    }
}
