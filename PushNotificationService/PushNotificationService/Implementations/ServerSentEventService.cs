using Dapper;
using PushNotificationService.Interfaces;
using System.Collections.Concurrent;
using System.Data.SqlClient;

namespace PushNotificationService.Implementations
{
    public class ServerSentEventService : IServerSentEventService
    {
        private static readonly ConcurrentDictionary<Guid, HttpResponse> _sseClients = new();
        private readonly string _connectionString;

        public ServerSentEventService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SSEDatabase");
        }
        /// <summary>
        /// Send a notification to the client
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task SendNotificationToClientsAsync(string appId, string userId, string message)
        {
            var query = "SELECT ConnectionId FROM ClientConnections WHERE AppId = @AppId AND UserId = @UserId AND IsActive = 1";

            using var connection = new SqlConnection(_connectionString);
            var connectionIds = await connection.QueryAsync<Guid>(query, new { AppId = appId, UserId = userId });

            foreach (var connectionId in connectionIds)
            {
                await SendMessageToClient(connectionId, message);
            }
        }

        /// <summary>
        /// Subscribe to notifications
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task SubscribeToNotificationsAsync(string appId, string userId, HttpContext context)
        {
            var response = context.Response?? context.Response;
            response?.Headers?.TryAdd("Cache-Control", "no-cache");
            response?.Headers?.TryAdd("Content-Type", "text/event-stream");

            var connectionId = Guid.NewGuid();

            // Save connection metadata in SQL
            await AddClientConnection(appId, userId, connectionId);

            // Save active response in memory
            _sseClients.TryAdd(connectionId, response);

            try
            {
                while (!context.RequestAborted.IsCancellationRequested)
                {
                    // Send ping to keep connection alive
                    await Task.Delay(10000);
                    await SendMessageToClient(connectionId, "ping");
                }
            }
            finally
            {
                await RemoveClientConnection(connectionId);
                _sseClients.TryRemove(connectionId, out _);
            }
        }

        private async Task AddClientConnection(string appId, string userId, Guid connectionId)
        {
            var query = "INSERT INTO ClientConnections (AppId, UserId, ConnectionId, IsActive) VALUES (@AppId, @UserId, @ConnectionId, 1)";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, new { AppId = appId, UserId = userId, ConnectionId = connectionId });
        }

        private async Task RemoveClientConnection(Guid connectionId)
        {
            var query = "UPDATE ClientConnections SET IsActive = 0 WHERE ConnectionId = @ConnectionId";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, new { ConnectionId = connectionId });
        }

        private async Task SendMessageToClient(Guid connectionId, string message)
        {
            if (_sseClients.TryGetValue(connectionId, out var response))
            {
                var formattedMessage = $"data: {message}\n\n";
                var messageBytes = System.Text.Encoding.UTF8.GetBytes(formattedMessage);

                try
                {
                    await response.Body.WriteAsync(messageBytes, 0, messageBytes.Length);
                    await response.Body.FlushAsync();
                }
                catch
                {
                    await RemoveClientConnection(connectionId);
                    _sseClients.TryRemove(connectionId, out _);
                }
            }
        }

    }
}
