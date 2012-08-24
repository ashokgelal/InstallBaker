using System;
using System.IO;

using EnvDTE;

namespace AshokGelal.InstallBaker.Helpers
{
    public class ProjectPaths
    {
        #region Properties

        public string ItsOutputPath
        {
            get; private set;
        }

        public string ItsRootPath
        {
            get; private set;
        }

        #endregion Properties

        #region Constructors

        public ProjectPaths(string rootpath, string outputpath)
        {
            ItsRootPath = rootpath;
            ItsOutputPath = outputpath;
        }

        #endregion Constructors
    }

    public static class Utilities
    {
        #region Public Methods

        public static ProjectPaths GetOutputPath(Project project)
        {
            var rootPath = Path.GetDirectoryName(project.FullName);
            if (rootPath == null)
                return null;
            var outputPath = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            var fullOutputPath =  Path.Combine(rootPath, outputPath);
            return new ProjectPaths(rootPath, fullOutputPath);
        }

        #endregion Public Methods
    }
}