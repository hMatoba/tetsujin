using Microsoft.AspNetCore.Mvc.Filters;
using System;
using tetsujin.Models;

namespace tetsujin.Filters
{

    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Cookies[Session.SESSION_COOKIE];
            if (!Session.isAuthorized(token))
            {
                context.HttpContext.Response.StatusCode = 403;
                throw new ArgumentException("Forbidden access.");
            }
        }
    }
}