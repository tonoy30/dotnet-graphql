using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConferencePlanner.Business.DbContexts;
using ConferencePlanner.Business.GraphQL.DataLoaders;
using ConferencePlanner.Business.GraphQL.Extensions;
using ConferencePlanner.Contacts.Models;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace ConferencePlanner.Business.GraphQL.Types
{
    public class AttendeeType : ObjectType<Attendee>
    {
        private class AttendeeResolvers
        {
            public async Task<IEnumerable<Session>> GetSessionAsync(Attendee attendee,
                [ScopedService] ApplicationDbContext dbContext, SessionByIdDataLoader sessionByIdDataLoader,
                CancellationToken cancellationToken)
            {
                int[] speakerIds = await dbContext.Attendees
                    .Where(a => a.Id == attendee.Id)
                    .Include(a => a.SessionsAttendees)
                    .SelectMany(a => a.SessionsAttendees.Select(t => t.SessionId))
                    .ToArrayAsync(cancellationToken);

                return await sessionByIdDataLoader.LoadAsync(speakerIds, cancellationToken);
            }
        }

        protected override void Configure(IObjectTypeDescriptor<Attendee> descriptor)
        {
            descriptor
                .ImplementsNode()
                .IdField(t => t.Id)
                .ResolveNode((ctx, id) => ctx.DataLoader<AttendeeByIdDataLoader>().LoadAsync(id, ctx.RequestAborted));

            descriptor
                .Field(t => t.SessionsAttendees)
                .ResolveWith<AttendeeResolvers>(t => t.GetSessionAsync(default!, default!, default!, default))
                .UseDbContext<ApplicationDbContext>()
                .Name("sessions");
        }
    }
}