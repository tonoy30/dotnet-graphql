using System.Collections.Generic;
using System.Threading.Tasks;
using ConferencePlanner.Business.DbContexts;
using ConferencePlanner.Business.GraphQL.Common;
using ConferencePlanner.Business.GraphQL.Extensions;
using ConferencePlanner.Contacts.Models;
using HotChocolate;
using HotChocolate.Types;

namespace ConferencePlanner.Business.GraphQL.Mutations
{
    public record AddSpeakerInput(string Name, string? Bio, string? Website);

    public class SpeakerPayloadBase : Payload
    {
        protected SpeakerPayloadBase(Speaker speaker)
        {
            Speaker = speaker;
        }

        protected SpeakerPayloadBase(IReadOnlyList<UserError> errors) : base(errors)
        {
        }

        public Speaker? Speaker { get; }
    }

    public class AddSpeakerPayload : SpeakerPayloadBase
    {
        public AddSpeakerPayload(Speaker speaker) : base(speaker)
        {
        }

        public AddSpeakerPayload(UserError error) : base(new []{error})
        {
        }
    }

    [ExtendObjectType(Name = "Mutation")]
    public class SpeakerMutation
    {
        [UseApplicationDbContext]
        public async Task<AddSpeakerPayload> AddSpeakerAsync(AddSpeakerInput input,
            [ScopedService] ApplicationDbContext context)
        {
            if (string.IsNullOrEmpty(input.Name))
            {
                return new AddSpeakerPayload(
                    new UserError("The title cannot be empty.", "TITLE_EMPTY"));
            }
            var speaker = new Speaker
            {
                Name = input.Name,
                Bio = input.Bio,
                Website = input.Website
            };
            context.Speakers.Add(speaker);
            await context.SaveChangesAsync();

            return new AddSpeakerPayload(speaker);
        }
    }
}