using Autofac;
using PlanScoreCard.Services;
using PlanScoreCard.ViewModels;
using PlanScoreCard.ViewModels.MetricEditors;
using PlanScoreCard.Views;
using PlanScoreCard.Views.MetricEditors;
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

            //view models
            container.RegisterType<NavigationViewModel>().AsSelf();
            container.RegisterType<PlanScoreViewModel>().AsSelf();
            container.RegisterType<PluginViewModel>().AsSelf();
            container.RegisterType<MainViewModel>().AsSelf();
            container.RegisterType<EditScoreCardViewModel>().AsSelf();
            container.RegisterType<EditDoseAtVolumeViewModel>().AsSelf();
            container.RegisterType<EditVolumeAtDoseViewModel>().AsSelf();
            container.RegisterType<EditDoseValueViewModel>().AsSelf();
            container.RegisterType<EditHIViewModel>().AsSelf();
            container.RegisterType<EditCIViewModel>().AsSelf();
            container.RegisterType<StructureBuilderViewModel>().AsSelf();
            container.RegisterType<ScoreCardViewModel>().AsSelf();


            // views
            container.RegisterType<MainView>().AsSelf();
            container.RegisterType<PluginView>().AsSelf();
            container.RegisterType<EditScoreCardView>().AsSelf();
            container.RegisterType<EditDoseAtVolumeView>().AsSelf();
            container.RegisterType<EditVolumeAtDoseView>().AsSelf();
            container.RegisterType<EditDoseValueView>().AsSelf();
            container.RegisterType<EditHIView>().AsSelf();
            container.RegisterType<EditCIView>().AsSelf();
            container.RegisterType<StructureBuilderView>().AsSelf();
            container.RegisterType<ScoreCardView>().AsSelf();

            //events
            container.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            // services
            container.RegisterType<PluginViewService>().AsSelf();
            container.RegisterType<ViewLauncherService>().AsSelf();

            return container.Build();
        }
    }
}
