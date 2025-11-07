using System.Net.Mime;
using CatchUpPlatform.API.News.Domain.Model.Queries;
using CatchUpPlatform.API.News.Domain.Services;
using CatchUpPlatform.API.News.Interfaces.REST.Resources;
using CatchUpPlatform.API.News.Interfaces.REST.Transform;
using CatchUpPlatform.API.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace CatchUpPlatform.API.News.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Favorite Sources")]
public class FavoriteSourcesController(
    IFavoriteSourceCommandService favoriteSourceCommandService,
    IFavoriteSourceQueryService favoriteSourceQueryService,
    IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Get Favorite Source by Id",
        Description = "Retrieve a favorite source by its unique identifier.",
        OperationId = "GetFavoriteSourceById")]
    [SwaggerResponse(200, "Returns the favorite source by its unique identifier.")]
    [SwaggerResponse(404, "Favorite source not found.")]
    public async Task<IActionResult> GetFavoriteSourceById(int id)
    {
        var getFavoriteSourceById = new GetFavoriteSourceByIdQuery(id);
        var result = await favoriteSourceQueryService.Handle(getFavoriteSourceById);
        if (result is null) return NotFound();
        var resource = FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resource);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create Favorite Source",
        Description = "Create a new favorite source.",
        OperationId = "CreateFavoriteSource")]
    [SwaggerResponse(201, "Favorite source created successfully.", typeof(FavoriteSourceResource))]
    [SwaggerResponse(400, "Invalid input data.")]
    [SwaggerResponse(409, "Favorite source with this SourceId already exists for the NewsAPI Key.")]
    public async Task<IActionResult> CreateFavoriteSource([FromBody] CreateFavoriteSourceResource resource)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var createFavoriteSourceCommand = CreateFavoriteSourceCommandFromResourceAssembler
            .ToCommandFromResource(resource);
        try
        {
            var result = await favoriteSourceCommandService.Handle(createFavoriteSourceCommand);
            if (result is null) return Conflict(localizer["NewsFavoriteSourceDuplicated"].Value);
            return CreatedAtAction(nameof(GetFavoriteSourceById), 
                new { id = result.Id }, 
                FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result));
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(localizer["NewsFavoriteSourceDuplicated"].Value);
        }
        catch
        {
            return BadRequest();
        }
    }

    private async Task<IActionResult> GetAllFavoriteSourcesByNewsApiKey(string newsApiKey)
    {
        var getAllFavoriteSourcesByNewsApiKey = new GetAllFavoriteSourcesByNewsApiKeyQuery(newsApiKey);
        var result  = await favoriteSourceQueryService.Handle(getAllFavoriteSourcesByNewsApiKey);
        var resources = result
            .Select(FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    private async Task<IActionResult> GetFavoriteSourceByNewsApiKeyAndSourceId(string newsApiKey, string sourceId)
    {
        var getFavoriteSourceByNewsApiKeyAndSourceId =
            new GetFavoriteSourceByNewsApiKeyAndSourceIdQuery(newsApiKey, sourceId);
        var result = await favoriteSourceQueryService.Handle(getFavoriteSourceByNewsApiKeyAndSourceId);
        if (result is null) return NotFound();
        var resource = FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resource);
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get Favorite Sources",
        Description = "Retrieve favorite sources by NewsAPI Key and optionally by SourceId.",
        OperationId = "GetFavoriteSourcesFromQuery")]
    [SwaggerResponse(200, "Returns the favorite sources based on the provided query parameters.", typeof(FavoriteSourceResource))]
    [SwaggerResponse(404, "Favorite source not found.")]
    public async Task<IActionResult> GetFavoriteSourcesFromQuery([FromQuery] string newsApiKey,
        [FromQuery] string? sourceId)
    {
        return string.IsNullOrEmpty(sourceId)
            ? await GetAllFavoriteSourcesByNewsApiKey(newsApiKey)
            : await GetFavoriteSourceByNewsApiKeyAndSourceId(newsApiKey, sourceId);
    }
}