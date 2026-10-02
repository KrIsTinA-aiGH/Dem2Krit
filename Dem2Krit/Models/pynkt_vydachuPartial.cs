using System;

namespace Dem2Krit.Models
{
    public partial class pynkt_vydachu
    {
        public string FullAddress
        {
            get
            {
                string gName = gorod != null ? gorod.name_gorod : "";
                string uName = ylucha != null ? ylucha.name_ylicha : "";

                return $"{indeks}, г. {gName}, ул. {uName}, д. {nomer_doma}";
            }
        }
    }
}