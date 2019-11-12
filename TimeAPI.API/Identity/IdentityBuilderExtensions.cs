using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TimeAPI.API.Models;

namespace TimeAPI.API.Identity
{
    public static class IdentityBuilderExtensions
    {
        public static IdentityBuilder AddCustomStores(this IdentityBuilder builder)
        {
            builder.Services.AddTransient<IUserStore<ApplicationUser>, CustomUserStore>();
            builder.Services.AddTransient<IRoleStore<IdentityRole>, CustomRoleStore>();
            return builder;
        }
    }
}
