namespace WebApplicationRedis.Domain
{
    public interface IRedisCacheRepository
    {
        Task<T> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T value, TimeSpan timeSpan);

        void Delete(string cacheKey);
    }
}
