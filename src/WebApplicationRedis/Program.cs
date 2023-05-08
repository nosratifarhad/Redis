using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Runtime;
using WebApplicationRedis.Domain;
using WebApplicationRedis.Helpers;
using WebApplicationRedis.Repositorys;
using WebApplicationRedis.Repositorys.RedisCacheRepositorys;
using WebApplicationRedis.Services;
using WebApplicationRedis.Services.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region DIC


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("RedisConfiguration:ConnectionString");
});

//builder.Services.AddScoped<IDatabase>(cfg =>
//{
//    IConnectionMultiplexer multiplexer = 
//    ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("RedisConfiguration:ConnectionString"));
//    return multiplexer.GetDatabase();
//});


builder.Services.AddScoped<IProductServices, ProductServices>();
builder.Services.AddScoped<IConnectionMultiplexer, ConnectionMultiplexer>();
builder.Services.AddScoped<IRedisCacheRepository, RedisCacheRepository>();
builder.Services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
builder.Services.AddScoped<IProductReadRepository, ProductReadRepository>();


#endregion DIC

#region Add Redis Services 

builder.Services.AddOptions<RedisOption>()
    .Bind(builder.Configuration.GetSection("RedisConfiguration"))
    .ValidateDataAnnotations();

builder.Services.Configure<RedisSettingOption>(options => builder.Configuration.GetSection("RedisSetting").Bind(options));

#endregion Add Redis Services 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
