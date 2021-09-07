using Autofac;
using PlanScoreCard.Views;
using PlanScoreCard.Views.Plugins;
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

        public PluginView GetPluginView()
        {
            return GetView<PluginView>();
        }

        public PluginConsoleView GetPluginConsoleView()
        {
            return GetView<PluginConsoleView>();
        }

        public PluginPlotView GetPluginPlotView()
        {
            return GetView<PluginPlotView>();
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
