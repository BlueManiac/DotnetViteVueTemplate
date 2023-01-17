var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.AddProblemDetails();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));
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

app.MapGet("/hello", async () =>
{
    await Task.Delay(500);

    return new { Hello = "Hello World!" };
});

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.MapFallbackToFile("index.html");
app.Run();