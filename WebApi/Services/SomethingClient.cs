﻿
using Microsoft.AspNetCore.SignalR;

using Catalog.Application.Common.Interfaces;

using WebApi.Hubs;

namespace WebApi.Services;

public class SomethingClient : ISomethingClient
{
    private readonly IHubContext<SomethingHub, ISomethingClient> _somethingHubContext;

    public SomethingClient(IHubContext<SomethingHub, ISomethingClient> somethingHubContext)
    {
        _somethingHubContext = somethingHubContext;
    }

    public async Task ResponseReceived(string message)
    {
        await _somethingHubContext.Clients.All.ResponseReceived(message);
    }
}

