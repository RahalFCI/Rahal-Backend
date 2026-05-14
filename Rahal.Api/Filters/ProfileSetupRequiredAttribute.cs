using Microsoft.AspNetCore.Mvc.Filters;

namespace Rahal.Api.Filters
{
    public class ProfileSetupRequiredAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ProfileSetupRequiredFilter>();
        }
    }

}
