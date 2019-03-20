using Assets.Func_Area_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Models.Interfaces
{
    public interface IAtlas
    {
        IEnumerable<Regions> Desikan_Atlas { get; set; }
        IEnumerable<Regions> Destrieux_Atlas { get; set; }
        IEnumerable<Regions> Craddock_Atlas { get; set; }
        IEnumerable<Regions> Aal116_Atlas { get; set; }
        IEnumerable<Regions> Aal90_Atlas { get; set; }

       
    }
}
