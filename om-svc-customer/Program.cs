using Microsoft.AspNetCore.Hosting;
using om_svc_customer;
using om_svc_customer.Data;

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();
