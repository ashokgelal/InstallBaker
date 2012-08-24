using System.Runtime.InteropServices;

using AshokGelal.InstallBaker.Events;
using AshokGelal.InstallBaker.Services;
using AshokGelal.InstallBaker.ViewModels;

using Microsoft.VisualStudio.Shell;

namespace AshokGelal.InstallBaker.Views
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("e5116d85-19f6-4a60-8271-a6c83f73e49d")]
    internal class InstallBakerToolWindow : ToolWindowPane
    {
        #region Fields

        private InstallBakerPackage _basePackage;
        private BuildProgressService _buildProgressService;
        private DependenciesRegistry _dependenciesRegistry;
        private InstallBakerEventAggregator _eventAggreagator;
        private InstallerProjectManagementService _installerProjectManagementService;
        private ToolWindowViewModel _toolWindowViewModel;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public InstallBakerToolWindow()
            : base(null)
        {
            // Set the window title reading it from the resources.
            Caption = Properties.Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;
            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            base.Content = new ToolWindowView();
        }

        #endregion Constructors

        #region Protected Methods

        protected override void Initialize()
        {
            base.Initialize();
            _basePackage = (InstallBakerPackage)Package;
            _eventAggreagator = new InstallBakerEventAggregator();
            _dependenciesRegistry = new DependenciesRegistry(_eventAggreagator);
            _buildProgressService = new BuildProgressService(_eventAggreagator, _basePackage.IDE.Events.BuildEvents, _basePackage.IDE.Solution);
            _installerProjectManagementService = new InstallerProjectManagementService(_eventAggreagator,
                                                                       _basePackage.IDE.Events.SolutionEvents, _basePackage.IDE.Solution);
            _toolWindowViewModel = new ToolWindowViewModel(_eventAggreagator, _dependenciesRegistry);
            ((ToolWindowView) Content).DataContext = _toolWindowViewModel;
        }

        protected override void OnClose()
        {
            _toolWindowViewModel.Dispose();
            _dependenciesRegistry.Dispose();
            _buildProgressService.Dispose();
            _installerProjectManagementService.Dispose();
            _eventAggreagator = null;
            _toolWindowViewModel = null;
            _dependenciesRegistry = null;
            _buildProgressService = null;
            _installerProjectManagementService = null;
            base.OnClose();
        }

        #endregion Protected Methods
    }
}