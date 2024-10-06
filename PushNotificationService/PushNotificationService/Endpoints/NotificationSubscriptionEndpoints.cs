using Microsoft.AspNetCore.Mvc;
using PushNotificationService.Interfaces;

namespace PushNotificationService.Endpoints
{
    public static class NotificationSubscriptionEndpoints
    {
        public static void MapNotificationSubscriptionEndpoints(this WebApplication app)
        {
            app.MapGet("api/notifications/subscribe/{appId}/{userId}", async ([FromRoute] string appId,[FromRoute] string userId,IServerSentEventService sseService, HttpContext context) =>
            {               

                if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(userId))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                await sseService.SubscribeToNotificationsAsync(appId, userId, context);
            });
        }
    }
}
