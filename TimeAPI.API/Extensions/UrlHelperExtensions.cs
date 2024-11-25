using TimeAPI.API.Controllers;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return "http://www.enforcesolutions.com/verification?userId=" + userId + "&code="  + code;
            //return urlHelper.Action(  + "&code=" 
            //    action: nameof(AccountController.ConfirmEmail),
            //    controller: "Account",
            //    values: new { userId, code },
            //    protocol: scheme);
        }

        public static string PasswordLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return "http://www.enforcesolutions.com//password-setup?userId=" + userId + "&code=" + code;
            //return urlHelper.Action(
            //    action: nameof(AccountController.ResetPassword),
            //    controller: "Account",
            //    values: new { userId, code },
            //    protocol: scheme);
        }

        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return "http://www.enforcesolutions.com//password-setup?userId=" + userId + "&code=" + code;
            //return urlHelper.Action(
            //    action: nameof(HomeController.Get),
            //    controller: "Account",
            //    values: new { userId, code },
            //    protocol: scheme);
        }
    }
}
