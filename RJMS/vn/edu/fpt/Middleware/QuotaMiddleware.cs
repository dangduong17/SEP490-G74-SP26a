using Microsoft.AspNetCore.Http;
using RJMS.Vn.Edu.Fpt.Service;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace RJMS.vn.edu.fpt.Middleware
{
    public class QuotaMiddleware
    {
        private readonly RequestDelegate _next;

        public QuotaMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ISubscriptionService subscriptionService)
        {
            var routeData = context.GetRouteData();
            var controller = routeData.Values["controller"]?.ToString();
            var action = routeData.Values["action"]?.ToString();
            var method = context.Request.Method;

            // ── QUOTA CHECK LOGIC ──
            if (method == "POST")
            {
                string? featureCode = null;

                // Example: Recruiters creating jobs
                if (controller == "Job" && action == "Create")
                {
                    featureCode = "JOB_POSTING";
                }
                // Example: AI CV Processing (if applicable)
                else if ((controller == "CV" || controller == "Home") && action == "ProcessAiCv")
                {
                    featureCode = "CV_AI_FILTER";
                }

                if (featureCode != null)
                {
                    var userIdStr = context.Request.Cookies["UserId"];
                    if (int.TryParse(userIdStr, out int userId))
                    {
                        var quotaResult = await subscriptionService.CheckQuotaAsync(userId, featureCode);
                        if (!quotaResult.Allowed)
                        {
                            context.Response.Redirect($"/Subscription/Index?error={System.Net.WebUtility.UrlEncode(quotaResult.Message)}");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
