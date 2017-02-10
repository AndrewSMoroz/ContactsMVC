using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsMVC.Models
{

    public class Event
    {

        public int ID { get; set; }
        public int PositionID { get; set; }
        public int EventTypeID { get; set; }
        public DateTime DateTime { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public Position Position { get; set; }
        public EventType EventType { get; set; }
        public ICollection<EventContact> EventContacts { get; set; }

    }

}
