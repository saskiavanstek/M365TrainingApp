using Microsoft.AspNetCore.Http;

public class SessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsStudent(string uniqueId)
    {
        return _httpContextAccessor.HttpContext?.Session.GetString($"IsStudent_{uniqueId}") == "true";
    }
}