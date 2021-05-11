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
    public class SessionType : ObjectType<Session>
    {
        private class SessionResolvers
        {
            public async Task<IEnumerable<Speaker>> GetSpeakersAsync(Session session,
                [ScopedService] ApplicationDbContext dbContext, SpeakerByIdDataLoader speakerByIdDataLoader,
                CancellationToken cancellationToken)
            {
                var speakersIds = await dbContext.Sessions.Where(s => s.Id == session.Id)
                    .Include(s => s.SessionSpeakers)
                    .SelectMany(s => s.SessionSpeakers.Select(t => t.SpeakerId))
                    .ToArrayAsync(cancellationToken);
                return await speakerByIdDataLoader.LoadAsync(speakersIds, cancellationToken);
            }

            public async Task<IEnumerable<Attendee>> GetAttendeesAsync(Session session,
                [ScopedService] ApplicationDbContext dbContext, AttendeeByIdDataLoader attendeeByIdDataLoader,
                CancellationToken cancellationToken)
            {
                var speakerIds = await dbContext.Sessions
                    .Where(s => s.Id == session.Id)
                    .Include(s => s.SessionSpeakers)
                    .SelectMany(s => s.SessionSpeakers.Select(t => t.SpeakerId))
                    .ToArrayAsync(cancellationToken);
                return await attendeeByIdDataLoader.LoadAsync(speakerIds, cancellationToken);
            }

            public async Task<Track?> GetTrackAsync(
                Session session,
                TrackByIdDataLoader trackById,
                CancellationToken cancellationToken)
            {
                if (session.TrackId is null)
                {
                    return null;
                }

                return await trackById.LoadAsync(session.TrackId.Value, cancellationToken);
            }
        }

        protected override void Configure(IObjectTypeDescriptor<Session> descriptor)
        {
            descriptor
                .ImplementsNode()
                .IdField(s => s.Id)
                .ResolveNode((ctx, id) => ctx.DataLoader<SessionByIdDataLoader>().LoadAsync(id, ctx.RequestAborted));

            descriptor
                .Field(t => t.SessionSpeakers)
                .ResolveWith<SessionResolvers>(t => t.GetSpeakersAsync(default!, default!, default!, default))
                .UseDbContext<ApplicationDbContext>()
                .Name("speakers");

            descriptor
                .Field(t => t.SessionAttendees)
                .ResolveWith<SessionResolvers>(t => t.GetSpeakersAsync(default!, default!, default!, default))
                .UseDbContext<ApplicationDbContext>()
                .Name("attendees");

            descriptor
                .Field(t => t.TrackId)
                .ID(nameof(Track));
        }
    }
}