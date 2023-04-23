using Microsoft.AspNetCore.Http;
using System.Security.Cryptography.Xml;
using WorkLog.Data;
using WorkLog.Models;
using WorkLog.Repository;

namespace WorkLog.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate next;

        public TenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IHttpContextAccessor httpContextAccessor)
        {
            Tenant tenant = null;
            
            string domain = context.Request.Headers.Host;

            if (!string.IsNullOrEmpty(domain))
            {
                TenantRepository _tenantRepository = new TenantRepository(httpContextAccessor);
                tenant = await _tenantRepository.GetTenantByDomain(domain);

                if(tenant == null)
                {
                    return;
                }

                context.Session.SetString("TenantId", tenant.id.ToString());
            }

            context.Items.Add("Tenant", tenant);

            await next.Invoke(context);           
        }
    }
}
