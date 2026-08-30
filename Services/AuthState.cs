using Microsoft.AspNetCore.Http;

namespace MtgCollection.Web.Services;

public class AuthState
{
    private const string CookieName = "mtg_api_key";
    public bool IsUnlocked { get; private set; }
    public string? ApiKey { get; private set; }

    public event Action? OnChange;

    public AuthState(IHttpContextAccessor httpContextAccessor)
    {
        var cookieValue = httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
        if (!string.IsNullOrEmpty(cookieValue))
        {
            ApiKey = cookieValue;
            IsUnlocked = true;
        }
    }

    public void Unlock(string apiKey)
    {
        ApiKey = apiKey;
        IsUnlocked = true;
        OnChange?.Invoke();
    }
}