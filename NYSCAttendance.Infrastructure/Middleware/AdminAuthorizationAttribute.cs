using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Infrastructure.Middleware;

public sealed class AdminAuthorizationAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _permission;
    public AdminAuthorizationAttribute(string permission)
    {
        _permission = permission;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        //Get the users permission
        //Check if the user has the permission to manage other admin users
        var _context = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var cancellationToken = context.HttpContext.RequestAborted;

        var userIdentifier = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdentifier))
        {
            context.Result = new BadRequestObjectResult(new BaseResponse(false, "You do not have the authorization to perform this action."));
            return;
        }

        if (!long.TryParse(userIdentifier, out long userId))
        {
            context.Result = new BadRequestObjectResult(new BaseResponse(false, "Application ran into an error."));
            return;
        }

        var userPermissions = await (from userClaim in _context.UserClaims
                                     where userClaim.UserId == userId
                                     select new
                                     {
                                         userClaim.ClaimValue
                                     }).ToArrayAsync(cancellationToken);

        if (!userPermissions.Any(x => x.ClaimValue == _permission))
        {
            context.Result = new BadRequestObjectResult(new
            {
                status = false,
                message = "You are not authorized to perform this action"
            });
            return;
        }

        await next();
    }
}
