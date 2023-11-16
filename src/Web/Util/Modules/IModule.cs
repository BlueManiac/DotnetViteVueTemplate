namespace Web.Util.Modules;
public interface IModule
{
    static abstract void AddServices(WebApplicationBuilder builder);
    static abstract void MapRoutes(WebApplication app);
}
