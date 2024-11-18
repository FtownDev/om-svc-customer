using Microsoft.AspNetCore.Hosting;
using om_svc_customer;

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);

app.Run();
