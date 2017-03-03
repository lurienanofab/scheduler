using LNF.Cache;
using Scheduler.Models;
using System.Web.Http;

namespace Scheduler.Controllers.Api
{
    public class DefaultController : ApiController
    {
        [Route("api")]
        public string Get()
        {
            return "scheduler-api";
        }

        [Route("api/current-user")]
        public UserModel GetCurrentUser()
        {
            return new UserModel()
            {
                ClientID = CacheManager.Current.ClientID,
                UserName = CacheManager.Current.CurrentUser.UserName,
                DisplayName = CacheManager.Current.CurrentUser.DisplayName,
                Email = CacheManager.Current.CurrentUser.Email,
                Privs = (int)CacheManager.Current.CurrentUser.Privs
            };
        }
    }
}
