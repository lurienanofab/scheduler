using System.Web.Mvc;
using LNF.Repository;
using LNF.Repository.Scheduler;

namespace Scheduler.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("history/{id}")]
        public ActionResult History(int id)
        {
            return Redirect(string.Format("/sselonline/?view=/sselscheduler/ReservationHistory.aspx?ReservationID={0}", id));
        }

        [Route("resource/{id}")]
        public ActionResult Resource(int id)
        {
            var res = DA.Current.Single<ResourceInfo>(id);
            return Redirect(string.Format("/sselonline/?view=/sselscheduler/ResourceDayWeek.aspx?Path={0}:{1}:{2}:{3}", res.BuildingID, res.LabID, res.ProcessTechID, res.ResourceID));
        }
    }
}