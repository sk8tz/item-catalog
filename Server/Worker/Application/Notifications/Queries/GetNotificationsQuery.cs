
using Worker.Application.Common.Interfaces;
using Worker.Application.Common.Models;
using Worker.Domain;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Worker.Application.Notifications.Queries;

public class GetNotificationsQuery : IRequest<NotificationsResults>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? SortBy { get; }
    public Application.Common.Models.SortDirection? SortDirection { get; }
    public bool IncludeUnreadNotificationsCount { get; }

    public GetNotificationsQuery(
        bool includeUnreadNotificationsCount,
        int page, int pageSize, string? sortBy = null, Application.Common.Models.SortDirection? sortDirection = null)
    {
        IncludeUnreadNotificationsCount = includeUnreadNotificationsCount;
        Page = page;
        PageSize = pageSize;
        SortBy = sortBy;
        SortDirection = sortDirection;
    }

    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, NotificationsResults>
    {
        private readonly IWorkerContext context;
        private readonly ICurrentUserService _currentUserService;

        public GetNotificationsQueryHandler(IWorkerContext context, ICurrentUserService currentUserService)
        {
            this.context = context;
            _currentUserService = currentUserService;
        }

        public async Task<NotificationsResults> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var query = context.Notifications
                .Where(n => n.Published != null)
                .AsQueryable();

            if (_currentUserService.UserId is not null)
            {
                query = query.Where(n => n.UserId == _currentUserService.UserId || n.UserId == null);
            }
            else
            {
                query = query.Where(n => n.UserId == null);
            }

            query = query.OrderByDescending(n => n.Published);

            var totalCount = await query.CountAsync(cancellationToken);

            if (request.SortBy is not null)
            {
                query = query.OrderBy(
                    request.SortBy,
                    request.SortDirection == Application.Common.Models.SortDirection.Desc ? Application.SortDirection.Descending : Application.SortDirection.Ascending);
            }

            query = query.Skip(request.Page * request.PageSize)
                .Take(request.PageSize).AsQueryable();

            int? unreadNotificationsCount = null;

            if (request.IncludeUnreadNotificationsCount)
            {
                unreadNotificationsCount = await context.Notifications
                    .OrderByDescending(n => n.Published)
                    .Where(n => n.Published != null)
                    .Where(n => n.UserId == _currentUserService.UserId || n.UserId == null)
                    .Where(n => !n.IsRead)
                    .CountAsync(cancellationToken);
            }

            var notifications = await query.ToListAsync(cancellationToken);

            return new NotificationsResults(
                notifications.Select(notification => new NotificationDto(notification.Id, notification.Title, notification.Text, notification.Link, notification.UserId, notification.IsRead, notification.Published, notification.ScheduledFor, notification.Created, notification.CreatedBy, notification.LastModified, notification.LastModifiedBy)),
                unreadNotificationsCount,
                totalCount);
        }
    }
}
