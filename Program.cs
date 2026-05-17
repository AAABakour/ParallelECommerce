using ParallelECommerce.Endpoints;
using ParallelECommerce.Middleware;
using ParallelECommerce.Services;

var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI/Swagger services
builder.Services.AddOpenApi();

// Register services
builder.Services.AddSingleton<PerformanceMetricsService>();
builder.Services.AddSingleton<ResourceMonitoringService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<ResourceMonitoringService>());

builder.Services.AddSingleton<InventoryService>();
builder.Services.AddSingleton<CapacityControlService>();
builder.Services.AddSingleton<NotificationQueueService>();
builder.Services.AddHostedService<NotificationWorkerService>();
builder.Services.AddSingleton<BatchProcessingService>();
builder.Services.AddHostedService<BatchJobWorkerService>();
builder.Services.AddSingleton<LoadBalancingService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ParallelECommerce API v1");
    });
}

// System health endpoint
app.MapGet("/", () => Results.Ok(new
{
    project = "High-Performance E-Commerce Backend Engine",
    status = "Running",
    message = "Parallel E-Commerce API is ready"
}))
.WithName("HealthCheck")
.WithTags("System");

// AOP-style cross-cutting performance monitoring for all API requests.
app.UseMiddleware<PerformanceMonitoringMiddleware>();

// Register endpoint groups
app.MapMonitoringEndpoints();
app.MapInventoryEndpoints();
app.MapCapacityControlEndpoints();
app.MapAsyncProcessingEndpoints();
app.MapBatchProcessingEndpoints();
app.MapLoadBalancingEndpoints();
app.Run();