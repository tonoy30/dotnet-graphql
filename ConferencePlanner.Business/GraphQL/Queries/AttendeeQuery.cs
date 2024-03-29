﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConferencePlanner.Business.DbContexts;
using ConferencePlanner.Business.GraphQL.DataLoaders;
using ConferencePlanner.Business.GraphQL.Extensions;
using ConferencePlanner.Contacts.Models;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Relay;

namespace ConferencePlanner.Business.GraphQL.Queries
{
    public class AttendeeQuery
    {
        [UseApplicationDbContext]
        [UsePaging]
        public IQueryable<Attendee> GetAttendees(
            [ScopedService] ApplicationDbContext context) =>
            context.Attendees;

        public Task<Attendee> GetAttendeeByIdAsync(
            [ID(nameof(Attendee))] int id,
            AttendeeByIdDataLoader attendeeById,
            CancellationToken cancellationToken) =>
            attendeeById.LoadAsync(id, cancellationToken);

        public async Task<IEnumerable<Attendee>> GetAttendeesByIdAsync(
            [ID(nameof(Attendee))] int[] ids,
            AttendeeByIdDataLoader attendeeById,
            CancellationToken cancellationToken) =>
            await attendeeById.LoadAsync(ids, cancellationToken);
    }
}