using PushNotificationService.Endpoints;
using PushNotificationService.Implementations;
using PushNotificationService.Interfaces;
using Microsoft.AspNetCore.Builder; // Add this using directive for CORS
using Microsoft.Extensions.DependencyInjection; // Add this using directive for CORS

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IServerSentEventService, ServerSentEventService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:4200")
                          //.AllowCredentials()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS policy
app.UseCors("AllowSpecificOrigin");

app.MapNotificationSendEndpoints();
app.MapNotificationSubscriptionEndpoints();

app.Run();
