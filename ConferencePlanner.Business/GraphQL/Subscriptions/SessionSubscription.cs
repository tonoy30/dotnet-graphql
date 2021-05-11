using System.Threading;
using System.Threading.Tasks;
using ConferencePlanner.Business.GraphQL.DataLoaders;
using ConferencePlanner.Contacts.Models;
using HotChocolate;
using HotChocolate.Types;

namespace ConferencePlanner.Business.GraphQL.Subscriptions
{
    [ExtendObjectType(Name = "Subscription")]
    public class SessionSubscription
    {
        [Subscribe]
        [Topic]
        public Task<Session> OnSessionScheduledAsync(
            [EventMessage] int sessionId,
            SessionByIdDataLoader sessionById,
            CancellationToken cancellationToken) =>
            sessionById.LoadAsync(sessionId, cancellationToken);
    }
}