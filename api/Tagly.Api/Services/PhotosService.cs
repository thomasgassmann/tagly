
namespace Tagly.Api.Services;

public class PhotosService : Photos.PhotosBase
{
    private readonly ILogger<PhotosService> _logger;

    public PhotosService(ILogger<PhotosService> logger)
    {
        _logger = logger;
    }
}
