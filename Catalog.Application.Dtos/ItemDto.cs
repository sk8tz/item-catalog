﻿namespace Catalog.Application;

public record ItemDto(
    string Id, string Name, string? Description, string? Image,
    DateTime Created, string CreatedBy, DateTime? LastModified, string? LastModifiedBy);
