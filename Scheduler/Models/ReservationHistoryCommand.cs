namespace Scheduler.Models
{
    public class ReservationHistoryCommand
    {
        /// <summary>
        /// Id of the client making the reservation history change.
        /// </summary>
        public int ClientID { get; set; }

        public string Notes { get; set; }

        public double ForgivenPercentage { get; set; }

        public int AccountID { get; set; }

        public bool EmailClient { get; set; }
    }
}