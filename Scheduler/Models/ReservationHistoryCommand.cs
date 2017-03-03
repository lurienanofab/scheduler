namespace Scheduler.Models
{
    public class ReservationHistoryCommand
    {
        public string Notes { get; set; }

        public double ForgivenPercentage { get; set; }

        public int AccountID { get; set; }

        public bool EmailClient { get; set; }
    }
}