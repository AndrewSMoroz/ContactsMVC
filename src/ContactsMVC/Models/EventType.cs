using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsMVC.Models
{

    public class EventType : LookupItem
    {

        public ICollection<Event> Events { get; set; }

    }

}
