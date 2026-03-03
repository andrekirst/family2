using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.Photos.Domain.Repositories;
using FamilyHub.Api.Features.Photos.Infrastructure.Repositories;

namespace FamilyHub.Api.Features.Photos;

public sealed class PhotosModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPhotoRepository, PhotoRepository>();
    }
}
