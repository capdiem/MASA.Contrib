﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Globalization.I18n.Dcc.Tests;

[TestClass]
// ReSharper disable once InconsistentNaming
public class I18nTest
{
    private const string DEFAULT_RESOURCE = "Resources/I18n";

    [TestInitialize]
    public void Initialize()
    {
        I18nResourceResourceConfiguration.Resources = new();
    }

    [DataTestMethod]
    [DataRow("zh-CN", "吉姆")]
    [DataRow("en-US", "JIM")]
    public void Test(string cultureName, string expectedValue)
    {
        var appId = "appid";
        var configObjectPrefix = "Culture";
        var key = "key";
        var services = new ServiceCollection();
        MasaApp.SetServiceCollection(services);
        Mock<IMasaConfiguration> masaConfiguration = new();
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new List<KeyValuePair<string, string>>()
        {
            new($"{configObjectPrefix}.{cultureName}:{key}", expectedValue)
        });
        var configuration = configurationBuilder.Build();
        masaConfiguration.Setup(config => config.ConfigurationApi.Get(appId)).Returns(configuration);
        services.AddSingleton(masaConfiguration.Object);
        services.AddI18n(options =>
        {
            options.ResourcesDirectory = DEFAULT_RESOURCE;
        }, options => options.UseDcc(appId, configObjectPrefix));
        MasaApp.SetServiceCollection(services);

        var serviceProvider = services.BuildServiceProvider();
        var i18n = serviceProvider.GetService<II18n>();
        Assert.IsNotNull(i18n);
        i18n.SetUiCulture(cultureName);
        var value = i18n.T(key);
        Assert.AreEqual(expectedValue, value);
    }

    [DataTestMethod]
    [DataRow("appid", "appid2", "zh-CN", "key", "吉姆")]
    [DataRow("appid", "appid2", "en-US", "key", "JIM")]
    public void Test2(string appId, string appId2, string cultureName, string key, string expectedValue)
    {
        var configObjectPrefix = "Culture";
        var services = new ServiceCollection();
        MasaApp.SetServiceCollection(services);
        Mock<IMasaConfiguration> masaConfiguration = new();
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new List<KeyValuePair<string, string>>()
        {
            new($"{configObjectPrefix}.{cultureName}:{key}", expectedValue),
            new($"{configObjectPrefix}2.{cultureName}:{key}", $"{expectedValue}2")
        });
        var configuration = configurationBuilder.Build();
        masaConfiguration.Setup(config => config.ConfigurationApi.Get(appId)).Returns(configuration);
        masaConfiguration.Setup(config => config.ConfigurationApi.Get(appId2)).Returns(configuration);
        services.AddSingleton(masaConfiguration.Object);

        services.Configure<MasaI18nOptions>(options =>
        {
            options.Resources.Add<CustomResource>().UseDcc(appId2, $"{configObjectPrefix}2");
        });
        services.AddI18n(options =>
        {
            options.ResourcesDirectory = DEFAULT_RESOURCE;
        }, options => options.UseDcc(appId, configObjectPrefix));

        MasaApp.SetServiceCollection(services);

        var serviceProvider = services.BuildServiceProvider();
        var i18n = serviceProvider.GetService<II18n>();
        Assert.IsNotNull(i18n);
        i18n.SetUiCulture(cultureName);
        var value = i18n.T(key);
        Assert.AreEqual(expectedValue, value);

        var customI18n = serviceProvider.GetService<II18n<CustomResource>>();
        Assert.IsNotNull(customI18n);
        customI18n.SetUiCulture(cultureName);
        var value2 = customI18n.T(key);
        Assert.AreEqual($"{expectedValue}2", value2);
    }
}
