using Microsoft.EntityFrameworkCore;
using om_svc_customer.Data;
using om_svc_customer.Services;
using StackExchange.Redis;


namespace om_svc_customer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CustomerDbContext>(opt =>
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                opt.UseNpgsql(connectionString);
            });
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var redisConnection = Configuration.GetConnectionString("RedisCache");
            if(redisConnection == null) 
            {
                throw new NullReferenceException("Cannot find Redis Connection string ");
            }

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "Customers_";
            });

            var redisConfiguration = ConfigurationOptions.Parse(redisConnection); 
            
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConfiguration));

            services.AddScoped<ICacheService, RedisCacheService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsStaging())
            {
                //app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
