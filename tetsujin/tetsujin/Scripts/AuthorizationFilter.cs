using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
