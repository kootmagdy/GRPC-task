using Client;
using Client.Proto;
using Client.Services;
using Grpc.Net.Client;

var builder = WebApplication.CreateBuilder(args);

var _configuration = builder.Configuration;
builder.Services.AddControllers();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddGrpcClient<Inventory.InventoryClient>(options =>
{
    var address = _configuration.GetValue<string>(Declartions.GrpcServiceAddressSettingName);
    options.Address = new Uri(address);
}).AddCallCredentials((context, metadata, serviceProvider) =>
{
    var apiKeyProvider = serviceProvider.GetRequiredService<IAuthService>();
    var apiKey = apiKeyProvider.GetApiKey();
    metadata.Add(Declartions.ApiKeyHeaderName, apiKey); ;
    return Task.CompletedTask;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
