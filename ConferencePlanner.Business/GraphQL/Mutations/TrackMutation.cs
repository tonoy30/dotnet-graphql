using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConferencePlanner.Business.DbContexts;
using ConferencePlanner.Business.GraphQL.Common;
using ConferencePlanner.Business.GraphQL.Extensions;
using ConferencePlanner.Contacts.Models;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Relay;

namespace ConferencePlanner.Business.GraphQL.Mutations
{
    public record AddTrackInput(string Name);

    public record RenameTrackInput([ID(nameof(Track))] int Id, string Name);

    public class TrackPayloadBase : Payload
    {
        public TrackPayloadBase(Track track)
        {
            Track = track;
        }

        public TrackPayloadBase(IReadOnlyList<UserError> errors)
            : base(errors)
        {
        }

        public Track? Track { get; }
    }

    public class AddTrackPayload : TrackPayloadBase
    {
        public AddTrackPayload(Track track)
            : base(track)
        {
        }

        public AddTrackPayload(IReadOnlyList<UserError> errors)
            : base(errors)
        {
        }
    }

    public class RenameTrackPayload : TrackPayloadBase
    {
        public RenameTrackPayload(Track track)
            : base(track)
        {
        }

        public RenameTrackPayload(IReadOnlyList<UserError> errors)
            : base(errors)
        {
        }
    }

    [ExtendObjectType(Name = "Mutation")]
    public class TrackMutation
    {
        [UseApplicationDbContext]
        public async Task<AddTrackPayload> AddTrackAsync(
            AddTrackInput input,
            [ScopedService] ApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            var track = new Track {Name = input.Name};
            context.Tracks.Add(track);

            await context.SaveChangesAsync(cancellationToken);

            return new AddTrackPayload(track);
        }

        [UseApplicationDbContext]
        public async Task<RenameTrackPayload> RenameTrackAsync(
            RenameTrackInput input,
            [ScopedService] ApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            Track track = await context.Tracks.FindAsync(input.Id);
            track.Name = input.Name;

            await context.SaveChangesAsync(cancellationToken);

            return new RenameTrackPayload(track);
        }
    }
}