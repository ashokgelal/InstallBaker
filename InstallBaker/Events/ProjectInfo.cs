namespace AshokGelal.InstallBaker.Events
{
    internal class ProjectInfo
    {
        public ProjectInfo(string name, string rootloc, string outputdir)
        {
            ItsName = name;
            ItsRootDir = rootloc;
            ItsOutputDir = outputdir;
        }

        public string ItsOutputDir { get; private set; }

        public string ItsRootDir { get;  private set; }

        public string ItsName { get; private set; }

        public bool ItsStartupProjectFlag { get; private set; }
    }
}