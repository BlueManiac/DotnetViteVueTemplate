using Web.Util.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.AddProblemDetails();
builder.Services.AddSignalR();

builder.Services.Configure<HostOptions>(options =>
{
    options.ServicesStartConcurrently = true;
    options.ServicesStopConcurrently = true;
});

builder.AddModules();

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

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    OnPrepareResponse = context => context.Context.Response.GetTypedHeaders().CacheControl = new()
    {
        Public = true,
        MaxAge = TimeSpan.FromDays(30)
    }
});
app.UseRouting();

app.UseAuthorization();

var api = app.MapGroup("/api").RequireAuthorization();

app.MapModules(api);

app.MapFallbackToFile("index.html");
app.Run();