﻿
using Catalog.Application.Common.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Items.Queries;

public record GetCommentQuery(string Id) : IRequest<CommentDto?>
{
    public class GetCommentQueryHandler : IRequestHandler<GetCommentQuery, CommentDto?>
    {
        private readonly ICatalogContext context;

        public GetCommentQueryHandler(ICatalogContext context)
        {
            this.context = context;
        }

        public async Task<CommentDto?> Handle(GetCommentQuery request, CancellationToken cancellationToken)
        {
            var comment = await context.Comments
                .Include(i => i.CreatedBy)
                .Include(i => i.LastModifiedBy)
                .AsSplitQuery()
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (comment is null) return null;

            return comment.ToDto();
        }
    }
}