using System.Threading;
using System.Threading.Tasks;
using ConferencePlanner.Business.GraphQL.DataLoaders;
using ConferencePlanner.Business.GraphQL.Queries;
using ConferencePlanner.Contacts.Models;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using HotChocolate.Types.Relay;

namespace ConferencePlanner.Business.GraphQL.Subscriptions
{
    [ExtendObjectType(Name = "Subscription")]
    public class AttendeeSubscription
    {
        [Subscribe(With = nameof(SubscribeToOnAttendeeCheckedInAsync))]
        public SessionAttendeeCheckIn OnAttendeeCheckedIn(
            [ID(nameof(Session))] int sessionId,
            [EventMessage] int attendeeId,
            SessionByIdDataLoader sessionById,
            CancellationToken cancellationToken) =>
            new SessionAttendeeCheckIn(attendeeId, sessionId);

        public async ValueTask<ISourceStream<int>> SubscribeToOnAttendeeCheckedInAsync(
            int sessionId,
            [Service] ITopicEventReceiver eventReceiver,
            CancellationToken cancellationToken) =>
            await eventReceiver.SubscribeAsync<string, int>(
                "OnAttendeeCheckedIn_" + sessionId, cancellationToken);
    }
}