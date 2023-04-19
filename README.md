# Redis

### Add Redis Services 

```csharp
builder.Services.AddOptions<RedisOption>()
    .Bind(builder.Configuration.GetSection("RedisConfiguration"))
    .ValidateDataAnnotations();
```
### RedisOption
```csharp
    public class RedisOption
    {
        public string ConnectionString { get; set; }

        public int DataBaseNumber { get; set; }
    }
```
