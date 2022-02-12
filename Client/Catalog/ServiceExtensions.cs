﻿using System;
using System.Globalization;

using Catalog.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.JSInterop;

using MudBlazor.Services;

namespace Catalog;

public static class ServiceExtensions
{
    public static IServiceCollection AddApp(this IServiceCollection services, IConfiguration configuration)
    {
        //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("sv");
        //CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("sv");

        string url = "https://localhost/";

        services.AddLocalization();

        services.AddMudServices();

        services.AddScoped<INotificationService, NotificationService>();

        services.AddSingleton<IFilePickerService, FilePickerService>();

        services.AddScoped<CustomAuthorizationMessageHandler>();

        services.AddHttpClient(nameof(Catalog.Client.IClient), (sp, http) =>
        {
            var navigationManager = sp.GetRequiredService<NavigationManager>();
            http.BaseAddress = new Uri($"{url}api/");
        })
        .AddTypedClient<Catalog.Client.IClient>((http, sp) => new Catalog.Client.Client(http));

        services.AddHttpClient(nameof(Catalog.Client.IItemsClient), (sp, http) =>
        {
            var navigationManager = sp.GetRequiredService<NavigationManager>();
            http.BaseAddress = new Uri($"{url}api/");
        })
        .AddTypedClient<Catalog.Client.IItemsClient>((http, sp) => new Catalog.Client.ItemsClient(http))
        .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

        services.AddHttpClient(nameof(Catalog.Client.IDoSomethingClient), (sp, http) =>
        {
            var navigationManager = sp.GetRequiredService<NavigationManager>();
            http.BaseAddress = new Uri($"{url}api/");
        })
        .AddTypedClient<Catalog.Client.IDoSomethingClient>((http, sp) => new Catalog.Client.DoSomethingClient(http));

        services.AddHttpClient(nameof(Catalog.Client.ISearchClient), (sp, http) =>
        {
            var navigationManager = sp.GetRequiredService<NavigationManager>();
            http.BaseAddress = new Uri($"{url}api/");
        })
        .AddTypedClient<Catalog.Client.ISearchClient>((http, sp) => new Catalog.Client.SearchClient(http))
        .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

        services.AddHttpClient(nameof(Catalog.Client.INotificationsClient), (sp, http) =>
        {
            var navigationManager = sp.GetRequiredService<NavigationManager>();
            http.BaseAddress = new Uri($"{url}api/");
        })
        .AddTypedClient<Catalog.Client.INotificationsClient>((http, sp) => new Catalog.Client.NotificationsClient(http))
        .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

        return services;
    }

    public static async Task ConfigureApp(this IServiceProvider serviceProvider)
    {
        await SetLocale(serviceProvider);
    }

    private static async Task SetLocale(IServiceProvider serviceProvider)
    {
        CultureInfo culture;
        var js = serviceProvider.GetRequiredService<IJSRuntime>();
        var result = await js.InvokeAsync<string>("blazorCulture.get");

        if (result != null)
        {
            culture = new CultureInfo(result);
        }
        else
        {
            culture = new CultureInfo("en-US");
            await js.InvokeVoidAsync("blazorCulture.set", "en-US");
        }

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}