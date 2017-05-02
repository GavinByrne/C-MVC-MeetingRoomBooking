using BookingSystemData.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MRBS.Attributes
{
    public class BookingAuthorize : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if(HttpContext.Current.User.Identity.IsAuthenticated == false)
            {
                return false;
            }

            if (Roles.Any())
            {
                BookingRepository repository = new BookingRepository();
                var userRoles = repository.GetRoles(HttpContext.Current.User.Identity.Name);

                var allowedRoles = Roles.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var role in allowedRoles)
                {
                    if (userRoles.Any(ur => ur == role))
                    {
                        //The user has one of the allowed roles specified in the BookingAuthorize(Roles = "xxx, xxx, ...")
                        return true;
                    };
                }
                
                //If we got here the user didn't have any of the roles specified, return false
                return false;
            }

            //If we used the BookingAuthorize attribute without specifying any roles then the default AuthorizeAttribute behaviour
            return base.AuthorizeCore(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new ViewResult { ViewName = "Unauthorized" };
            filterContext.HttpContext.Response.StatusCode = 403;
        }
    }
}