using System.Reflection;
using ConferencePlanner.Business.DbContexts;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace ConferencePlanner.Business.GraphQL.Extensions
{
    public class UseApplicationDbContextAttribute: ObjectFieldDescriptorAttribute
    {
        public override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
        {
            descriptor.UseDbContext<ApplicationDbContext>();
        }
    }
}