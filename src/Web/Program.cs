using Microsoft.AspNetCore.Mvc;
using Web.Features.RealTime;
using Web.Util.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.AddProblemDetails();
builder.Services.AddSignalR();

builder.AddModule<RealTimeModule>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed((hosts) => true)));
}

var app = builder.Build();

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions()
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(30)
        };
    }
});
app.UseRouting();

if (builder.Environment.IsDevelopment())
{
    app.UseCors();
}

app.MapModule<RealTimeModule>();

app.MapGet("/hello", () =>
{
    return new { Hello = "Hello World!" };
});

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.MapPost("/hello2", ([FromBody] string data) =>
{
    return new { data };
});

app.MapFallbackToFile("index.html");
app.Run();