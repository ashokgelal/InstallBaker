using System;
using System.Collections.Generic;
using System.IO;

using AshokGelal.InstallBaker.Events;
using AshokGelal.InstallBaker.Helpers;
using AshokGelal.InstallBaker.Models;

using EnvDTE;

namespace AshokGelal.InstallBaker.Services
{
    internal class InstallerProjectManagementService : BaseEventsService
    {
        #region Fields

        public static readonly string BakeOutputFileName = "bake.xml";
        public static readonly string InitialOutputFile = "../{0}/bin/Debug/{0}.exe";
        public static readonly string WixOutputFileName = "Product.wxs";
        public static readonly string WixProjectGuid = "{930C7802-8A8C-48F9-8165-68863BCCD9DD}";
        private BakeMetadata _bakeMetadata;
        private readonly Solution _currentSolution;

        #endregion Fields

        #region Properties

        public string ItsBakeFile
        {
            get; private set;
        }

        public ProjectInfo ItsInstallerProjectInfo
        {
            get;
            private set;
        }

        public string ItsInstallerProjectName
        {
            get;
            private set;
        }

        public string ItsWixFile
        {
            get; private set;
        }

        #endregion Properties

        #region Event Fields

        private readonly SolutionEvents _solutionEvents;

        #endregion Event Fields

        #region Constructors

        public InstallerProjectManagementService(InstallBakerEventAggregator eventAggregator, SolutionEvents solutionEvents, Solution solution)
            : base(eventAggregator)
        {
            _solutionEvents = solutionEvents;
            _currentSolution = solution;
            LoadInstallerProject();
            HookEvents();
        }

        #endregion Constructors

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                if (disposing && _solutionEvents != null)
                    UnHookEvents();
            }
        }

        #endregion Dispose

        #region Public Methods

        public void AddNewFile(string fullPath)
        {
            var relativePath = fullPath.GetRelativePath(ItsWixFile);
            _bakeMetadata.ItsMainExecutableComponent.ItsBakeFiles.Add(new BakeFile(string.Format("FI_{0}", Path.GetFileNameWithoutExtension(relativePath)), relativePath, _bakeMetadata.ItsMainExecutableComponent));
            UpdateBakeFile();
        }

        public void RemoveFile(string fullPath)
        {
            var relativePath = fullPath.GetRelativePath(ItsWixFile);
            _bakeMetadata.ItsMainExecutableComponent.ItsBakeFiles.RemoveAll(e => e.ItsSource.Equals(relativePath));
            UpdateBakeFile();
        }

        public void UpdateBakeFile()
        {
            XmlFileParserService.UpdateBakeFile(_bakeMetadata, ItsBakeFile);
            XmlFileParserService.WriteWixFile(_bakeMetadata, ItsWixFile);
        }

        #endregion Public Methods

        #region Private Methods

        private void BakeMetaDataUpdatedEventHandler(object sender, EventArgs e)
        {
            XmlFileParserService.UpdateBakeFile(_bakeMetadata, ItsBakeFile);
            XmlFileParserService.WriteWixFile(_bakeMetadata, ItsWixFile);
        }

        private void HookEvents()
        {
            _solutionEvents.ProjectAdded += SolutionEvents_ProjectAdded;
            _solutionEvents.Opened += SolutionEvents_SolutionOpened;
            _eventAggregator.BakeMetaDataUpdated.ItsEvent += BakeMetaDataUpdatedEventHandler;
        }

        private void InitializeBakeFile(Project project)
        {
            ItsInstallerProjectName = project.Name;
            var paths = Utilities.GetOutputPath(project);
            ItsInstallerProjectInfo = new ProjectInfo(project.Name, paths);
            ItsBakeFile = Path.Combine(paths.ItsRootPath, BakeOutputFileName);
            ItsWixFile = Path.Combine(paths.ItsRootPath, WixOutputFileName);
            BakeMetadata metadata;

            if (!File.Exists(ItsBakeFile))
            {
                using (var vf = File.Create(ItsBakeFile))
                {
                    var mainProjectFile = Path.GetFileNameWithoutExtension(((object[])(_currentSolution.SolutionBuild.StartupProjects))[0].ToString());
                    metadata = new BakeMetadata
                                   {
                                           ItsCompanyName = "MetaGeek, LLC",
                                           ItsIconName = "icon.ico",
                                           ItsMainExecutableDisplayName = string.Format("{0}.exe", mainProjectFile),
                                           ItsMainExecutableSource = string.Format(InitialOutputFile, mainProjectFile),
                                           ItsManufacturer = "MetaGeek",
                                           ItsProductName = mainProjectFile,
                                           ItsUpgradeCode = Guid.NewGuid(),
                                           ItsMainExecutableComponent = new BakeComponent("MainExecutable", Guid.NewGuid()),
                                           ItsProgramMenuComponent = new BakeComponent("ProgramMenuDir", Guid.NewGuid()),
                                           ItsSubDirectories = new List<BakeDirectory>()
                                       };

                    XmlFileParserService.WriteBakeFile(vf, metadata);
                }
            }
            else
            {
                metadata = XmlFileParserService.ReadBakeFile(ItsBakeFile);
            }

            _bakeMetadata = metadata;
            XmlFileParserService.WriteWixFile(metadata, ItsWixFile);
            _eventAggregator.BakeMetadataAvailable.Raise(this, metadata);
        }

        private void LoadInstallerProject()
        {
            foreach (Project project in _currentSolution.Projects)
            {
                if (project.Kind.Equals(WixProjectGuid, StringComparison.CurrentCultureIgnoreCase))
                {
                    InitializeBakeFile(project);
                    break;
                }
            }
        }

        private void SolutionEvents_ProjectAdded(Project project)
        {
            if (!project.Kind.Equals(WixProjectGuid, StringComparison.CurrentCultureIgnoreCase))
                return;
            InitializeBakeFile(project);
        }

        private void SolutionEvents_SolutionOpened()
        {
            LoadInstallerProject();
        }

        private void UnHookEvents()
        {
            _solutionEvents.ProjectAdded -= SolutionEvents_ProjectAdded;
            _solutionEvents.Opened -= SolutionEvents_SolutionOpened;
            _eventAggregator.BakeMetaDataUpdated.ItsEvent -= BakeMetaDataUpdatedEventHandler;
        }

        #endregion Private Methods
    }
}