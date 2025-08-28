using System.Web.Http;
using TodoApp.Data;
using TodoApp.Data.Repositories;
using TodoApp.Services;
using Unity;
using Unity.Lifetime;
using Unity.WebApi;

namespace TodoApp.Api
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // Register log4net logger for each type
            container.RegisterFactory<log4net.ILog>(c =>
                log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType),
                new HierarchicalLifetimeManager());

            // Register TodoContext
            container.RegisterType<TodoContext>(new HierarchicalLifetimeManager());

            // Register repository with logger injection
            container.RegisterType<ITodoRepository, TodoRepository>(
                new HierarchicalLifetimeManager());

            // Register service with repository injection
            container.RegisterType<ITodoService, TodoService>(
                new HierarchicalLifetimeManager());

            // Register controller with logger injection
            container.RegisterType<TodoApp.Api.Controllers.TodoApiController>(
                );

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}
