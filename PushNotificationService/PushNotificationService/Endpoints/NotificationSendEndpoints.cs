using Microsoft.AspNetCore.Mvc;
using PushNotificationService.Interfaces;

namespace PushNotificationService.Endpoints
{
    public static class NotificationSendEndpoints
    {
        public static void MapNotificationSendEndpoints(this WebApplication app)
        {
            app.MapPost("api/notifications/send/{appId}/{userId}", async ([FromRoute]string appId,[FromRoute] string userId, IServerSentEventService sseService, HttpContext context) =>
            {
                if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(userId))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                var message = "validating the SSE"; //await new StreamReader(context.Request.Body).ReadToEndAsync();
                await sseService.SendNotificationToClientsAsync(appId, userId, message);
            });
        }
    }
}
