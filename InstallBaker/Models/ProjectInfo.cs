using AshokGelal.InstallBaker.Helpers;

namespace AshokGelal.InstallBaker.Models
{
    public class ProjectInfo
    {
        #region Properties

        public string ItsName
        {
            get; private set;
        }

        public ProjectPaths ItsProjectPaths
        {
            get; private set;
        }

        public bool ItsStartupProjectFlag
        {
            get; set;
        }

        #endregion Properties

        #region Constructors

        public ProjectInfo(string name, ProjectPaths projectPaths)
        {
            ItsName = name;
            ItsProjectPaths = projectPaths;
        }

        #endregion Constructors
    }
}