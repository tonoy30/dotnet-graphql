using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConferencePlanner.Business.DbContexts;
using ConferencePlanner.Business.GraphQL.Common;
using ConferencePlanner.Business.GraphQL.DataLoaders;
using ConferencePlanner.Business.GraphQL.Extensions;
using ConferencePlanner.Business.GraphQL.Subscriptions;
using ConferencePlanner.Contacts.Models;
using HotChocolate;
using HotChocolate.Data.Filters;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using Microsoft.EntityFrameworkCore;

namespace ConferencePlanner.Business.GraphQL.Mutations
{
    public record AddSessionInput(string Title, string? Abstract, [ID(nameof(Speaker))] IReadOnlyList<int> SpeakerIds);

    public record ScheduleSessionInput([ID(nameof(Session))] int SessionId, [ID(nameof(Track))] int TrackId,
        DateTimeOffset StartTime, DateTimeOffset EndTime);

    public class SessionsPayloadBase : Payload
    {
        protected SessionsPayloadBase(Session session)
        {
            Session = session;
        }

        protected SessionsPayloadBase(IReadOnlyList<UserError> errors) : base(errors)
        {
        }

        public Session? Session { get; }
    }

    public class AddSessionPayload : SessionsPayloadBase
    {
        public AddSessionPayload(Session session) : base(session)
        {
        }

        public AddSessionPayload(UserError error) : base(new[] {error})
        {
        }
    }

    public class ScheduleSessionPayload : SessionsPayloadBase
    {
        public ScheduleSessionPayload(Session session) : base(session)
        {
        }

        public ScheduleSessionPayload(UserError error) : base(new[] {error})
        {
        }

        public async Task<Track?> GetTrackAsync(
            TrackByIdDataLoader trackById,
            CancellationToken cancellationToken)
        {
            if (Session is null)
            {
                return null;
            }

            return await trackById.LoadAsync(Session.Id, cancellationToken);
        }

        [UseApplicationDbContext]
        public async Task<IEnumerable<Speaker>?> GetSpeakersAsync(
            [ScopedService] ApplicationDbContext dbContext,
            SpeakerByIdDataLoader speakerById,
            CancellationToken cancellationToken)
        {
            if (Session is null)
            {
                return null;
            }

            int[] speakerIds = await dbContext.Sessions
                .Where(s => s.Id == Session.Id)
                .Include(s => s.SessionSpeakers)
                .SelectMany(s => s.SessionSpeakers.Select(t => t.SpeakerId))
                .ToArrayAsync(cancellationToken);

            return await speakerById.LoadAsync(speakerIds, cancellationToken);
        }
    }


    [ExtendObjectType(Name = "Mutation")]
    public class SessionsMutation
    {
        [UseApplicationDbContext]
        public async Task<AddSessionPayload> AddSessionAsync(AddSessionInput input,
            [ScopedService] ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input.Title))
            {
                return new AddSessionPayload(
                    new UserError("The title cannot be empty.", "TITLE_EMPTY"));
            }

            if (input.SpeakerIds.Count == 0)
            {
                return new AddSessionPayload(
                    new UserError("No speaker assigned.", "NO_SPEAKER"));
            }

            var session = new Session
            {
                Title = input.Title,
                Abstract = input.Abstract
            };
            foreach (var speakerId in input.SpeakerIds)
            {
                session.SessionSpeakers.Add(new SessionSpeaker
                {
                    SpeakerId = speakerId
                });
            }

            dbContext.Sessions.Add(session);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new AddSessionPayload(session);
        }

        [UseApplicationDbContext]
        public async Task<ScheduleSessionPayload> ScheduleSessionAsync(
            ScheduleSessionInput input,
            [ScopedService] ApplicationDbContext context,
            [Service] ITopicEventSender eventSender)
        {
            if (input.EndTime < input.StartTime)
            {
                return new ScheduleSessionPayload(
                    new UserError("endTime has to be larger than startTime.", "END_TIME_INVALID"));
            }

            var session = await context.Sessions.FindAsync(input.SessionId);
            var initialTrackId = session.TrackId;

            if (session is null)
            {
                return new ScheduleSessionPayload(
                    new UserError("Session not found.", "SESSION_NOT_FOUND"));
            }

            session.TrackId = input.TrackId;
            session.StartTime = input.StartTime;
            session.EndTime = input.EndTime;

            await context.SaveChangesAsync();
            await eventSender.SendAsync(nameof(SessionSubscription.OnSessionScheduledAsync), session.Id);
            return new ScheduleSessionPayload(session);
        }
    }
}