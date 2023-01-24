namespace Web.Util.Modules;
public interface IModule
{
    static abstract void AddServices(IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration);
    static abstract void MapRoutes(WebApplication app);
}
