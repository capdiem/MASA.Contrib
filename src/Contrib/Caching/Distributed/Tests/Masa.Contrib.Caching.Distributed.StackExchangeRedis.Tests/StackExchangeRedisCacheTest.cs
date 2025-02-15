// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Caching.Distributed.StackExchangeRedis.Tests;

#pragma warning disable CS0618
[TestClass]
public class StackExchangeRedisCacheTest : TestBase
{
    [TestMethod]
    public void TestAddStackExchangeRedisCache()
    {
        var services = new ServiceCollection();
        services.AddDistributedCache(distributedCacheOptions => distributedCacheOptions.UseStackExchangeRedisCache(option =>
        {
            option.DefaultDatabase = 1;
            option.Servers = new List<RedisServerOptions>()
            {
                new(REDIS_HOST)
            };
            option.GlobalCacheOptions = new CacheOptions()
            {
                CacheKeyType = CacheKeyType.None
            };
        }));

        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetService<IOptions<RedisConfigurationOptions>>();
        Assert.IsNotNull(options);
        Assert.AreEqual(1, options.Value.DefaultDatabase);
        Assert.AreEqual(1, options.Value.Servers.Count);
        Assert.AreEqual(REDIS_HOST, options.Value.Servers[0].Host);
        Assert.AreEqual(6379, options.Value.Servers[0].Port);

        var distributedCacheClient = serviceProvider.GetService<IDistributedCacheClient>();
        Assert.IsNotNull(distributedCacheClient);
        string key = "test_key";
        distributedCacheClient.Set(key, "content");
        Assert.IsTrue(distributedCacheClient.Exists(key));
        distributedCacheClient.Remove(key);
    }

    [TestMethod]
    public void TestAddMultiStackExchangeRedisCache()
    {
        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache("test", option =>
        {
            option.DefaultDatabase = 1;
            option.Servers = new List<RedisServerOptions>()
            {
                new(REDIS_HOST)
            };
            option.GlobalCacheOptions = new CacheOptions()
            {
                CacheKeyType = CacheKeyType.None
            };
        });
        services.AddStackExchangeRedisCache("test2", new RedisConfigurationOptions()
        {
            DefaultDatabase = 2,
            Servers = new List<RedisServerOptions>()
            {
                new(REDIS_HOST)
            },
            GlobalCacheOptions = new CacheOptions()
            {
                CacheKeyType = CacheKeyType.None
            }
        });
        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<IDistributedCacheClientFactory>();
        var distributedCacheClient = factory.Create("test");
        Assert.IsNotNull(distributedCacheClient);
        string key = "test_key";
        distributedCacheClient.Set(key, "content");
        Assert.IsTrue(distributedCacheClient.Exists(key));

        var distributedCacheClient2 = factory.Create("test2");
        Assert.IsFalse(distributedCacheClient2.Exists(key));
        distributedCacheClient2.Set(key + "2", "content_2");

        Assert.AreEqual("content_2", distributedCacheClient2.Get<string>(key + "2"));
        Assert.AreEqual(null, distributedCacheClient.Get<string>(key + "2"));
        distributedCacheClient.Remove(key);
        distributedCacheClient2.Remove(key + "2");
    }

    [TestMethod]
    public void TestAddStackExchangeRedisCacheByAppsettings()
    {
        var builder = WebApplication.CreateBuilder();
        var rootPath = builder.Environment.ContentRootPath;
        var services = builder.Services;
        services.AddStackExchangeRedisCache("test");

        var serviceProvider = services.BuildServiceProvider();
        var distributedCacheClient = serviceProvider.GetRequiredService<IDistributedCacheClient>();
        string key = "test_1";
        distributedCacheClient.Set(key, "test_content");
        Assert.IsTrue(distributedCacheClient.Exists(key));

        var oldContent = File.ReadAllText(Path.Combine(rootPath, "appsettings.json"));
        File.WriteAllText(Path.Combine(rootPath, "appsettings.json"),
            JsonSerializer.Serialize(new
            {
                RedisConfig = new RedisConfigurationOptions()
                {
                    Servers = new List<RedisServerOptions>()
                    {
                        new(REDIS_HOST, 6379)
                    },
                    DefaultDatabase = 1
                }
            }));

        Task.Delay(3000).ConfigureAwait(false).GetAwaiter().GetResult();
        distributedCacheClient = serviceProvider.GetRequiredService<IDistributedCacheClientFactory>().Create();

        var exist = distributedCacheClient.Exists(key);

        Assert.IsFalse(exist);

        File.WriteAllText(Path.Combine(Path.Combine(rootPath, "appsettings.json")), oldContent);

        Task.Delay(3000).ConfigureAwait(false).GetAwaiter().GetResult();

        distributedCacheClient.Remove(key);
    }

