﻿using AutoMapperFactory;
using ExcelFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Models.Correlation_Generator
{
    public static class CorrelationGenerator
    {
        public static IEnumerable<Corelation> GenerateRandomCorrelation(IEnumerable<Region> atlas)
        {
            string[] corr_matrix = new string[atlas.Count() + 2];
            StringBuilder regions = new StringBuilder(" ");
            Random random = new Random();
            GenerateColumns(atlas, corr_matrix, regions);

            for (int i = 1; i < corr_matrix.Length - 1; i++)
            {
                regions.Remove(0, regions.Length);
                regions.Append(atlas.ElementAt(i-1).Abbreviation);
                regions.Append(",");

                for (int j = 1; j < corr_matrix.Length - 1; j++)
                {
                    float r = random.Next(0, 150) / 151f;
                    var random_weight = r.ToString("n2");
                    regions.Append(random_weight);

                    if (j != atlas.Count())
                        regions.Append(",");
                }
                corr_matrix[i] = regions.ToString();
            }
            return MapperFactory<Corelation>.Map_CSV(corr_matrix, MapperEnums.Inputs.Correlations);
        }

        private static void GenerateColumns(IEnumerable<Region> atlas, string[] corr_matrix, StringBuilder regions)
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