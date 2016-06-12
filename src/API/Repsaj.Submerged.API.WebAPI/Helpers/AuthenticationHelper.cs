using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Repsaj.Submerged.API.Helpers
{
    public class AuthenticationHelper
    {
        /// <summary>
        /// Returns the claims based user id from the current set of claims
        /// </summary>
        public static string UserId
        {
            get
            {
                return ClaimsPrincipal.Current.FindFirst("stable_sid").Value;
            }
        }
    }
}