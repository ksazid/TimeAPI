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

namespace TimeAPI.API.Filters
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "ApiKey";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //HttpRequestExtensions.IsLocal(context.HttpContext.Request);
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var _ApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            //UserHelpers.GetApiKeyByUserID(_ApiKey);

            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            //using (var unitOfWork = new DapperUnitOfWork(configuration.GetConnectionString("DefaultConnection")))
            //{
            //    unitOfWork.SubscriptionRepository.FindByApiKeyByUserID();
            //}



            var apikey = configuration.GetValue<string>(key: "ApiKey");

            if (!apikey.Equals(_ApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next().ConfigureAwait(true);
        }


        //public static string SubjectId(this ClaimsPrincipal user) 
        //    { return user?.Claims?.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase))?.Value; }

    }
}
