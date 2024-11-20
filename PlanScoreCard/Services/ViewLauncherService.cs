using Autofac;
using PlanScoreCard.Views;
using PlanScoreCard.Views.MetricEditors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Services
{
    public class ViewLauncherService
    {
        private readonly IComponentContext Context;

        /// <summary>
        /// Constructor that takes a component context through dependency injection
        /// </summary>
        /// <param name="componentContext"></param>
        public ViewLauncherService(IComponentContext componentContext)
        {
            Context = componentContext;
        }

        /// <summary>
        /// Generate a home view
        /// </summary>
        /// <returns></returns>
        public EditScoreCardView GetEditScoreCardView()
        {
            return GetView<EditScoreCardView>();
        }

        public PluginView GetPluginView()
        {
            return GetView<PluginView>();
        }

        public ScorecardVisualizer.Views.MainView GetVisualizerView()
        {
            return GetView<ScorecardVisualizer.Views.MainView>();
        }

        /// <summary>
        /// Generate a home view
        /// </summary>
        /// <returns></returns>
        public EditDoseAtVolumeView GetEditMetricView_DoseAtVolume()
        {
            return GetView<EditDoseAtVolumeView>();
        }

        public EditVolumeAtDoseView GetEditMetricView_VolumeAtDose()
        {
            return GetView<EditVolumeAtDoseView>();
        }

        public EditDoseValueView GetEditMetricView_DoseValue()
        {
            return GetView<EditDoseValueView>();
        }

        public EditHIView GetEditMetricView_HI()
        {
            return GetView<EditHIView>();
        }
        public EditCIView GetEditMetricView_CI()
        {
            return GetView<EditCIView>();
        }

        public SimpleStructureBuilderView GetStructureBuilderView()
        {
            return GetView<SimpleStructureBuilderView>();
        }

        /// <summary>
        /// Generate a generic view
        /// </summary>
        /// <returns></returns>
        public T GetView<T>()
        {
            return Context.Resolve<T>();
        }

        /// <summary>
        /// Generate a view specified by the passed in type
        /// </summary>
        /// <returns></returns>
        public object GetView(Type type)
        {
            return Context.Resolve(type);
        }
    }
}
