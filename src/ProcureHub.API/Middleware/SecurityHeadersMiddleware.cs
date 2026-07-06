namespace ProcureHub.API.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext ctx)
    {
        var headers = ctx.Response.Headers;

        headers["X-Content-Type-Options"]  = "nosniff";
        headers["X-Frame-Options"]         = "DENY";
        headers["Referrer-Policy"]         = "strict-origin-when-cross-origin";
        headers["X-XSS-Protection"]        = "1; mode=block";
        headers["Permissions-Policy"]      = "camera=(), microphone=(), geolocation=()";

        return next(ctx);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