    [TestMethod]
    public void TestAddStackExchangeRedisCacheRepeat()
    {
        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(options =>
        {
            options.DefaultDatabase = 1;
            options.Servers = new List<RedisServerOptions>()
            {
                new(REDIS_HOST)
            };
            options.GlobalCacheOptions = new CacheOptions()
            {
                CacheKeyType = CacheKeyType.None
            };
        });
        services.AddStackExchangeRedisCache(new RedisConfigurationOptions()
        {
            DefaultDatabase = 2,
            Servers = new List<RedisServerOptions>()
            {
                new(REDIS_HOST)
            },
            GlobalCacheOptions = new CacheOptions()
            {
                CacheKeyType = CacheKeyType.None
            }
        });
        var serviceProvider = services.BuildServiceProvider();

        var distributedCacheClient = serviceProvider.GetService<IDistributedCacheClient>();
        Assert.IsNotNull(distributedCacheClient);

        Assert.IsTrue(distributedCacheClient is RedisCacheClient redisClient);
        var fieldInfo = typeof(RedisCacheClient).GetField("Db", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(fieldInfo);

        var value = fieldInfo.GetValue((RedisCacheClient)distributedCacheClient);
        Assert.IsNotNull(value);
        Assert.AreEqual(1, ((IDatabase)value).Database);
    }

    [TestMethod]
    public void TestAddStackExchangeRedisCacheRepeatByConfiguration()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddStackExchangeRedisCache();
        builder.Services.AddStackExchangeRedisCache(Options.DefaultName, "RedisConfig2");
        builder.Services.AddStackExchangeRedisCache(Options.DefaultName, "RedisConfig3");
        var serviceProvider = builder.Services.BuildServiceProvider();

        var distributedCacheClient = serviceProvider.GetService<IDistributedCacheClient>();
        Assert.IsNotNull(distributedCacheClient);

        Assert.IsTrue(distributedCacheClient is RedisCacheClient redisClient);
        var fieldInfo = typeof(RedisCacheClient).GetField("Db", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(fieldInfo);

        var value = fieldInfo.GetValue((RedisCacheClient)distributedCacheClient);
        Assert.IsNotNull(value);
        Assert.AreEqual(6, ((IDatabase)value).Database);
    }

    [TestMethod]
    public void TestCachingBuilder()
    {
        var services = new ServiceCollection();
        var cachingBuilder = services.AddStackExchangeRedisCache(options =>
        {
            options.Servers = new List<RedisServerOptions>()
            {
                new()
            };
        });
        Assert.AreEqual(Options.DefaultName, cachingBuilder.Name);
        Assert.AreEqual(services, cachingBuilder.Services);

        cachingBuilder = services.AddStackExchangeRedisCache("test", options =>
        {
            options.Servers = new List<RedisServerOptions>()
            {
                new()
            };
        });
        Assert.AreEqual("test", cachingBuilder.Name);
    }

    [TestMethod]
    public void TestFormatCacheKey()
    {
        var services = new ServiceCollection();
        services.AddDistributedCache("test", distributedCacheOptions => distributedCacheOptions.UseStackExchangeRedisCache(options =>
        {
            options.Servers = new List<RedisServerOptions>()
            {
                new()
            };
            options.GlobalCacheOptions = new CacheOptions()
            {
                CacheKeyType = CacheKeyType.TypeName
            };
        }));

        services.AddDistributedCache("test2", distributedCacheOptions => distributedCacheOptions.UseStackExchangeRedisCache(options =>
        {
            options.Servers = new List<RedisServerOptions>()
            {
                new(),
            };
            options.DefaultDatabase = 0;
            options.GlobalCacheOptions = new CacheOptions()
            {
                CacheKeyType = CacheKeyType.None
            };
        }));
        var serviceProvider = services.BuildServiceProvider();
        var distributedCacheClientFactory = serviceProvider.GetRequiredService<IDistributedCacheClientFactory>();
        var key = "redisConfig";
        var distributedCacheClient = distributedCacheClientFactory.Create("test");
        Assert.IsNotNull(distributedCacheClient);
        distributedCacheClient.Remove<string>(key);
        distributedCacheClient.Set(key, "redis configuration json");
        var value = distributedCacheClient.Get<string>(key);
        Assert.AreEqual("redis configuration json", value);

        var distributedCacheClient2 = distributedCacheClientFactory.Create("test2");
        Assert.IsNotNull(distributedCacheClient2);

        Assert.IsFalse(distributedCacheClient2.Exists(key));
        Assert.IsFalse(distributedCacheClient2.Exists<string>(key));

        distributedCacheClient2.Set(key, "redis configuration2 json");
        var value2 = distributedCacheClient2.Get<string>(key);
        Assert.AreEqual("redis configuration2 json", value2);

        distributedCacheClient.Remove<string>(key);
        distributedCacheClient2.Remove<string>(key);
    }

    [TestMethod]
    public void TestFormatCacheKeyByTypeNameAlias()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDistributedCache("test", distributedCacheOptions =>
        {
            distributedCacheOptions.UseStackExchangeRedisCache("RedisConfig4");
        });
        var serviceProvider = builder.Services.BuildServiceProvider();
        var distributedCacheClient = serviceProvider.GetRequiredService<IDistributedCacheClientFactory>().Create("test");
        Assert.IsNotNull(distributedCacheClient);

        Assert.ThrowsException<NotImplementedException>(() =>
        {
            distributedCacheClient.GetOrSet("redisConfiguration", () => new CacheEntry<string>("redis configuration2 json"));
        });
    }

