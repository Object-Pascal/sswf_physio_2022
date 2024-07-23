using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Web_App.Session
{
    public static class SessionState
    {
        public static bool IsUserConsent(HttpContext context)
        {
            ITrackingConsentFeature consentFeature = context.Features.Get<ITrackingConsentFeature>();
            return consentFeature?.CanTrack ?? false;
        }
    }
}