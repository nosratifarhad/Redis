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
### Add Redis Cache Repository

```csharp
public async Task<T> GetAsync<T>(string key)
{
    var redisValue = await _database.StringGetAsync(key);
    if (string.IsNullOrWhiteSpace(redisValue) ||
        string.IsNullOrEmpty(redisValue))
        return default;

    return JsonConvert.DeserializeObject<T>(redisValue);
}

public async Task SetAsync<T>(string key, T value, TimeSpan timeSpan)
{
    var redisValue = JsonConvert.SerializeObject(value);

    await _database.StringSetAsync(key, redisValue, timeSpan);
}

public void Delete(string cacheKey)
{
    _database.KeyDelete(cacheKey);
}
```
