﻿using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AshokGelal.InstallBaker.Events
{
    internal class BuildProgressService : BaseEventsService
    {
        private const string InstallerProjectName = "Installer.wixproj";
        private readonly BuildEvents _buildEvents;
        private readonly Solution _currentSolution;
        private readonly Dictionary<string, ProjectInfo> _availableProjectsDict;

        public BuildProgressService(InstallBakerEventAggregator eventAggregator, BuildEvents buildEvents, Solution currentSolution)
            : base(eventAggregator)
        {
            _buildEvents = buildEvents;
            _currentSolution = currentSolution;
            _availableProjectsDict = new Dictionary<string, ProjectInfo>();
            HookEvents();
        }

        private void HookEvents()
        {
            _buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            _buildEvents.OnBuildProjConfigBegin += BuildEvents_OnBuildProjConfigBegin;
            _buildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;
            _buildEvents.OnBuildDone += BuildEvents_OnBuildDone;
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
        /// Event raised when the build of an individual project is done.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="projectConfig">The project config.</param>
        /// <param name="platform">The platform.</param>
        /// <param name="solutionConfig">The solution config.</param>
        /// <param name="success">True if project build was successful, otherwise false.</param>
        private void BuildEvents_OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
        {
            ProjectInfo projectInfo;
            if (_availableProjectsDict.TryGetValue(project, out projectInfo))
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
        /// Event raise when a build being
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="action">The action.</param>
        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            PopulateProjectsInfo();
            _eventAggregator.PublishEvent(_eventAggregator.BuildStarted, _availableProjectsDict.Values.ToList());
        }

        private void PopulateProjectsInfo()
        {
            try
            {
                var mainProjectName = ((object[])(_currentSolution.SolutionBuild.StartupProjects))[0].ToString();
                _availableProjectsDict.Clear();
                foreach (Project project in _currentSolution.Projects)
                {
                    var rootPath = Path.GetDirectoryName(project.FullName);
                    if (rootPath == null)
                        continue;

                    var outputPath = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                    var outputFullPath = Path.Combine(rootPath, outputPath);

                    var info = new ProjectInfo(project.Name, rootPath, outputFullPath);
                    if (project.Name.Equals(mainProjectName, StringComparison.CurrentCultureIgnoreCase))
                        _availableProjectsDict.Add(project.Name, info);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error while getting the startup application: {0}", e.Message);
            }
        }

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

        private void UnHookEvents()
        {
            _buildEvents.OnBuildBegin -= BuildEvents_OnBuildBegin;
            _buildEvents.OnBuildProjConfigBegin -= BuildEvents_OnBuildProjConfigBegin;
            _buildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
            _buildEvents.OnBuildDone -= BuildEvents_OnBuildDone;
        }
    }
}