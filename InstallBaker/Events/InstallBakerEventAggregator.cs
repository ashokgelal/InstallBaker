using System.Collections.Generic;
using DietMvvm.Events;

namespace AshokGelal.InstallBaker.Events
{
    internal class InstallBakerEventAggregator : EventAggregator
    {
        public readonly GenericEventHandler<BuildConfig> IndividualProjectBuildStarted = new GenericEventHandler<BuildConfig>();
        public readonly GenericEventHandler<BuildConfig> IndividualProjectBuildFinished = new GenericEventHandler<BuildConfig>();
        public readonly GenericEventHandler<BuildConfig> StartupProjectBuildStarted = new GenericEventHandler<BuildConfig>();
        public readonly GenericEventHandler<BuildConfig> StartupProjectBuildFinished = new GenericEventHandler<BuildConfig>();
        public readonly GenericEventHandler<BuildConfig> InstallerProjectBuildStarted = new GenericEventHandler<BuildConfig>();
        public readonly GenericEventHandler<BuildConfig> InstallerProjectBuildFinished = new GenericEventHandler<BuildConfig>();
        public SingleArgsEventHandler<List<ProjectInfo>> BuildStarted = new SingleArgsEventHandler<List<ProjectInfo>>();
        public EmptyArgsEventHandler BuildFinished = new EmptyArgsEventHandler();
    }
}
