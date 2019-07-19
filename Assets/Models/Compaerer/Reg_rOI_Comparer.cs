using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Models.Compaerer
{
    public class Reg_rOI_Comparer : IEqualityComparer<Region>
    {
        public bool Equals(Region x, Region y)
        {
            if(x is null || y is null)
            {
                return false;
            }

            return x.Abbreviation.ToUpper() == y.Abbreviation.ToUpper();
        }

        public int GetHashCode(Region obj)
        {
            if (obj is null) return 0;

            int hashnumf = obj.GetHashCode();

            return hashnumf;
        }
    }
}
