using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using AshokGelal.InstallBaker.Events;
using AshokGelal.InstallBaker.Helpers;
using AshokGelal.InstallBaker.Models;

using EnvDTE;

namespace AshokGelal.InstallBaker.Services
{
    internal class BuildProgressService : BaseEventsService
    {
        #region Fields

        private const string InstallerProjectName = "Installer.wixproj";
        private readonly Dictionary<string, ProjectInfo> _availableProjectsDict;
        private readonly Solution _currentSolution;

        #endregion Fields

        #region Event Fields

        private readonly BuildEvents _buildEvents;

        #endregion Event Fields

        #region Constructors

        public BuildProgressService(InstallBakerEventAggregator eventAggregator, BuildEvents buildEvents, Solution currentSolution)
            : base(eventAggregator)
        {
            _buildEvents = buildEvents;
            _currentSolution = currentSolution;
            _availableProjectsDict = new Dictionary<string, ProjectInfo>();
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

                if (disposing && _buildEvents != null)
                    UnHookEvents();
            }
        }

        #endregion Dispose

        #region Private Methods

        /// <summary>
        /// Event raise when a build being
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="action">The action.</param>
        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            PopulateProjectsInfo();
            _eventAggregator.PublishEvent(_eventAggregator.BuildStarted, _availableProjectsDict.Values.ToList());
        }

        /// <summary>
        /// Event raised when a build is done.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="action">The action.</param>
        private void BuildEvents_OnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            _eventAggregator.PublishEvent(_eventAggregator.BuildFinished);
        }

        /// <summary>
        /// Event raised when the build of an individual project begins.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="projectConfig">The project config.</param>
        /// <param name="platform">The platform.</param>
        /// <param name="solutionConfig">The solution config.</param>
        private void BuildEvents_OnBuildProjConfigBegin(string project, string projectConfig, string platform, string solutionConfig)
        {
            ProjectInfo projectInfo;
            if (_availableProjectsDict.TryGetValue(project, out projectInfo))
            {
                var config = new BuildConfig(projectInfo, projectConfig, platform, solutionConfig);

                if (projectInfo.ItsStartupProjectFlag)
                    _eventAggregator.PublishEvent(_eventAggregator.StartupProjectBuildStarted, config);
                else
                {
                    //TODO find a way to use a guid instead
                    var fileName = Path.GetFileName(project);
                    if (fileName != null &&
                        fileName.Equals(InstallerProjectName, StringComparison.CurrentCultureIgnoreCase))
                        _eventAggregator.PublishEvent(_eventAggregator.InstallerProjectBuildStarted, config);
                    else
                        _eventAggregator.PublishEvent(_eventAggregator.IndividualProjectBuildStarted, config);
                }
            }
        }

        /// <summary>
        /// Event raised when the build of an individual project is done.
        /// </summary>
        /// <param name="projectName">The project name.</param>
        /// <param name="projectConfig">The project config.</param>
        /// <param name="platform">The platform.</param>
        /// <param name="solutionConfig">The solution config.</param>
        /// <param name="success">True if project build was successful, otherwise false.</param>
        private void BuildEvents_OnBuildProjConfigDone(string projectName, string projectConfig, string platform, string solutionConfig, bool success)
        {
            var project= Path.GetFileName(projectName);
            ProjectInfo projectInfo;
            if (project != null && _availableProjectsDict.TryGetValue(project, out projectInfo))
            {
                var config = new BuildConfig(projectInfo, projectConfig, platform, solutionConfig) { ItsSuccessFlag = success };

                if (projectInfo.ItsStartupProjectFlag)
                    _eventAggregator.PublishEvent(_eventAggregator.StartupProjectBuildFinished, config);
                else
                {
                    var fileName = Path.GetFileName(project);
                    if (fileName != null && fileName.Equals(InstallerProjectName, StringComparison.CurrentCultureIgnoreCase))
                        _eventAggregator.PublishEvent(_eventAggregator.InstallerProjectBuildFinished, config);
                    else
                        _eventAggregator.PublishEvent(_eventAggregator.IndividualProjectBuildFinished, config);
                }
            }
        }

        private void HookEvents()
        {
            _buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            _buildEvents.OnBuildProjConfigBegin += BuildEvents_OnBuildProjConfigBegin;
            _buildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;
            _buildEvents.OnBuildDone += BuildEvents_OnBuildDone;
        }

        private void PopulateProjectsInfo()
        {
            try
            {
                var mainProjectName =
                    Path.GetFileName(((object[]) (_currentSolution.SolutionBuild.StartupProjects))[0].ToString());
                _availableProjectsDict.Clear();
                foreach (Project project in _currentSolution.Projects)
                {
                    var paths = Utilities.GetOutputPath(project);
                    var projectName = Path.GetFileName(project.FullName);
                    if (projectName != null)
                    {
                        var info = new ProjectInfo(project.Name, paths)
                                       {
                                           ItsStartupProjectFlag = projectName.Equals(mainProjectName, StringComparison.  CurrentCultureIgnoreCase)
                                       };
                        _availableProjectsDict.Add(projectName, info);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error while getting the startup application: {0}", e.Message);
            }
        }

        private void UnHookEvents()
        {
            _buildEvents.OnBuildBegin -= BuildEvents_OnBuildBegin;
            _buildEvents.OnBuildProjConfigBegin -= BuildEvents_OnBuildProjConfigBegin;
            _buildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
            _buildEvents.OnBuildDone -= BuildEvents_OnBuildDone;
        }

        #endregion Private Methods
    }
}