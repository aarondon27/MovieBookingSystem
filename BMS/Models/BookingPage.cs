using System;
using System.Collections.Generic;

namespace BMS.Models
{
    public class BookingPage
    {
        public required Movie Movie { get; set; }
        public required List<string> Dates { get; set; }
        public required Dictionary<string, List<string>> Theatres { get; set; }    
    }
}
