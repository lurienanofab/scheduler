using LNF.Repository;
using LNF.Repository.Scheduler;
using Scheduler.Models;
using System.Net;
using System.Net.Http;
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
        public ReservationModel Get(int reservationId, int clientId = 0)
        {
            var rsv = DA.Current.Single<Reservation>(reservationId);

            if (rsv != null)
            {
                int cid = (clientId == 0) ? rsv.Client.ClientID : clientId;
                return _manager.CreateReservationModel(rsv, cid, Request.GetOwinContext().Request.RemoteIpAddress);
            }
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Reservation does not exist."));
        }

        [HttpGet, Route("api/reservation/{reservationId}/start")]
        public async Task<ReservationModel> Start(int reservationId, int clientId = 0)
        {
            var rsv = DA.Current.Single<Reservation>(reservationId);

            if (rsv != null)
            {
                int cid = (clientId == 0) ? rsv.Client.ClientID : clientId;
                return await _manager.Start(reservationId, cid, Request.GetOwinContext().Request.RemoteIpAddress);
            }
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Reservation does not exist."));
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
