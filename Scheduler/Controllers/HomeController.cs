using LNF.Repository;
using LNF.Repository.Scheduler;
using System.Net;
using System.Web.Mvc;

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
            string view = WebUtility.UrlEncode(string.Format("/sselscheduler/ReservationHistory.aspx?ReservationID={0}", id));
            return Redirect(string.Format("/sselonline/?view={0}", view));
        }

        [Route("resource/{id}")]
        public ActionResult Resource(int id)
        {
            var res = DA.Current.Single<ResourceInfo>(id);
            if (res != null)
            {
                string view = WebUtility.UrlEncode(string.Format("/sselscheduler/ResourceDayWeek.aspx?Path={0}:{1}:{2}:{3}", res.BuildingID, res.LabID, res.ProcessTechID, res.ResourceID));
                return Redirect(string.Format("/sselonline/?view={0}", view));
            }
            else
            {
                return HttpNotFound(string.Format("Cannot not find a resource with ResourceID = {0}", id));
            }
        }
    }
}