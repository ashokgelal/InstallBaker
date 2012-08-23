using System.IO;
using EnvDTE;

namespace AshokGelal.InstallBaker.Helpers
{
    public static class Utilities
    {
        public static ProjectPaths GetOutputPath(Project project)
        {
            var rootPath = Path.GetDirectoryName(project.FullName);
            if (rootPath == null)
                return null;
            var outputPath = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            var fullOutputPath =  Path.Combine(rootPath, outputPath);
            return new ProjectPaths(rootPath, fullOutputPath);
        }
    }

    public class ProjectPaths
    {
        public string ItsRootPath { get; private set; }
        public string ItsOutputPath { get; private set; }

        public ProjectPaths(string rootpath, string outputpath)
        {
            ItsRootPath = rootpath;
            ItsOutputPath = outputpath;
        }
    }
}
