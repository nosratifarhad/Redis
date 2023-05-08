# Redis

### First Install Package
```
StackExchange.Redis
```
### Add RedisConfiguration And RedisSetting In appsettings.json File 
```json
"RedisConfiguration": {
  "ConnectionString": "127.0.0.1:6379",
  "DataBaseNumber": 0
},
"RedisSetting": {
  "RedisKey": "**********",
  "CacheTimeOut": 180
}
```

### Add Redis Options

```csharp
builder.Services.AddOptions<RedisConnectionOption>()
    .Bind(builder.Configuration.GetSection("RedisConfiguration"))
    .ValidateDataAnnotations();

builder.Services.Configure<RedisSettingOption>(options => builder.Configuration.GetSection("RedisSetting").Bind(options));
```

### Redis Options
```csharp
public class RedisOption
{
    public string ConnectionString { get; set; }

    public int DataBaseNumber { get; set; }
}

public class RedisSettingOption
{
    public string RedisKey { get; set; }

    public int CacheTimeOut { get; set; }
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

### You can Use This Methods in Services 
### Note : I User "ValueTask" for Get (Query) Methods.
And you can see this [link](https://github.com/dotnet/efcore/blob/main/src/EFCore/Internal/EntityFinder.cs) ef using this key For "FindAsync" Method .
### Exemple :
```csharp
 public async ValueTask<ProductViewModel> GetProductAsync(int productId)
 {
     if (productId <= 0)
         throw new NullReferenceException("Product Id Is Invalid");

     string cacheKey = _redisSettingOption.RedisKey;
     int cacheTimeOut = _redisSettingOption.CacheTimeOut;

     var cacheResult = await GetFromCacheAsync<ProductViewModel>(cacheKey);
     if (cacheResult != null)
         return cacheResult;

     var productDto = await _productReadRepository.GetProductAsync(productId).ConfigureAwait(false);
     if (productDto == null)
         return new ProductViewModel();

     var productViewModel = CreateProductViewModelFromProductDto(productDto);

     await SetInToCacheAsync(cacheKey, productViewModel, cacheTimeOut).ConfigureAwait(false);

     return productViewModel;
 }

 public async Task<int> CreateProductAsync(CreateProductInputModel inputModel)
 {
     if (inputModel == null)
         throw new NullReferenceException("Product Id Is Invalid");

     string cacheKey = _redisSettingOption.RedisKey;
     int cacheTimeOut = _redisSettingOption.CacheTimeOut;

     ValidateProductName(inputModel.ProductName);

     ValidateProductTitle(inputModel.ProductTitle);

     var productEntoty = CreateProductEntityFromInputModel(inputModel);

     int productId = await _productWriteRepository.CreateProductAsync(productEntoty).ConfigureAwait(false);

     productEntoty.setProductId(productId);

     await SetInToCacheAsync(cacheKey, productEntoty, cacheTimeOut).ConfigureAwait(false);

     return productId;

 }

private void DeleteCache(string key)
   => _redisCacheRepository.Delete(key);

private async Task SetInToCacheAsync<T>(string key, T? result, int cacheTimeOut)
    => await _redisCacheRepository
         .SetAsync(key, result, TimeSpan.FromMinutes(cacheTimeOut));

private async Task<T> GetFromCacheAsync<T>(string cacheKey)
    => await _redisCacheRepository
        .GetAsync<T>(cacheKey);
```
### EF using "ValueTask" for this mthod because this method use cache tracked .
```csharp
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueTask<TEntity?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken = default)
    {
        if (keyValues == null
            || keyValues.Any(v => v == null))
        {
            return default;
        }

        var (processedKeyValues, ct) = ValidateKeyPropertiesAndExtractCancellationToken(keyValues!, async: true, cancellationToken);

        var tracked = FindTracked(processedKeyValues);
        return tracked != null // this line <=======
            ? new ValueTask<TEntity?>(tracked) // this line <=======
            : new ValueTask<TEntity?>( // this line <=======
                _queryRoot.FirstOrDefaultAsync(BuildLambda(_primaryKey.Properties, new ValueBuffer(processedKeyValues)), ct)); // this line
    }
```

## Good luck ;)

