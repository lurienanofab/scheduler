using System;

namespace Scheduler.Models
{
    public class UpdateBillingCommand
    {
        public int ClientID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}