    [TestMethod]
    public void TestFormatCacheKeyByTypeNameAlias2()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDistributedCache("test", distributedCacheOptions =>
        {
            distributedCacheOptions.UseStackExchangeRedisCache("RedisConfig4");
        });
        builder.Services.Configure("test", (TypeAliasOptions options) =>
        {
            options.GetAllTypeAliasFunc = () => new Dictionary<string, string>()
            {
                { "String", "s" }
            };
        });
        var serviceProvider = builder.Services.BuildServiceProvider();
        var distributedCacheClient = serviceProvider.GetRequiredService<IDistributedCacheClientFactory>().Create("test");
        Assert.IsNotNull(distributedCacheClient);

        var value = distributedCacheClient.GetOrSet("redisConfiguration", () => new CacheEntry<string>("redis configuration2 json"));
        Assert.AreEqual("redis configuration2 json", value);
        distributedCacheClient.Remove<string>("redisConfiguration");
    }

    [TestMethod]
    public void TestFormatCacheKeyByTypeNameAlias3()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDistributedCache(
            distributedCacheOptions =>
            {
                distributedCacheOptions.UseStackExchangeRedisCache("RedisConfig4");
            },
            typeAliasOptions =>
            {
                typeAliasOptions.GetAllTypeAliasFunc = () => new Dictionary<string, string>()
                {
                    { "String", "s" }
                };
            });
        builder.Services.Configure((TypeAliasOptions options) =>
        {
            options.GetAllTypeAliasFunc = () => new Dictionary<string, string>()
            {
                { "String", "s" }
            };
        });
        var serviceProvider = builder.Services.BuildServiceProvider();
        var distributedCacheClient = serviceProvider.GetRequiredService<IDistributedCacheClientFactory>().Create();
        Assert.IsNotNull(distributedCacheClient);

        var value = distributedCacheClient.GetOrSet("redisConfiguration", () => new CacheEntry<string>("redis configuration2 json"));
        Assert.AreEqual("redis configuration2 json", value);
        distributedCacheClient.Remove<string>("redisConfiguration");
    }

    [TestMethod]
    public void TestFormatCacheKeyByTypeNameAlias4()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDistributedCache(
            distributedCacheOptions =>
            {
                distributedCacheOptions.UseStackExchangeRedisCache(builder.Configuration.GetSection("RedisConfig4"));
            },
            typeAliasOptions =>
            {
                typeAliasOptions.GetAllTypeAliasFunc = () => new Dictionary<string, string>()
                {
                    { "String", "s" }
                };
            });
        builder.Services.Configure((TypeAliasOptions options) =>
        {
            options.GetAllTypeAliasFunc = () => new Dictionary<string, string>()
            {
                { "String", "s" }
            };
        });
        var serviceProvider = builder.Services.BuildServiceProvider();
        var distributedCacheClient = serviceProvider.GetRequiredService<IDistributedCacheClientFactory>().Create();
        Assert.IsNotNull(distributedCacheClient);

        var value = distributedCacheClient.GetOrSet("redisConfiguration", () => new CacheEntry<string>("redis configuration2 json"));
        Assert.AreEqual("redis configuration2 json", value);
        distributedCacheClient.Remove<string>("redisConfiguration");
    }
}
#pragma warning restore CS0618
