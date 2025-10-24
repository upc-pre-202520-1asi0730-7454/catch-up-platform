using System.Net.Mime;
using CatchUpPlatform.API.News.Domain.Model.Queries;
using CatchUpPlatform.API.News.Interfaces.REST.Resources;
using CatchUpPlatform.API.News.Interfaces.REST.Transform;
using CatchUpPlatform.API.Shared.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace CatchUpPlatform.API.News.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Favorite Sources")]
public class FavoriteSourcesController(
    IFavoriteSourceCommandService favoriteSourceCommandService,
    IFavoriteSourceQueryService favoriteSourceQueryService
    ) : ControllerBase
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
            if (result is null) return BadRequest();
            return CreatedAtAction(nameof(GetFavoriteSourceById), 
                new { id = result.Id }, 
                FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result));
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict("Favorite source with this SourceId already exists for the NewsAPI Key.");
        }
        catch
        {
            return BadRequest();
        }
    }
}