using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsMVC.Models
{

    public class ContactPhoneType : LookupItem
    {

        public ICollection<ContactPhone> ContactPhones { get; set; }

    }

}
