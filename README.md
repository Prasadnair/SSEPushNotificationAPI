# SSEPushNotificationAPI
Push Notification using Server-Sent-Event in .NET
# Setup:
SQL Table to store active connections (simplified schema).
CREATE TABLE ClientConnections (
    ConnectionId UNIQUEIDENTIFIER PRIMARY KEY,
    AppId NVARCHAR(100),
    UserId NVARCHAR(100),
    IsActive BIT
);

In-memory storage (ConcurrentDictionary) for active connections.
Web API endpoints for subscribing and publishing notifications.
