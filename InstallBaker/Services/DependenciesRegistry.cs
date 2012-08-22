using System;
using System.Collections.Generic;
using AshokGelal.InstallBaker.Events;
using AshokGelal.InstallBaker.Models;
using CWEngine.Shared.FileSystemService;
using CWEngine.Shared.FileSystemService.Models;
using DietMvvm.Events;

namespace AshokGelal.InstallBaker.Services
{
    internal class DependenciesRegistry : IDisposable
    {
        private readonly InstallBakerEventAggregator _eventAggregator;
        private FileSystemService _fileSystemSerivce;
        public EmptyArgsEventHandler DependenciesRegistryUpdateEvent;
        public Dictionary<int, FileEntry> ItsNewFileEntriesDict { get; private set; }
        public Dictionary<int, FileEntry> ItsIncludedFileEntriesDict { get; private set; }
        public Dictionary<int, FileEntry> ItsExcludedFileEntriesDict { get; private set; }

        public DependenciesRegistry(InstallBakerEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Initialize();
            HookEvents();
        }

        private void Initialize()
        {
            _fileSystemSerivce = new FileSystemService();
            DependenciesRegistryUpdateEvent = new EmptyArgsEventHandler();
            ItsNewFileEntriesDict = new Dictionary<int, FileEntry>();
            ItsIncludedFileEntriesDict = new Dictionary<int, FileEntry>();
            ItsExcludedFileEntriesDict = new Dictionary<int, FileEntry>();
        }

        private void HookEvents()
        {
            _eventAggregator.BuildStarted.ItsEvent +=BuildStartedEventHandler;
            _eventAggregator.StartupProjectBuildFinished.ItsEvent += StartupProjectBuildFinishedEventHandler;
            _fileSystemSerivce.ItsFileEntryAvailableEvent.ItsEvent +=FileSystemService_FileEntryAvailableEventHandler;
            _fileSystemSerivce.ItsScanStatusChangedEvent.ItsEvent += FileSystemService_ScanStatusChangedEventHandler;
        }

        private void FileSystemService_ScanStatusChangedEventHandler(object sender, SingleEventArgs<ScanStatus> e)
        {
            switch (e.ItsValue)
            {
                case ScanStatus.Stopped:
                case ScanStatus.Started:
                case ScanStatus.Cancelled:
                    ItsNewFileEntriesDict.Clear();
                    break;
                case ScanStatus.Completed:
                    break;
            }
            RaiseRegistryUpdateEvent();
        }

        private void RaiseRegistryUpdateEvent()
        {
            DependenciesRegistryUpdateEvent.Raise(this);
        }

        private void FileSystemService_FileEntryAvailableEventHandler(object sender, SingleEventArgs<FileEntry> e)
        {
            var hash = e.ItsValue.GetHashCode();
            if(!ItsIncludedFileEntriesDict.ContainsKey(hash))
                ItsNewFileEntriesDict.Add(hash, e.ItsValue);
        }

        private void StartupProjectBuildFinishedEventHandler(object sender, BuildConfig e)
        {
            _fileSystemSerivce.StartScanAsync(e.ItsProjectInfo.ItsOutputDir);
        }

        private void BuildStartedEventHandler(object sender, SingleEventArgs<List<ProjectInfo>> e)
        {
        }

        public void Dispose()
        {
            UnHookEvents();
        }

        private void UnHookEvents()
        {
            _eventAggregator.BuildStarted.ItsEvent -=BuildStartedEventHandler;
            _eventAggregator.StartupProjectBuildStarted.ItsEvent -= StartupProjectBuildFinishedEventHandler;
            _fileSystemSerivce.ItsFileEntryAvailableEvent.ItsEvent -=FileSystemService_FileEntryAvailableEventHandler;
        }
    }
}
