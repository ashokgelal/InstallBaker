using System;

namespace AshokGelal.InstallBaker.Events
{
    internal class BuildConfig : EventArgs
    {
        public ProjectInfo ProjectInfo { get; private set; }
        public string ItsProjectConfig { get; private set; }
        public string ItsPlatform { get; private set; }
        public string ItsSolutionConfig { get; private set; }
        public bool? ItsSuccessFlag { get; set; }

        public BuildConfig(ProjectInfo projectInfo, string projectConfig, string platform, string solutionConfig)
        {
            ProjectInfo = projectInfo;
            ItsProjectConfig = projectConfig;
            ItsPlatform = platform;
            ItsSolutionConfig = solutionConfig;
            ItsSuccessFlag = null;
        }
    }
}