using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConferencePlanner.Business.DbContexts;
using ConferencePlanner.Business.GraphQL.Common;
using ConferencePlanner.Business.GraphQL.DataLoaders;
using ConferencePlanner.Business.GraphQL.Extensions;
using ConferencePlanner.Contacts.Models;
using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using Microsoft.EntityFrameworkCore;

namespace ConferencePlanner.Business.GraphQL.Mutations
{
    public record RegisterAttendeeInput(
        string FirstName,
        string LastName,
        string UserName,
        string EmailAddress);

    public record CheckInAttendeeInput(
        [ID(nameof(Session))] int SessionId,
        [ID(nameof(Attendee))] int AttendeeId);

    public class AttendeePayloadBase : Payload
    {
        protected AttendeePayloadBase(Attendee attendee)
        {
            Attendee = attendee;
        }

        protected AttendeePayloadBase(IReadOnlyList<UserError> errors)
            : base(errors)
        {
        }

        public Attendee? Attendee { get; }
    }

    public class RegisterAttendeePayload : AttendeePayloadBase
    {
        public RegisterAttendeePayload(Attendee attendee)
            : base(attendee)
        {
        }

        public RegisterAttendeePayload(UserError error)
            : base(new[] {error})
        {
        }
    }

    public class CheckInAttendeePayload : AttendeePayloadBase
    {
        private int? _sessionId;

        public CheckInAttendeePayload(Attendee attendee, int sessionId)
            : base(attendee)
        {
            _sessionId = sessionId;
        }

        public CheckInAttendeePayload(UserError error)
            : base(new[] {error})
        {
        }

        public async Task<Session?> GetSessionAsync(
            SessionByIdDataLoader sessionById,
            CancellationToken cancellationToken)
        {
            if (_sessionId.HasValue)
            {
                return await sessionById.LoadAsync(_sessionId.Value, cancellationToken);
            }

            return null;
        }
    }

    [ExtendObjectType(Name = "Mutation")]
    public class AttendeeMutation
    {
        [UseApplicationDbContext]
        public async Task<RegisterAttendeePayload> RegisterAttendeeAsync(
            RegisterAttendeeInput input,
            [ScopedService] ApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            var attendee = new Attendee
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                UserName = input.UserName,
                EmailAddress = input.EmailAddress
            };

            context.Attendees.Add(attendee);

            await context.SaveChangesAsync(cancellationToken);

            return new RegisterAttendeePayload(attendee);
        }

        [UseApplicationDbContext]
        public async Task<CheckInAttendeePayload> CheckInAttendeeAsync(
            CheckInAttendeeInput input,
            [ScopedService] ApplicationDbContext context,
            [Service] ITopicEventSender eventSender,
            CancellationToken cancellationToken)
        {
            Attendee attendee = await context.Attendees.FirstOrDefaultAsync(
                t => t.Id == input.AttendeeId, cancellationToken);

            if (attendee is null)
            {
                return new CheckInAttendeePayload(
                    new UserError("Attendee not found.", "ATTENDEE_NOT_FOUND"));
            }

            attendee.SessionsAttendees.Add(
                new SessionAttendee
                {
                    SessionId = input.SessionId
                });

            await context.SaveChangesAsync(cancellationToken);

            await eventSender.SendAsync(
                "OnAttendeeCheckedIn_" + input.SessionId,
                input.AttendeeId,
                cancellationToken);

            return new CheckInAttendeePayload(attendee, input.SessionId);
        }
    }
}