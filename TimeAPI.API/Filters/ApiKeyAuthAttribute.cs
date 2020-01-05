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
using System.Threading.Tasks;

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

            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apikey = configuration.GetValue<string>(key: "ApiKey");

            if (!apikey.Equals(_ApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next().ConfigureAwait(true);

        }
    }




}
