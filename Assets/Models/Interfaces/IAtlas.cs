using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Models.Interfaces
{
    public interface IAtlas
    {
        IEnumerable<Region> Desikan_Atlas { get; set; }
        IEnumerable<Region> Destrieux_Atlas { get; set; }
        IEnumerable<Region> Craddock_Atlas { get; set; }
        IEnumerable<Region> Aal116_Atlas { get; set; }
        IEnumerable<Region> Aal90_Atlas { get; set; }

       
    }
}
