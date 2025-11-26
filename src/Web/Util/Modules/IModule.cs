namespace Web.Util.Modules;

public interface IModule
{
    static virtual void AddServices(WebApplicationBuilder builder) { }
    static virtual void MapRoutes(IEndpointRouteBuilder routes) { }
}
