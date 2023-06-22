using Web.Features.Examples.Errors;
using Web.Features.Examples.Home;
using Web.Features.Examples.RealTime;
using Web.Util.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.AddProblemDetails();
builder.Services.AddSignalR();

builder.AddModule<RealTimeModule>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));
}

var app = builder.Build();

if (builder.Environment.IsProduction())
{
    app.UseExceptionHandler();
}

app.UseResponseCompression();
app.UseHttpsRedirection();

if (builder.Environment.IsDevelopment())
{
    app.UseCors();
}

app.UseStaticFiles(new StaticFileOptions()
{
    OnPrepareResponse = context => context.Context.Response.GetTypedHeaders().CacheControl = new()
    {
        Public = true,
        MaxAge = TimeSpan.FromDays(30)
    }
});
app.UseRouting();

app.MapModule<HomeModule>();
app.MapModule<ErrorModule>();
app.MapModule<RealTimeModule>();

app.MapFallbackToFile("index.html");
app.Run();