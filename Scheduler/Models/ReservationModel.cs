using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models
{
    public class ReservationModel
    {
        public int ReservationID { get; set; }
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public string ShortCode { get; set; }
        public int ReservedByClientID { get; set; }
        public string ReservedByClientName { get; set; }
        public int StartedByClientID { get; set; }
        public string StartedByClientName { get; set; }
        public bool Startable { get; set; }
        public string NotStartableMessage { get; set; }
        public bool HasInterlock { get; set; }
        public string ReturnUrl { get; set; }
    }
}