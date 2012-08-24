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

        private ViewModelCommand<FileEntry> _addFileCommand;
        private BakeMetadata _bakeMetaData;
        private readonly DependenciesRegistry _dependenciesRegistry;
        private readonly InstallBakerEventAggregator _eventAggregator;
        private ObservableCollection<FileEntry> _excludedFileList;
        private ObservableCollection<FileEntry> _includedFileList;
        private ObservableCollection<FileEntry> _newFileList;
        private ViewModelCommand<FileEntry> _removeFileCommand;
        private ViewModelCommand _updateMetadataCommand;

        #endregion Fields

        #region Properties

        public ViewModelCommand<FileEntry> ItsAddFileCommand
        {
            get { return _addFileCommand; }
            set
            {
                _addFileCommand = value;
                NotifyPropertyChanged(()=>ItsAddFileCommand);
            }
        }

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

        public ViewModelCommand<FileEntry> ItsRemoveFileCommand
        {
            get { return _removeFileCommand; }
            set
            {
                _removeFileCommand = value;
                NotifyPropertyChanged(()=>ItsRemoveFileCommand);
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
            Initialize();
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

        private void AddFileCommandHandler(FileEntry file)
        {
            _dependenciesRegistry.IncludeFile(file);
        }

        private void AddRemoveCommandHandler(FileEntry file)
        {
            _dependenciesRegistry.ExcludeFile(file);
        }

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

        private void Initialize()
        {
            ItsUpdateMetadataCommand = new ViewModelCommand(UpdateMetadataCommandHandler, CanUpdateMetadata);
            ItsAddFileCommand = new ViewModelCommand<FileEntry>(AddFileCommandHandler);
            ItsRemoveFileCommand = new ViewModelCommand<FileEntry>(AddRemoveCommandHandler);
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