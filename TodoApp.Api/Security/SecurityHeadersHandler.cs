using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WebApiDemo1.Security
{
    // Adds common OWASP security headers to all API responses
    public class SecurityHeadersHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            response.Headers.Add("X-Content-Type-Options", "nosniff");
            response.Headers.Add("X-Frame-Options", "DENY");
            response.Headers.Add("X-XSS-Protection", "1; mode=block");
            response.Headers.Add("Referrer-Policy", "no-referrer");
            // Set cache headers using proper properties
            response.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
            {
                NoStore = true,
                NoCache = true,
                MustRevalidate = true,
                Private = true
            };
            response.Headers.Pragma.Clear();
            response.Headers.Pragma.Add(new System.Net.Http.Headers.NameValueHeaderValue("no-cache"));
            if (response.Content != null)
            {
                response.Content.Headers.Expires = DateTimeOffset.UtcNow;
            }
            // Add Content-Security-Policy if needed
            // response.Headers.Add("Content-Security-Policy", "default-src 'self'");
            return response;
        }
    }
}
