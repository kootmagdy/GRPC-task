using InventoryService.Handler;
using InventoryService.Services;
using Microsoft.AspNetCore.Authentication;

const string Key = "X-Api-Key";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Key;
}).AddScheme<AuthenticationSchemeOptions, ApuAuthHandler>
(Key,configureOptions => { });
var app = builder.Build();
app.MapGrpcService<InventoryServiceFunctions>();
//app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<AuthService>();
app.Run();