using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsMVC.Models
{

    public class Company
    {

        public int ID { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "Address cannot be longer than 100 characters.")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "City cannot be longer than 50 characters.")]
        public string City { get; set; }

        [StringLength(2, ErrorMessage = "State cannot be longer than 2 characters.")]
        public string State { get; set; }

        [StringLength(10, ErrorMessage = "Postal Code cannot be longer than 10 characters.")]
        [DataType(DataType.PostalCode, ErrorMessage = "Please enter a valid Postal Code.")]
        [Display(Name="Postal Code")]
        public string PostalCode { get; set; }

        public ICollection<Position> Positions { get; set; }
        public ICollection<Contact> Contacts { get; set; }

    }

}
