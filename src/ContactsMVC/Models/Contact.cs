using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsMVC.Models
{

    public class Contact
    {

        public int ID { get; set; }
        public int CompanyID { get; set; }
        public int ContactTypeID { get; set; }

        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters.")]
        [Display(Name="First Name")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Last Name cannot be longer than 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                return ((FirstName ?? "") + " " + (LastName ?? "")).Trim();
            }
        }
        public string FullNameLastNameFirst
        {
            get
            {
                return ((LastName ?? "") + (string.IsNullOrEmpty(FirstName) ? "" : ", " + (FirstName ?? ""))).Trim();
            }
        }

        public Company Company { get; set; }
        public ContactType ContactType { get; set; }
        public ICollection<ContactPhone> ContactPhones { get; set; }
        public ICollection<PositionContact> PositionContacts { get; set; }
        public ICollection<EventContact> EventContacts { get; set; }

    }

}
