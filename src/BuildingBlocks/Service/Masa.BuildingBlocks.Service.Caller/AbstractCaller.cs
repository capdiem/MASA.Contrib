// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.BuildingBlocks.Service.Caller;

public abstract class AbstractCaller : ICaller
{
    private readonly ITypeConvertor _typeConvertor;
    protected readonly IServiceProvider ServiceProvider;

    private IRequestMessage? _requestMessage;
    private IResponseMessage? _responseMessage;
    protected IRequestMessage RequestMessage => _requestMessage ??= ServiceProvider.GetRequiredService<IRequestMessage>();
    protected IResponseMessage ResponseMessage => _responseMessage ??= ServiceProvider.GetRequiredService<IResponseMessage>();
    protected Func<HttpRequestMessage, Task>? RequestMessageFunc;

    protected AbstractCaller(IServiceProvider serviceProvider)
    {
        _typeConvertor = serviceProvider.GetRequiredService<ITypeConvertor>();
        ServiceProvider = serviceProvider;
    }

    public virtual void ConfigRequestMessage(Func<HttpRequestMessage, Task> func)
    {
        RequestMessageFunc = func;
    }

    public virtual async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(request, cancellationToken);
        if (autoThrowException)
            await ResponseMessage.ProcessResponseAsync(response, cancellationToken);

