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
    public class SpeakerType : ObjectType<Speaker>
    {
        private class SpeakerResolvers
        {
            public async Task<IEnumerable<Session>> GetSessionsAsync(Speaker speaker,
                [ScopedService] ApplicationDbContext dbContext, SessionByIdDataLoader sessionByIdDataLoader,
                CancellationToken cancellationToken)
            {
                var sessionIds = await dbContext.Speakers
                    .Where(s => s.Id == speaker.Id)
                    .Include(s => s.SessionSpeakers)
                    .SelectMany(s => s.SessionSpeakers.Select(t => t.SessionId))
                    .ToArrayAsync(cancellationToken);

                return await sessionByIdDataLoader.LoadAsync(sessionIds, cancellationToken);
            }
        }

        protected override void Configure(IObjectTypeDescriptor<Speaker> descriptor)
        {
            descriptor
                .ImplementsNode()
                .IdField(t => t.Id)
                .ResolveNode((ctx, id) => ctx.DataLoader<SpeakerByIdDataLoader>().LoadAsync(id, ctx.RequestAborted));

            descriptor
                .Field(t => t.SessionSpeakers)
                .ResolveWith<SpeakerResolvers>(t => t.GetSessionsAsync(default!, default!, default!, default))
                .UseDbContext<ApplicationDbContext>()
                .Name("sessions");
        }
    }
}