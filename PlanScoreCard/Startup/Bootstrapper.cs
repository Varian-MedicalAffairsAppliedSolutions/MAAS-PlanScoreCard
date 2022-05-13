using Autofac;
using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Services;
using PlanScoreCard.ViewModels;
using PlanScoreCard.ViewModels.MetricEditors;
using PlanScoreCard.Views;
using PlanScoreCard.Views.HelperWindows;
using PlanScoreCard.Views.MetricEditors;
using Prism.Events;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap(Patient patient, Course course, PlanSetup plan, User user, Application app, IEventAggregator eventAggregator)
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
            container.RegisterType<EditScoreCardViewModel>().AsSelf().SingleInstance();
            container.RegisterType<EditDoseAtVolumeViewModel>().AsSelf().SingleInstance();
            container.RegisterType<EditVolumeAtDoseViewModel>().AsSelf().SingleInstance();
            container.RegisterType<EditDoseValueViewModel>().AsSelf().SingleInstance();
            container.RegisterType<EditHIViewModel>().AsSelf().SingleInstance();
            container.RegisterType<EditCIViewModel>().AsSelf().SingleInstance();
            container.RegisterType<StructureBuilderViewModel>().AsSelf();
            container.RegisterType<ScoreCardViewModel>().AsSelf();
            container.RegisterType<BuildStructureViewModel>().AsSelf();
            container.RegisterType<EditModifiedGradientIndexViewModel>().AsSelf().SingleInstance();
            container.RegisterType<EditDoseSubVolumeViewModel>().AsSelf().SingleInstance();

            // views
            container.RegisterType<PluginView>().AsSelf();
            container.RegisterType<EditScoreCardView>().AsSelf();
            container.RegisterType<EditDoseAtVolumeView>().AsSelf();
            container.RegisterType<EditVolumeAtDoseView>().AsSelf();
            container.RegisterType<EditDoseValueView>().AsSelf().SingleInstance();
            container.RegisterType<EditHIView>().AsSelf();
            container.RegisterType<EditCIView>().AsSelf();
            container.RegisterType<StructureBuilderView>().AsSelf();
            container.RegisterType<ScoreCardView>().AsSelf();
            container.RegisterType<ProgressView>().AsSelf().SingleInstance();
            container.RegisterType<EditVolumeAtDoseView>().AsSelf();
            container.RegisterType<BuildStructureView>().AsSelf();
            container.RegisterType<InhomogeneityIndexView>().AsSelf();
            container.RegisterType<EditModifiedGradientIndexView>().AsSelf();
            container.RegisterType<EditDoseSubVolumeView>().AsSelf();


            //events
            container.RegisterInstance<IEventAggregator>(eventAggregator);
            container.RegisterType<CancelEvent>().AsSelf();

            // services
            container.RegisterType<PluginViewService>().AsSelf();
            container.RegisterType<ViewLauncherService>().AsSelf();
            container.RegisterType<ProgressViewService>().AsSelf().SingleInstance();
            container.RegisterType<StructureDictionaryService>().SingleInstance();


            return container.Build();
        }
    }
}
