// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Storage.ObjectStorage.Aliyun.Tests;

public class CustomClient : DefaultStorageClient
{
    public Mock<IOss>? Oss;

    public CustomClient(ICredentialProvider credentialProvider,
        IAliyunStorageOptionProvider optionProvider,
        ILogger<DefaultStorageClient>? logger)
        : base(credentialProvider, optionProvider, logger)
    {
    }

    public override IOss GetClient()
    {
        if (Oss != null)
            return Oss.Object;

        Oss = new();
        Oss.Setup(c => c.GetObject(It.IsAny<string>(), It.IsAny<string>())).Returns(GetOssObject()).Verifiable();
        Oss.Setup(c => c.GetObject(It.IsAny<GetObjectRequest>())).Returns(GetOssObject()).Verifiable();
        Oss.Setup(c => c.PutObject(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<ObjectMetadata>()))
            .Returns(GetPutObjectResult()).Verifiable();
        Oss.Setup(c => c.ResumableUploadObject(It.IsAny<UploadObjectRequest>())).Returns(GetPutObjectResult()).Verifiable();
        Oss.Setup(c => c.DoesObjectExist(It.IsAny<string>(), "1.jpg")).Returns(false).Verifiable();
        Oss.Setup(c => c.DoesObjectExist(It.IsAny<string>(), "2.jpg")).Returns(true).Verifiable();
        Oss.Setup(c => c.DeleteObject(It.IsAny<string>(), "1.jpg")).Returns(GetDeleteFail()).Verifiable();
        Oss.Setup(c => c.DeleteObject(It.IsAny<string>(), "2.jpg")).Returns(GetDeleteSuccess()).Verifiable();
        Oss.Setup(c => c.DeleteObjects(It.IsAny<DeleteObjectsRequest>())).Returns(GetDeleteObjectsResult()).Verifiable();
        return Oss.Object;
    }

    private OssObject GetOssObject()
    {
        string objectName = string.Empty;
        var constructor = typeof(OssObject).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, new[] { typeof(string) })!;
        OssObject ossObject = (constructor.Invoke(new object[] { objectName }) as OssObject)!;
        ossObject.ResponseStream = null;
        return ossObject;
    }

    private PutObjectResult GetPutObjectResult()
    {
        var constructor = typeof(PutObjectResult).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes)!;
        var result = (constructor.Invoke(Array.Empty<object>()) as PutObjectResult)!;
        result.ResponseStream = new MemoryStream(Encoding.Default.GetBytes("test"));
        return result;
    }

    private DeleteObjectResult GetDeleteFail()
    {
        var constructor = typeof(DeleteObjectResult).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes)!;
        var result = (constructor.Invoke(Array.Empty<object>()) as DeleteObjectResult)!;
        result.HttpStatusCode = HttpStatusCode.NotFound;
        return result;
    }

    private DeleteObjectResult GetDeleteSuccess()
    {
        var constructor = typeof(DeleteObjectResult).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes)!;
        var result = (constructor.Invoke(Array.Empty<object>()) as DeleteObjectResult)!;
        result.HttpStatusCode = HttpStatusCode.OK;
        return result;
    }

    private DeleteObjectsResult GetDeleteObjectsResult()
    {
        var constructor = typeof(DeleteObjectsResult).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes)!;
        var result = (constructor.Invoke(Array.Empty<object>()) as DeleteObjectsResult)!;
        result.HttpStatusCode = HttpStatusCode.OK;
        return result;
    }
}
