﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Utils.Data.Elasticsearch.Options;

public class ElasticsearchRelationsOptions
{
    public string Name { get; }

    public Func<IServiceProvider, IElasticClient> Func { get; set; }

    public ElasticsearchRelationsOptions(string name)
    {
        Name = name;
    }
}