﻿
using Catalog.Application.Common.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Notifications.Queries;

public class GetUnreadNotificationsCountQuery : IRequest<int>
{
    public class GetUnreadNotificationsCountQueryHandler : IRequestHandler<GetUnreadNotificationsCountQuery, int>
    {
        private readonly ICatalogContext context;
        private readonly ICurrentUserService _currentUserService;

        public GetUnreadNotificationsCountQueryHandler(ICatalogContext context, ICurrentUserService currentUserService)
        {
            this.context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(GetUnreadNotificationsCountQuery request, CancellationToken cancellationToken)
        {
            var query = context.Notifications.AsQueryable();

            if (_currentUserService.UserId is not null)
            {
                query = query.Where(n => n.UserId == _currentUserService.UserId);
            }
            else
            {
                query = query.Where(n => n.UserId == null);
            }

            var unreadNotificationsCount = await query
                .Where(n => !n.IsRead)
                .CountAsync(cancellationToken);

            return unreadNotificationsCount;
        }
    }
}