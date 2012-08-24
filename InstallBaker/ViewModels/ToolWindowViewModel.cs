using System;
using System.Collections.ObjectModel;

using AshokGelal.InstallBaker.Events;
using AshokGelal.InstallBaker.Models;
using AshokGelal.InstallBaker.Services;

using CWEngine.Shared.FileSystemService.Models;

using DietMvvm;

namespace AshokGelal.InstallBaker.ViewModels
{
    internal class ToolWindowViewModel : ViewModelBase, IDisposable
    {
        #region Fields

        private BakeMetadata _bakeMetaData;
        private readonly DependenciesRegistry _dependenciesRegistry;
        private readonly InstallBakerEventAggregator _eventAggregator;
        private ObservableCollection<FileEntry> _excludedFileList;
        private ObservableCollection<FileEntry> _includedFileList;
        private ObservableCollection<FileEntry> _newFileList;
        private ViewModelCommand _updateMetadataCommand;

        #endregion Fields

        #region Properties

        public BakeMetadata ItsBakeMetaData
        {
            get { return _bakeMetaData; }
            private set
            {
                _bakeMetaData = value;
                NotifyPropertyChanged(()=>ItsBakeMetaData);
            }
        }

        public ObservableCollection<FileEntry> ItsExcludedFileList
        {
            get { return _excludedFileList; }
            set
            {
                _excludedFileList = value;
                NotifyPropertyChanged(() => ItsExcludedFileList);
            }
        }

        public ObservableCollection<FileEntry> ItsIncludedFileList
        {
            get { return _includedFileList; }
            set
            {
                _includedFileList = value;
                NotifyPropertyChanged(()=>ItsIncludedFileList);
            }
        }

        public ObservableCollection<FileEntry> ItsNewFileList
        {
            get { return _newFileList; }
            set
            {
                _newFileList = value;
                NotifyPropertyChanged(() => ItsNewFileList);
            }
        }

        public ViewModelCommand ItsUpdateMetadataCommand
        {
            get { return _updateMetadataCommand; }
            set
            {
                _updateMetadataCommand = value;
                NotifyPropertyChanged(()=>ItsUpdateMetadataCommand);
            }
        }

        #endregion Properties

        #region Constructors

        public ToolWindowViewModel(InstallBakerEventAggregator eventAggregator, DependenciesRegistry dependenciesRegistry)
        {
            _eventAggregator = eventAggregator;
            _dependenciesRegistry = dependenciesRegistry;
            ItsUpdateMetadataCommand = new ViewModelCommand(UpdateMetadataCommandHandler, CanUpdateMetadata);
            HookEvents();
        }

        #endregion Constructors

        #region Dispose

        public void Dispose()
        {
            UnHookEvents();
        }

        #endregion Dispose

        #region Private Methods

        private void BakeMetadataAvailableEventHandler(object sender, DietMvvm.Events.SingleEventArgs<BakeMetadata> e)
        {
            ItsBakeMetaData = e.ItsValue;
        }

        private void BuildFinishedEventHandler(object sender, EventArgs e)
        {
        }

        private bool CanUpdateMetadata()
        {
            return true;
        }

        private void DependenciesRegistry_DependenciesRegistryUpdateEventHandler(object sender, EventArgs e)
        {
            ItsNewFileList = new ObservableCollection<FileEntry>(_dependenciesRegistry.ItsNewFileEntries);
            ItsIncludedFileList = new ObservableCollection<FileEntry>(_dependenciesRegistry.ItsIncludedFileEntriesDict.Values);
            ItsExcludedFileList = new ObservableCollection<FileEntry>(_dependenciesRegistry.ItsExcludedFileEntries);
        }

        private void HookEvents()
        {
            _eventAggregator.BuildFinished.ItsEvent += BuildFinishedEventHandler;
            _eventAggregator.BakeMetadataAvailable.ItsEvent += BakeMetadataAvailableEventHandler;
            _dependenciesRegistry.DependenciesRegistryUpdateEvent.ItsEvent += DependenciesRegistry_DependenciesRegistryUpdateEventHandler;
        }

        private void UnHookEvents()
        {
            _eventAggregator.BuildFinished.ItsEvent -= BuildFinishedEventHandler;
        }

        private void UpdateMetadataCommandHandler()
        {
            _eventAggregator.BakeMetaDataUpdated.Raise(this);
        }

        #endregion Private Methods
    }
}