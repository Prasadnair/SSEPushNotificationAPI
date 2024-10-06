namespace PushNotificationService.Interfaces
{
    public interface IServerSentEventService
    {
        Task SubscribeToNotificationsAsync(string appId, string userId, HttpContext context);
        Task SendNotificationToClientsAsync(string appId, string userId, string message);
    }
}
