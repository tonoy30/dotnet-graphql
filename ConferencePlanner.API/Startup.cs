using ConferencePlanner.Business.DbContexts;
using ConferencePlanner.Business.GraphQL;
using ConferencePlanner.Business.GraphQL.DataLoaders;
using ConferencePlanner.Business.GraphQL.Mutations;
using ConferencePlanner.Business.GraphQL.Queries;
using ConferencePlanner.Business.GraphQL.Subscriptions;
using ConferencePlanner.Business.GraphQL.Types;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ConferencePlanner.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "ConferencePlanner.API", Version = "v1"});
            });
            services
                .AddPooledDbContextFactory<ApplicationDbContext>(options =>
                    options.UseSqlite("Data Source=conferences.db"));

            services
                .AddGraphQLServer()
                .AddQueryType(d => d.Name("Query"))
                .AddTypeExtension<SpeakerQuery>()
                .AddTypeExtension<SessionQuery>()
                .AddTypeExtension<TrackQuery>()
                .AddTypeExtension<AttendeeQuery>()
                .AddMutationType(d => d.Name("Mutation"))
                .AddTypeExtension<SpeakerMutation>()
                .AddTypeExtension<SessionsMutation>()
                .AddTypeExtension<TrackMutation>()
                .AddTypeExtension<AttendeeMutation>()
                .AddSubscriptionType(d => d.Name("Subscription"))
                .AddTypeExtension<SessionSubscription>()
                .AddTypeExtension<AttendeeSubscription>()
                .AddType<SpeakerType>()
                .AddType<AttendeeType>()
                .AddType<TrackType>()
                .AddType<SessionType>()
                .EnableRelaySupport()
                .AddFiltering()
                .AddSorting()
                .AddInMemorySubscriptions()
                .AddDataLoader<SpeakerByIdDataLoader>()
                .AddDataLoader<AttendeeByIdDataLoader>()
                .AddDataLoader<TrackByIdDataLoader>()
                .AddDataLoader<SessionByIdDataLoader>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UsePlayground();
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConferencePlanner.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseWebSockets();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGraphQL();
            });
        }
    }
}
// dotnet ef migrations add -s ConferencePlanner.API -p ConferencePlanner.Business --context ApplicationDbContext -o Migrations/Initial Initial
// dotnet ef database update -s ConferencePlanner.API -p ConferencePlanner.Business --context ApplicationDbContext