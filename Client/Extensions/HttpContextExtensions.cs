using Microsoft.AspNetCore.Http;
using System;
using System.Security.Cryptography;

namespace Client.Extensions
{
    public static class HttpContextExtensions
    {
        private const string NonceKey = "CLIENT_NONCE";

        public static void SetPageSecurityHeaders(this HttpContext context, bool isDevelopment)
        {
            var headers = context.Response.Headers;
            var nonce = context.GetNonce();

            string csp;
            if (isDevelopment)
            {
                // Allow all localhost connections during development
                csp = $"default-src 'self'; " +
                      $"style-src 'self' 'nonce-{nonce}' 'unsafe-inline'; " +
                      $"script-src 'self' 'nonce-{nonce}' 'unsafe-inline' 'unsafe-eval' http://localhost/; " +
                      // Attempt to allow all subdomains and ports (not officially supported for ports)
                      $"connect-src 'self' ws: wss: http://localhost:*";
            }
            else
            {
                // More restrictive CSP in production
                csp = $"default-src 'none'; " +
                      $"font-src 'self'; " +
                      $"img-src 'self' data:; " +
                      $"style-src 'self' 'nonce-{nonce}' 'unsafe-inline'; " +
                      $"script-src 'nonce-{nonce}'; " +
                      $"connect-src 'self';";
            }

            headers.Append("Content-Security-Policy", csp);
            headers.Append("Cache-Control", "no-store");
        }

        public static void SetStaticFileSecurityHeaders(this HttpContext context, bool isDevelopment)
        {
            var headers = context.Response.Headers;

            SetGeneralSecurityHeaders(headers, isDevelopment);

            headers.Append("Content-Security-Policy", "default-src 'none';");

            headers.Append("Cache-Control", "public");
        }

        private static void SetGeneralSecurityHeaders(IHeaderDictionary headers, bool isDevelopment)
        {
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers.XContentTypeOptions = "nosniff";
            headers["X-Permitted-Cross-Domain-Policies"] = "none";
            headers["Cross-Origin-Embedder-Policy"] = "require-corp";
            headers["Cross-Origin-Resource-Policy"] = "same-site";
            headers["Cross-Origin-Opener-Policy"] = "same-origin";

            if (!isDevelopment)
            {
                headers.StrictTransportSecurity = "max-age=31536000";
            }

            // Remove x-aspnet-version header if it exists
            headers.Remove("x-aspnet-version");
        }

        public static string GetNonce(this HttpContext context)
        {
            return context.Items[NonceKey] as string ?? string.Empty;
        }

        internal static void SetNonce(this HttpContext context)
        {
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

            context.Items[NonceKey] = nonce;
        }
    }
}
