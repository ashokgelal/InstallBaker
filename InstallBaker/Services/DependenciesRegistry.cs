using System;
using System.Collections.Generic;
using System.IO;

using AshokGelal.InstallBaker.Events;
using AshokGelal.InstallBaker.Models;

using CWEngine.Shared.FileSystemService;
using CWEngine.Shared.FileSystemService.Helpers;
using CWEngine.Shared.FileSystemService.Models;

using DietMvvm.Events;

namespace AshokGelal.InstallBaker.Services
{
    internal class DependenciesRegistry : IDisposable
    {
        #region Fields

        private BakeMetadata _bakeMetadata;
        private readonly InstallBakerEventAggregator _eventAggregator;
        private List<string> _excludedExtensions;
        private FileSystemService _fileSystemSerivce;
        private List<FileEntry> _tempFiles;

        #endregion Fields

        #region Properties

        public List<FileEntry> ItsExcludedFileEntries
        {
            get; private set;
        }

        public Dictionary<int, FileEntry> ItsIncludedFileEntriesDict
        {
            get; private set;
        }

        public InstallerProjectManagementService ItsInstallerProjectManagementService
        {
            get; set;
        }

        public List<FileEntry> ItsNewFileEntries
        {
            get; private set;
        }

        #endregion Properties

        #region Event Fields

        public EmptyArgsEventHandler DependenciesRegistryUpdateEvent;

        #endregion Event Fields

        #region Constructors

        public DependenciesRegistry(InstallBakerEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Initialize();
            HookEvents();
        }

        #endregion Constructors

        #region Dispose

        public void Dispose()
        {
            UnHookEvents();
            _fileSystemSerivce.StopScan();
        }

        #endregion Dispose

        #region Public Methods

        public void ExcludeFile(FileEntry file)
        {
            // TODO: check for directory
            ItsIncludedFileEntriesDict.Remove(file.GetHashCode());
            ItsExcludedFileEntries.Add(file);
            ItsNewFileEntries.Remove(file);
            // TODO: update xml
            RaiseRegistryUpdateEvent();
            ItsInstallerProjectManagementService.RemoveFile(file.FullPath);
        }

        public void IncludeFile(FileEntry file)
        {
            // TODO: check for directory
            ItsExcludedFileEntries.Remove(file);
            ItsIncludedFileEntriesDict.Add(file.GetHashCode(), file);
            ItsNewFileEntries.Remove(file);
            // TODO: update xml
            RaiseRegistryUpdateEvent();
            ItsInstallerProjectManagementService.AddNewFile(file.FullPath);
        }

        #endregion Public Methods

        #region Private Methods

        private void BakeMetadataAvailableEventHandler(object sender, SingleEventArgs<BakeMetadata> e)
        {
            _bakeMetadata = e.ItsValue;
        }

        private void BuildStartedEventHandler(object sender, SingleEventArgs<List<ProjectInfo>> e)
        {
            ItsNewFileEntries.Clear();
        }

        private void FileSystemService_FileEntryAvailableEventHandler(object sender, SingleEventArgs<FileEntry> e)
        {
            // TODO: allow adding a directory
            if (e.ItsValue.FileEntryType == FileEntryType.Directory)
                return;
            var hash = e.ItsValue.GetHashCode();
            if(!ItsIncludedFileEntriesDict.ContainsKey(hash))
                _tempFiles.Add(e.ItsValue);
        }

        private void FileSystemService_ScanStatusChangedEventHandler(object sender, SingleEventArgs<ScanStatus> e)
        {
            switch (e.ItsValue)
            {
                case ScanStatus.Started:
                    ItsNewFileEntries.Clear();
                    _tempFiles.Clear();
                    break;
                case ScanStatus.Completed:
                    FilterFiles();
                    break;
            }

            RaiseRegistryUpdateEvent();
        }

        private void FilterFiles()
        {
            var lists = RegExFileEntrieFilterService.FilterFileEntries(_excludedExtensions, _tempFiles);
            foreach (var entry in lists.ItsNonMatchedFileEntries)
            {
                if(!ItsNewFileEntries.Contains(entry))
                {
                    if(!entry.DisplayTitle.Equals(_bakeMetadata.ItsMainExecutableDisplayName, StringComparison.CurrentCultureIgnoreCase))
                      ItsNewFileEntries.Add(entry);
                }
            }

            ItsExcludedFileEntries = lists.ItsMatchedFileEntries;
        }

        private void HookEvents()
        {
            _eventAggregator.BuildStarted.ItsEvent +=BuildStartedEventHandler;
            _eventAggregator.StartupProjectBuildFinished.ItsEvent += StartupProjectBuildFinishedEventHandler;
            _eventAggregator.BakeMetadataAvailable.ItsEvent +=BakeMetadataAvailableEventHandler;
            _fileSystemSerivce.ItsFileEntryAvailableEvent.ItsEvent +=FileSystemService_FileEntryAvailableEventHandler;
            _fileSystemSerivce.ItsScanStatusChangedEvent.ItsEvent += FileSystemService_ScanStatusChangedEventHandler;
        }

        private void Initialize()
        {
            _fileSystemSerivce = new FileSystemService();
            DependenciesRegistryUpdateEvent = new EmptyArgsEventHandler();
            ItsNewFileEntries = new List<FileEntry>();
            _tempFiles = new List<FileEntry>();
            ItsIncludedFileEntriesDict = new Dictionary<int, FileEntry>();
            ItsExcludedFileEntries = new List<FileEntry>();
            _excludedExtensions = new List<string> { "*.pdb", "*.manifest", "*.vshost.exe.*","*.vshost.exe", "*.xml"};
        }

        private void RaiseRegistryUpdateEvent()
        {
            DependenciesRegistryUpdateEvent.Raise(this);
        }

        private void StartupProjectBuildFinishedEventHandler(object sender, BuildConfig e)
        {
            _fileSystemSerivce.StartScanAsync(e.ItsProjectInfo.ItsProjectPaths.ItsOutputPath);
        }

        private void UnHookEvents()
        {
            _eventAggregator.BuildStarted.ItsEvent -=BuildStartedEventHandler;
            _eventAggregator.StartupProjectBuildStarted.ItsEvent -= StartupProjectBuildFinishedEventHandler;
            _eventAggregator.BakeMetadataAvailable.ItsEvent -=BakeMetadataAvailableEventHandler;
            _fileSystemSerivce.ItsFileEntryAvailableEvent.ItsEvent -=FileSystemService_FileEntryAvailableEventHandler;
            _fileSystemSerivce.ItsScanStatusChangedEvent.ItsEvent -= FileSystemService_ScanStatusChangedEventHandler;
        }

        #endregion Private Methods
    }
}