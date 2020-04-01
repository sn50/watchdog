using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Entity
{
    public class DealGroup
    {
        public int Id { get; set; }
        public ICollection<Deal> Deals { get; set; }
    }
}
