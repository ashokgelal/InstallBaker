using System;
using AshokGelal.InstallBaker.Events;
using AshokGelal.InstallBaker.Services;

namespace AshokGelal.InstallBaker.ViewModels
{
    internal class ToolWindowViewModel : DietMvvm.ViewModelBase, IDisposable
    {
        private readonly InstallBakerEventAggregator _eventAggregator;
        private readonly DependenciesRegistry _dependenciesRegistry;

        public ToolWindowViewModel(InstallBakerEventAggregator eventAggregator, DependenciesRegistry dependenciesRegistry)
        {
            _eventAggregator = eventAggregator;
            _dependenciesRegistry = dependenciesRegistry;
            HookEvents();
        }

        private void HookEvents()
        {
            _eventAggregator.BuildFinished.ItsEvent += BuildFinishedEventHandler;
            _dependenciesRegistry.DependenciesRegistryUpdateEvent.ItsEvent += DependenciesRegistry_DependenciesRegistryUpdateEventHandler;
        }

        private void DependenciesRegistry_DependenciesRegistryUpdateEventHandler(object sender, EventArgs e)
        {
        }

        private void BuildFinishedEventHandler(object sender, EventArgs e)
        {
        }

        public void Dispose()
        {
            UnHookEvents();
        }

        private void UnHookEvents()
        {
            _eventAggregator.BuildFinished.ItsEvent -= BuildFinishedEventHandler;
        }
    }
}
