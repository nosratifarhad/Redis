using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Runtime;
using WebApplicationRedis.Domain;
using WebApplicationRedis.Helpers;
using WebApplicationRedis.Infra.repositories;
using WebApplicationRedis.Infra.repositories.RedisCacheRepositorys;
using WebApplicationRedis.Services;
using WebApplicationRedis.Services.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region DIC

string connection = builder.Configuration.GetValue<string>("RedisConfiguration:ConnectionString");

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(connection));

builder.Services.AddScoped<IProductServices, ProductServices>();
builder.Services.AddTransient<IRedisCacheRepository, RedisCacheRepository>();
builder.Services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
builder.Services.AddScoped<IProductReadRepository, ProductReadRepository>();

#endregion DIC

#region Add Redis Option 

builder.Services.AddOptions<RedisConnectionOption>()
    .Bind(builder.Configuration.GetSection("RedisConfiguration"))
    .ValidateDataAnnotations();

builder.Services.Configure<RedisSettingOption>(options => builder.Configuration.GetSection("RedisSetting").Bind(options));

#endregion Add Redis Option 

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
