using Microsoft.Extensions.Options;
using WebApplicationRedis.Helpers;
using StackExchange.Redis;
using Newtonsoft.Json;
using WebApplicationRedis.Domain;

namespace WebApplicationRedis.Infra.Repositories.CacheRepositories.RedisCacheRepositories
{
    public class RedisCacheRepository : IRedisCacheRepository
    {
        #region Fields
        private readonly IDatabase _database;
        private readonly RedisConnectionOption _option;
        #endregion Fields

        #region Ctor
        public RedisCacheRepository(IConnectionMultiplexer connection,
                                    IOptions<RedisConnectionOption> option)
        {
            _option = option.Value;
            _database = connection.GetDatabase(_option.DataBaseNumber);
        }

        #endregion Ctor

        #region Methods

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

        #endregion Methods
    }
}
