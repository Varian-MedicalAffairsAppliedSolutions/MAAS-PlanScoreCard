using Autofac;
using PlanScoreCard.ViewModels;
using PlanScoreCard.Views;
using Prism.Events;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap(Patient patient, Course course, PlanSetup plan, User user, Application app)
        {
            var container = new ContainerBuilder();
            //esapi components.
            container.RegisterInstance<Patient>(patient);
            container.RegisterInstance<Course>(course);
            container.RegisterInstance<PlanSetup>(plan);
            container.RegisterInstance<User>(user);
            container.RegisterInstance<Application>(app);
            //startup components.
            container.RegisterType<MainView>().AsSelf();
            container.RegisterType<MainViewModel>().AsSelf();
            //view models
            container.RegisterType<NavigationViewModel>().AsSelf();
            container.RegisterType<PlanScoreViewModel>().AsSelf();
            container.RegisterType<PluginViewModel>().AsSelf();
            //events
            container.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();


            return container.Build();
        }
    }
}
