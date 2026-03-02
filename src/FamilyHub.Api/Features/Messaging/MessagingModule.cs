using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Infrastructure.Repositories;

namespace FamilyHub.Api.Features.Messaging;

public sealed class MessagingModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMessageRepository, MessageRepository>();
    }
}
