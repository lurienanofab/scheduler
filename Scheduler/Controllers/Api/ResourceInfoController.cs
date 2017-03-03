using LNF.Repository;
using LNF.Repository.Scheduler;
using System.Linq;
using System.Web.Http;

namespace Scheduler.Controllers.Api
{
    public class ResourceInfoController : ApiController
    {
        [Route("api/resource-info")]
        public ResourceInfo[] Get(bool? IsActive = null)
        {
            IQueryable<ResourceInfo> query;

            if (IsActive.HasValue)
                query = DA.Current.Query<ResourceInfo>().Where(x => x.IsActive == IsActive.Value);
            else
                query = DA.Current.Query<ResourceInfo>();

            return query
                .OrderBy(x => x.BuildingName)
                .ThenBy(x => x.LabID)
                .ThenBy(x => x.ProcessTechName)
                .ThenBy(x => x.ResourceName)
                .ToArray();
        }
    }
}
