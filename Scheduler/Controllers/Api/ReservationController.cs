using LNF.Repository;
using LNF.Repository.Scheduler;
using Scheduler.Models;
using System.Threading.Tasks;
using System.Web.Http;

namespace Scheduler.Controllers.Api
{
    public class ReservationController : ApiController
    {
        private ReservationManager _manager;

        public ReservationController()
        {
            _manager = new ReservationManager(ActionContext);
        }

        [Route("api/reservation/{reservationId}")]
        public ReservationModel Get(int reservationId)
        {
            return _manager.CreateReservationModel(DA.Current.Single<Reservation>(reservationId));
        }

        [HttpGet, Route("api/reservation/{reservationId}/start")]
        public async Task<ReservationModel> Start(int reservationId)
        {
            return await _manager.Start(reservationId);
        }

        [HttpPost, Route("api/reservation/{reservationId}/save-history")]
        public async Task<bool> SaveHistory([FromUri] int reservationId, [FromBody] ReservationHistoryCommand cmd)
        {
            return await _manager.SaveHistory(reservationId, cmd);
        }

        [HttpPost, Route("api/reservation/update-billing")]
        public async Task<bool> UpdateBilling([FromBody] UpdateBillingCommand cmd)
        {
            return await _manager.UpdateBilling(cmd);
        }
    }
}
