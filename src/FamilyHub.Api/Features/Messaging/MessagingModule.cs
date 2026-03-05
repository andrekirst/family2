using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Messaging.Application.Search;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Infrastructure.Repositories;

namespace FamilyHub.Api.Features.Messaging;

public sealed class MessagingModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMessageRepository, MessageRepository>();

        // Search
        services.AddScoped<ISearchProvider, MessagingSearchProvider>();
        services.AddSingleton<ICommandPaletteProvider, MessagingCommandPaletteProvider>();
    }
}
