using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BudgetPlanner.Middleware
{
    public class PageNotFoundMiddleware
    {
        private readonly RequestDelegate next;

        public PageNotFoundMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {

            await this.next(httpContext);
            if (httpContext.Request.Path != "/favicon.ico" && httpContext.Response.StatusCode == 404)
            {
                httpContext.Request.Path = "/Home/NotFoundError";
                await next(httpContext);
            }
        }
    }
    
    public static class PageNotFoundMiddlewareExtensions
    {
        public static IApplicationBuilder UsePageNotFoundErrorPage(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PageNotFoundMiddleware>();
        }
    }
}
