# Redis


```csharp
builder.Services.AddOptions<RedisOption>()
    .Bind(builder.Configuration.GetSection("RedisConfiguration"))
    .ValidateDataAnnotations();

```
