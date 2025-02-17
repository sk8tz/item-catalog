﻿
using System.ComponentModel.DataAnnotations;

using Catalog.Notifications.Application.Notifications;
using Catalog.Notifications.Application.Notifications.Commands;
using Catalog.Notifications.Application.Notifications.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Notifications.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = Notifications.Authentication.AuthSchemes.Default)]
public class NotificationsController : Controller
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<NotificationsResults>> GetNotifications(
        string? userId, string? tag,
        bool includeUnreadNotificationsCount = false,
        int page = 1, int pageSize = 5, string? sortBy = null, Application.Common.Models.SortDirection? sortDirection = null, CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetNotificationsQuery(userId, tag, includeUnreadNotificationsCount, page - 1, pageSize, sortBy, sortDirection), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult> CreateNotification(CreateNotificationDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CreateNotificationCommand(dto.Title, dto.Text, dto.Link, dto.UserId, dto.ExceptUserIds, dto.SubscriptionId, dto.SubscriptionGroupId, dto.ScheduledFor), cancellationToken);

        return Ok();
    }

    [HttpPost("{id}/MarkAsRead")]
    public async Task<ActionResult> MarkNotificationAsRead(string id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkNotificationAsReadCommand(id), cancellationToken);

        return Ok();
    }

    [HttpPost("MarkAllAsRead")]
    public async Task<ActionResult> MarkAllNotificationsAsRead(CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAllNotificationsAsReadCommand(), cancellationToken);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotification(string id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteNotificationCommand(id), cancellationToken);

        return Ok();
    }

    [HttpGet("UnreadCount")]
    public async Task<ActionResult<int>> GetUnreadNotificationsCount(string? userId, CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetUnreadNotificationsCountQuery(userId), cancellationToken));
    }
}

public class CreateNotificationDto
{
    [Required]
    public string Title { get; set; } = null!;

    public string? Text { get; set; }

    public string? Link { get; set; }

    public string? UserId { get; set; }

    public string[]? ExceptUserIds { get; set; }

    public string? SubscriptionId { get; set; }

    public string? SubscriptionGroupId { get; set; }

    public DateTime? ScheduledFor { get; set; }
}