using System;
using System.Threading.Tasks;
using ConferencePlanner.Business.DbContexts;
using ConferencePlanner.Business.GraphQL.Mutations;
using ConferencePlanner.Business.GraphQL.Queries;
using ConferencePlanner.Business.GraphQL.Types;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Xunit;

namespace ConferencePlanner.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Attendee_Schema_Changed()
        {
            // arrange
            // act
            var schema = await new ServiceCollection()
                .AddPooledDbContextFactory<ApplicationDbContext>(
                    options => options.UseInMemoryDatabase("Data Source=conferences.db"))
                .AddGraphQL()
                .AddQueryType(d => d.Name("Query"))
                .AddTypeExtension<AttendeeQuery>()
                .AddMutationType(d => d.Name("Mutation"))
                .AddTypeExtension<AttendeeMutation>()
                .AddType<AttendeeType>()
                .AddType<SessionType>()
                .AddType<SpeakerType>()
                .AddType<TrackType>()
                .EnableRelaySupport()
                .BuildSchemaAsync();

            // assert
            schema.Print().MatchSnapshot();
        }
    }
}