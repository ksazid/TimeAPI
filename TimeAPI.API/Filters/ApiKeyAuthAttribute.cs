using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TimeAPI.Data;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Filters
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "ApiKey";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //HttpRequestExtensions.IsLocal(context.HttpContext.Request);
            //if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var _ApiKey))
            //{
            //    context.Result = new UnauthorizedResult();
            //    return;
            //}


            var userId = context.HttpContext.User.FindFirst("UserID").Value;
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            Subscription _Subscription = new Subscription();
            using (var unitOfWork = new DapperUnitOfWork(configuration.GetConnectionString("DefaultConnection")))
            {
                _Subscription = unitOfWork.SubscriptionRepository.GetByApiKeyByUserID(userId);
                DateTime _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;

                //if (_Subscription.is_trial)
                //{
                //    if (_dateTime > Convert.ToDateTime(_Subscription.subscription_end_date))
                //    {
                //        context.Result = new UnauthorizedResult();
                //        return;
                //    }
                //}

                //if (_Subscription.is_subscibe_after_trial)
                //{
                //    if (_dateTime > Convert.ToDateTime(_Subscription.subscription_end_date))
                //    {
                //        context.Result = new Microsoft.AspNetCore.Mvc.ContentResult();
                //        return;
                //    }
                //}

            }


            //var apikey = configuration.GetValue<string>(key: "ApiKey");

            //if (!apikey.Equals(apikey))
            //{
            //    context.Result = new UnauthorizedResult();
            //    return;
            //}

            await next().ConfigureAwait(true);
        }


        //public static string SubjectId(this ClaimsPrincipal user) 
        //    { return user?.Claims?.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase))?.Value; }

    }
}
