using System.Buffers.Binary;
using System.Net;
using ThuInfoWeb.DBModels;

namespace ThuInfoWeb;

public sealed class HttpLoggingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context, Data data, ILogger<HttpLoggingMiddleware> logger)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            var ip = (context.Connection.RemoteIpAddress ?? IPAddress.None).MapToIPv4();
            var r = new Request
            {
                Method = context.Request.Method,
                Path = context.Request.Path,
                Ip = BinaryPrimitives.ReadUInt32BigEndian(ip.GetAddressBytes()),
                Time = DateTime.Now
            };

            try
            {
                await data.CreateHttpRequestLogAsync(r);
            }
            catch (Exception ex)
            {
                ApplicationLog.HttpRequestLogPersistenceFailed(logger, ex, context.Request.Path);
            }
        }

        await _next(context);
    }
}

public static class HttpLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseHttpLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HttpLoggingMiddleware>();
    }
}
