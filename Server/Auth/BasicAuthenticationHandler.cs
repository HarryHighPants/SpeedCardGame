namespace Server.Auth
{
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;

    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock
        ) : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var accessToken =
                Request.Query["access_token"].FirstOrDefault()
                ?? Request.Headers["Authorization"]
                    .FirstOrDefault()
                    ?.Split("Bearer ")
                    .Skip(1)
                    .FirstOrDefault();

            if (accessToken == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("No Access token specified!"));
            }

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, accessToken) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;

            Response.Headers.Append(HeaderNames.WWWAuthenticate, "No user token supplied!");

            return Task.CompletedTask;
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;

            Response.Headers.Append(HeaderNames.WWWAuthenticate, "No user token supplied!");

            return Task.CompletedTask;
        }
    }
}
