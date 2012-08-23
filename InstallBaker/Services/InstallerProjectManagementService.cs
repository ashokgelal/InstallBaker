using System;
using System.IO;
using System.Xml.Linq;

using AshokGelal.InstallBaker.Events;
using AshokGelal.InstallBaker.Helpers;
using AshokGelal.InstallBaker.Models;

using EnvDTE;

namespace AshokGelal.InstallBaker.Services
{
    internal class InstallerProjectManagementService : BaseEventsService
    {
        #region Fields

        public static readonly string BakeFileName = "bake.xml";
        public static readonly string WixGuid = "{930C7802-8A8C-48F9-8165-68863BCCD9DD}";
        private readonly Solution _currentSolution;

        #endregion Fields

        #region Properties

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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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
            var bakeFile = Path.Combine(paths.ItsRootPath, BakeFileName);
            BakeMetadata metadata;

            if (!File.Exists(bakeFile))
            {
                using (var vf = File.Create(bakeFile))
                {
                    metadata = new BakeMetadata()
                                       {
                                           ItsCompanyName = "MetaGeek, LLC",
                                           ItsIconName = "icon.ico",
                                           ItsMainExecutableDisplayName = "inSSIDer3.exe",
                                           ItsMainExecutableSource = "../output.exe",
                                           ItsManufacturer = "MetaGeek",
                                           ItsProductName = "inSSIDer 3",
                                           ItsUpgradeCode = Guid.NewGuid(),
                                       };
                    WriteBakeFile(vf, metadata);
                }
            }
            else
            {
                metadata = ReadBakeFile(bakeFile);
            }

            WriteWixFile(metadata);
        }

        private void WriteWixFile(BakeMetadata metadata)
        {
        }

        private void LoadInstallerProject()
        {
            foreach (Project project in _currentSolution.Projects)
            {
                if (project.Kind.Equals(WixGuid, StringComparison.CurrentCultureIgnoreCase))
                {
                    InitializeBakeFile(project);
                    break;
                }
            }
        }

        private BakeMetadata ReadBakeFile(string bakeFile)
        {
            var xdoc = XDocument.Load(bakeFile);
            var metadata = new BakeMetadata();

            try
            {
                var root = xdoc.Element("metadata");
                // ReSharper disable PossibleNullReferenceException
                var company_name = root.Element("company_name").Value;
                var icon_name = root.Element("icon_name").Value;
                var executable_display_name = root.Element("executable_display_name").Value;
                var executable_source = root.Element("executable_source").Value;
                var manufacturer = root.Element("manufacturer").Value;
                var product_name = root.Element("product_name").Value;
                var upgrade_code = root.Element("upgrade_code").Value;
                metadata.ItsCompanyName = company_name;
                metadata.ItsIconName = icon_name;
                metadata.ItsMainExecutableDisplayName = executable_display_name;
                metadata.ItsMainExecutableSource = executable_source;
                metadata.ItsManufacturer = manufacturer;
                metadata.ItsProductName = product_name;
                metadata.ItsUpgradeCode = new Guid(upgrade_code);
                // ReSharper restore PossibleNullReferenceException
                return metadata;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        private void SolutionEvents_ProjectAdded(Project project)
        {
            if (!project.Kind.Equals(WixGuid, StringComparison.CurrentCultureIgnoreCase))
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

        private void WriteBakeFile(Stream vf, BakeMetadata data)
        {
            var xdoc = new XDocument();
            var metadata = new XElement("metadata");
            metadata.Add(new XElement("company_name", data.ItsCompanyName));
            metadata.Add(new XElement("icon_name", data.ItsIconName));
            metadata.Add(new XElement("executable_display_name", data.ItsMainExecutableDisplayName));
            metadata.Add(new XElement("executable_source", data.ItsMainExecutableSource));
            metadata.Add(new XElement("manufacturer", data.ItsManufacturer));
            metadata.Add(new XElement("product_name", data.ItsProductName));
            metadata.Add(new XElement("upgrade_code", data.ItsUpgradeCode.ToString()));
            xdoc.Add(metadata);
            xdoc.Save(vf);
        }

        #endregion Private Methods
    }
}