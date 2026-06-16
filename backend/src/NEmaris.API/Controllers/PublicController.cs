using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEmaris.Application.Interfaces;

namespace NEmaris.API.Controllers;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;

    public PublicController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    [HttpGet("menu")]
    public async Task<IActionResult> GetMenu()
    {
        var items = await _menuItemService.GetPublicMenuAsync();
        return Ok(items);
    }
}
