using MtgCollection.Web.Components;
using MtgCollection.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents().AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    }); ;

builder.Services.AddHttpClient<CardApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5142/");
});

builder.Services.AddScoped<AuthState>();
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/api/unlock", (HttpContext context, UnlockRequest request) =>
{
    context.Response.Cookies.Append("mtg_api_key", request.ApiKey, new CookieOptions
    {
        HttpOnly = true,
        Secure = false, // set true once you're serving over HTTPS
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddHours(24)
    });
    return Results.Ok();
});


app.Run();
record UnlockRequest(string ApiKey);