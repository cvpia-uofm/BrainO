﻿using Assets.Models.Interfaces;
using System.Collections.Generic;

namespace Assets.Models
{
    public class Atlas : IAtlas
    {
        public IEnumerable<Regions> Desikan_Atlas { get; set; }
        public IEnumerable<Regions> Destrieux_Atlas { get; set; }
        public IEnumerable<Regions> Craddock_Atlas { get; set; }
        public IEnumerable<Regions> Aal116_Atlas { get; set; }
        public IEnumerable<Regions> Aal90_Atlas { get; set; }

        public const string DSK_Atlas = "Desikan_Atlas";
        public const string DTX_Atlas = "Destrieux_Atlas";
        public const string CDK_Atlas = "Craddock_Atlas";
        public const string A116_Atlas = "Aal116_Atlas";
        public const string A90_Atlas = "Aal90_Atlas";

        public Atlas()
        {
            Desikan_Atlas = new List<Regions>();
            Destrieux_Atlas = new List<Regions>();
            Craddock_Atlas = new List<Regions>();
            Aal116_Atlas = new List<Regions>();
            Aal90_Atlas = new List<Regions>();
        }
    }
}