using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsMVC.Models
{

    public class Position
    {

        public int ID { get; set; }
        public int CompanyID { get; set; }

        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Posted")]
        public DateTime? DatePosted { get; set; }

        public Company Company { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<PositionContact> PositionContacts { get; set; }

    }

}
