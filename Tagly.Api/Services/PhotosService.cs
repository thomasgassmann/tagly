using Grpc.Core;
using SixLabors.ImageSharp;
using Tagly.Db;
using Tagly.Grpc;

namespace Tagly.Api.Services;

public class PhotosService(ILogger<PhotosService> logger, TaglyContext dbContext) : Photos.PhotosBase
{
    private readonly ILogger<PhotosService> _logger = logger;

    public override async Task<PhotoCreationStatus> AddPhoto(ServerPhoto request, ServerCallContext context)
    {
        var requestBytes = request.Data.ToArray();
        using var requestStream = new MemoryStream(requestBytes);

        // verify that we're dealing with an image
        try
        {
            await Image.LoadAsync(requestStream, context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed adding photo: {message}", ex.Message);
            return new PhotoCreationStatus
            {
                Success = false,
                CreatedId = -1
            };
        }

        var entity = await dbContext.AddAsync(new StoredPhoto
        {
            Id = 0,
            Data = requestBytes,
            FileName = request.Meta.FileName,
            Date = request.Meta.Date.ToDateTime(),
            Description = request.Meta.Description,
            Latitude = request.Meta.Latitude,
            Longitude = request.Meta.Longitude,
            Created = DateTime.Now
        });
        await dbContext.SaveChangesAsync(context.CancellationToken);
        return new PhotoCreationStatus
        {
            Success = true,
            CreatedId = entity.Entity.Id
        };
    }
}