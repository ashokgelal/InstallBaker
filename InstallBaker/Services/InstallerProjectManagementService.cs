using System;
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
        public static readonly string WixOutputFileName = "Product.wxs";
        public static readonly string WixProjectGuid = "{930C7802-8A8C-48F9-8165-68863BCCD9DD}";
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

        #region Private Methods

        private void HookEvents()
        {
            _solutionEvents.ProjectAdded += SolutionEvents_ProjectAdded;
            _solutionEvents.Opened += SolutionEvents_SolutionOpened;
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
                                           ItsMainExecutableDisplayName = mainProjectFile,
                                           ItsMainExecutableSource = string.Format("{0}.exe", mainProjectFile),
                                           ItsManufacturer = "MetaGeek",
                                           ItsProductName = "inSSIDer 3",
                                           ItsUpgradeCode = Guid.NewGuid(),
                                       };

                    XmlFileParserService.WriteBakeFile(vf, metadata);
                }
            }
            else
            {
                metadata = XmlFileParserService.ReadBakeFile(ItsBakeFile);
            }

            _eventAggregator.BakeMetadataAvailable.Raise(this, metadata);

            //            XmlFileParserService.WriteWixFile(metadata, ItsWixFile);
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
        }

        #endregion Private Methods
    }
}