        return response;
    }

    public abstract Task<TResponse?> SendAsync<TResponse>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default);

    public abstract Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default);

    public virtual async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string? methodName,
        HttpContent? content,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = await CreateRequestAsync(method, methodName);
        request.Content = content;
        return await SendAsync(request, autoThrowException, cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> SendAsync<TRequest>(
        HttpMethod method,
        string? methodName,
        TRequest data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(method, methodName, data);
        return await SendAsync(request, autoThrowException, cancellationToken);
    }

    public virtual async Task<TResponse?> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string? methodName,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(method, methodName, data);
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public abstract Task<HttpRequestMessage> CreateRequestAsync(
        HttpMethod method,
        string? methodName);

    public abstract Task<HttpRequestMessage> CreateRequestAsync<TRequest>(
        HttpMethod method,
        string? methodName,
        TRequest data);

    public abstract Task SendGrpcAsync(
        string methodName,
        CancellationToken cancellationToken = default);

    public abstract Task<TResponse> SendGrpcAsync<TResponse>(
        string methodName,
        CancellationToken cancellationToken = default)
        where TResponse : IMessage, new();

    public abstract Task SendGrpcAsync<TRequest>(
        string methodName,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IMessage;

    public abstract Task<TResponse> SendGrpcAsync<TRequest, TResponse>(
        string methodName,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IMessage
        where TResponse : IMessage, new();

    public virtual async Task<string> GetStringAsync(
        string? methodName,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, methodName);
        var content = await SendAsync(request, autoThrowException, cancellationToken);
        return await content.Content.ReadAsStringAsync(cancellationToken);
    }

    public virtual Task<string> GetStringAsync<TRequest>(
        string? methodName,
        TRequest data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default) where TRequest : class
        => GetStringAsync(
            GetUrl(methodName, _typeConvertor.ConvertToKeyValuePairs(data)),
            autoThrowException,
            cancellationToken);

    public virtual Task<string> GetStringAsync(
        string? methodName,
        Dictionary<string, string> data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
        => GetStringAsync(GetUrl(methodName, data), autoThrowException, cancellationToken);

    public virtual async Task<byte[]> GetByteArrayAsync(
        string? methodName,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = await CreateRequestAsync(HttpMethod.Get, methodName);
        HttpResponseMessage content = await SendAsync(request, autoThrowException, cancellationToken);
        return await content.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public virtual Task<byte[]> GetByteArrayAsync<TRequest>(
        string? methodName,
        TRequest data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default) where TRequest : class
        => GetByteArrayAsync(
            GetUrl(methodName, _typeConvertor.ConvertToKeyValuePairs(data)),
            autoThrowException,
            cancellationToken);

    public virtual Task<byte[]> GetByteArrayAsync(
        string? methodName,
        Dictionary<string, string> data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
        => GetByteArrayAsync(GetUrl(methodName, data), autoThrowException, cancellationToken);

    public virtual async Task<Stream> GetStreamAsync(
        string? methodName,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, methodName);
        var content = await SendAsync(request, autoThrowException, cancellationToken);
        return await content.Content.ReadAsStreamAsync(cancellationToken);
    }

    public virtual Task<Stream> GetStreamAsync<TRequest>(
        string? methodName,
        TRequest data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default) where TRequest : class
        => GetStreamAsync(
            GetUrl(methodName, _typeConvertor.ConvertToKeyValuePairs(data)),
            autoThrowException,
            cancellationToken);

    public virtual Task<Stream> GetStreamAsync(
        string? methodName,
        Dictionary<string, string> data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
        => GetStreamAsync(GetUrl(methodName, data), autoThrowException, cancellationToken);

    public virtual Task<HttpResponseMessage> GetAsync(
        string? methodName,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Get, methodName, null, autoThrowException, cancellationToken);

    public virtual Task<HttpResponseMessage> GetAsync(
        string? methodName,
        Dictionary<string, string> data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
        => GetAsync(GetUrl(methodName, data), autoThrowException, cancellationToken);

    public virtual async Task<TResponse?> GetAsync<TResponse>(
        string? methodName,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, methodName);
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public virtual async Task<TResponse?> GetAsync<TRequest, TResponse>(
        string? methodName,
        TRequest data,
        CancellationToken cancellationToken = default) where TRequest : class
    {
        var request =
            await CreateRequestAsync(HttpMethod.Get, GetUrl(methodName, _typeConvertor.ConvertToKeyValuePairs(data)));
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public virtual async Task<TResponse?> GetAsync<TResponse>(
        string? methodName,
        object data,
        CancellationToken cancellationToken = default)
    {
        var request =
            await CreateRequestAsync(HttpMethod.Get, GetUrl(methodName, _typeConvertor.ConvertToKeyValuePairs(data)));
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public virtual async Task<TResponse?> GetAsync<TResponse>(
        string? methodName,
        Dictionary<string, string> data,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, GetUrl(methodName, data));
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    protected virtual string GetUrl(string? url, IEnumerable<KeyValuePair<string, string>> properties)
    {
        url ??= string.Empty;
        foreach (var property in properties)
        {
            string value = property.Value;

            url = !url.Contains("?") ?
                $"{url}?{property.Key}={value}" :
                $"{url}&{property.Key}={value}";
        }

        return url;
    }

    public virtual Task<HttpResponseMessage> PostAsync(
        string? methodName,
        HttpContent? content,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Post, methodName, content, autoThrowException, cancellationToken);

    public virtual async Task<HttpResponseMessage> PostAsync<TRequest>(
        string? methodName,
        TRequest data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Post, methodName, data);
        return await SendAsync(request, autoThrowException, cancellationToken);
    }

    public virtual async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string? methodName,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = await CreateRequestAsync(HttpMethod.Post, methodName, data);
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public virtual Task<TResponse?> PostAsync<TResponse>(
        string? methodName,
        object data,
        CancellationToken cancellationToken = default)
        => PostAsync<object, TResponse>(methodName, data, cancellationToken);

    public virtual Task<HttpResponseMessage> PatchAsync(
        string? methodName,
        HttpContent? content,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Patch, methodName, content, autoThrowException, cancellationToken);

    public virtual async Task<HttpResponseMessage> PatchAsync<TRequest>(
        string? methodName,
        TRequest data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Patch, methodName, data);
        return await SendAsync(request, autoThrowException, cancellationToken);
    }

    public virtual async Task<TResponse?> PatchAsync<TRequest, TResponse>(
        string? methodName,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Patch, methodName, data);
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public virtual Task<TResponse?> PatchAsync<TResponse>(
        string? methodName,
        object data,
        CancellationToken cancellationToken = default)
        => PatchAsync<object, TResponse>(methodName, data, cancellationToken);

    public virtual Task<HttpResponseMessage> PutAsync(
        string? methodName,
        HttpContent? content,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Put, methodName, content, autoThrowException, cancellationToken);

    public virtual async Task<HttpResponseMessage> PutAsync<TRequest>(
        string? methodName,
        TRequest data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Put, methodName, data);
        return await SendAsync(request, autoThrowException, cancellationToken);
    }

    public virtual async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string? methodName,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Put, methodName, data);
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public virtual Task<TResponse?> PutAsync<TResponse>(
        string? methodName,
        object data,
        CancellationToken cancellationToken = default)
        => PutAsync<object, TResponse>(methodName, data, cancellationToken);

    public virtual Task<HttpResponseMessage> DeleteAsync(
        string? methodName,
        HttpContent? content,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Delete, methodName, content, autoThrowException, cancellationToken);

    public virtual async Task<HttpResponseMessage> DeleteAsync<TRequest>(
        string? methodName,
        TRequest data,
        bool autoThrowException = true,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Delete, methodName, data);
        return await SendAsync(request, autoThrowException, cancellationToken);
    }

    public virtual async Task<TResponse?> DeleteAsync<TRequest, TResponse>(
        string? methodName,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Delete, methodName, data);
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public virtual Task<TResponse?> DeleteAsync<TResponse>(
        string? methodName,
        object data,
        CancellationToken cancellationToken = default)
        => DeleteAsync<object, TResponse>(methodName, data, cancellationToken);
}
