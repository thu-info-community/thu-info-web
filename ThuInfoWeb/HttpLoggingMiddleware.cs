using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using ThuInfoWeb.DBModels;

namespace ThuInfoWeb
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, Data data)
        {
            var path = context.Request.Path;
            if (!path.StartsWithSegments("/api"))
            {
                var r = new Request()
                {
                    Method = context.Request.Method,
                    Path = path,
                    Ip = (uint)context.Connection.RemoteIpAddress.Address,
                    Time = DateTime.Now
                };
                await data.CreateHttpRequestLog(r);
            }
            await _next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class HttpLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpLoggingMiddleware>();
        }
    }
}
