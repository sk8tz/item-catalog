﻿
using Messenger.Application.Common.Interfaces;
using Messenger.Domain.Entities;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Messenger.Contracts;
using MassTransit;

namespace Messenger.Application.Messages.Commands;

public record PostMessageCommand(string ItemId, string Text, string? ReplyToId) : IRequest<MessageDto>
{
    public class PostMessageCommandHandler : IRequestHandler<PostMessageCommand, MessageDto>
    {
        private readonly IMessengerContext context;
        private readonly IBus _bus;

        public PostMessageCommandHandler(IMessengerContext context, IBus bus)
        {
            this.context = context;
            _bus = bus;
        }

        public async Task<MessageDto> Handle(PostMessageCommand request, CancellationToken cancellationToken)
        {
            //var item = await context.Items.FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

            //if (item is null) throw new Exception();

            var message = new Message(request.Text, request.ReplyToId);

            context.Messages.Add(message);

            await context.SaveChangesAsync(cancellationToken);

            message = await context.Messages
                .Include(c => c.CreatedBy)
                .Include(c => c.LastModifiedBy)
                .Include(c => c.DeletedBy)
                .Include(c => c.ReplyTo)
                .ThenInclude(c => c.CreatedBy)
                .ThenInclude(c => c.LastModifiedBy)
                .ThenInclude(c => c.DeletedBy)
                .Include(c => c.Receipts)
                .ThenInclude(r => r.CreatedBy)
                //.Where(c => c.Item.Id == request.ItemId)
                .OrderByDescending(c => c.Created)
                .IgnoreQueryFilters()
                .AsSplitQuery()
                .FirstAsync(x => x.Id == message.Id);

            var result = message.ToDto();

            await _bus.Publish(new MessagePosted(result));

            return message.ToDto();
        }
    }
}
