using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsMVC
{

    /// <summary>
    /// Holds strongly-typed configuration settings from the sources defined in the Startup.cs constructor
    /// Relies on AddOptions and Configure method calls in Startup.ConfigureServices
    /// </summary>
    public class UISettings
    {

        public string KeyOperationErrorMessage { get; set; } = "OperationErrorMessage";
        public string LookupDescriptionContactTypes { get; set; } = "Contact Types";
        public string LookupDescriptionEventTypes { get; set; } = "Event Types";
        public string LookupDescriptionPhoneTypes { get; set; } = "Phone Types";
        public string SelectListDefaultItemText { get; set; } = "-- None Selected --";
        public string UserErrorMessage { get; set; } = "An unexpected error has occurred.";

    }

}
