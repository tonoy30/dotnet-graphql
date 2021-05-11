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
    public class TrackType : ObjectType<Track>
    {
        private class TrackResolvers
        {
            public async Task<IEnumerable<Session>> GetSessionsAsync(Track track,
                [ScopedService] ApplicationDbContext dbContext, SessionByIdDataLoader sessionByIdDataLoader,
                CancellationToken cancellationToken)
            {
                var sessionIds = await dbContext.Tracks
                    .Where(s => s.Id == track.Id)
                    .Select(s => s.Id)
                    .ToArrayAsync(cancellationToken);

                return await sessionByIdDataLoader.LoadAsync(sessionIds, cancellationToken);
            }
        }

        protected override void Configure(IObjectTypeDescriptor<Track> descriptor)
        {
            descriptor
                .ImplementsNode()
                .IdField(t => t.Id)
                .ResolveNode((ctx, id) => ctx.DataLoader<TrackByIdDataLoader>().LoadAsync(id, ctx.RequestAborted));
            descriptor
                .Field(t => t.Sessions)
                .ResolveWith<TrackResolvers>(t => t.GetSessionsAsync(default!, default!, default!, default))
                .UseDbContext<ApplicationDbContext>()
                .UsePaging<NonNullType<SessionType>>()
                .Name("sessions");
        }
    }
}