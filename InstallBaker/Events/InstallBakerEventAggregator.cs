using System.Collections.Generic;

using AshokGelal.InstallBaker.Models;

using DietMvvm.Events;

namespace AshokGelal.InstallBaker.Events
{
    public class InstallBakerEventAggregator : EventAggregator
    {
        #region Fields

        public readonly SingleArgsEventHandler<BakeMetadata> BakeMetadataAvailable = new SingleArgsEventHandler<BakeMetadata>();

        // Build Related Events
        public readonly EmptyArgsEventHandler BuildFinished = new EmptyArgsEventHandler();
        public readonly SingleArgsEventHandler<List<ProjectInfo>> BuildStarted = new SingleArgsEventHandler<List<ProjectInfo>>();
        public readonly GenericEventHandler<BuildConfig> IndividualProjectBuildFinished = new GenericEventHandler<BuildConfig>();
        public readonly GenericEventHandler<BuildConfig> IndividualProjectBuildStarted = new GenericEventHandler<BuildConfig>();
        public readonly GenericEventHandler<BuildConfig> InstallerProjectBuildFinished = new GenericEventHandler<BuildConfig>();
        public readonly GenericEventHandler<BuildConfig> InstallerProjectBuildStarted = new GenericEventHandler<BuildConfig>();
        public readonly GenericEventHandler<BuildConfig> StartupProjectBuildFinished = new GenericEventHandler<BuildConfig>();
        public readonly GenericEventHandler<BuildConfig> StartupProjectBuildStarted = new GenericEventHandler<BuildConfig>();

        #endregion Fields
    }
}