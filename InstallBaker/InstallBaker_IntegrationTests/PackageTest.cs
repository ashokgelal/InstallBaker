using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AshokGelal.InstallBaker.Integration;

using EnvDTE;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;

namespace InstallBaker_IntegrationTests
{
    /// <summary>
    /// Integration test for package validation
    /// </summary>
    [TestClass]
    public class PackageTest
    {
        #region Fields

        private TestContext testContextInstance;

        #endregion Fields

        #region Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion Properties

        #region Invoke Methods

        private delegate void ThreadInvoker();

        #endregion Invoke Methods

        #region Public Methods

        [TestMethod]
        [HostType("VS IDE")]
        public void PackageLoadTest()
        {
            UIThreadInvoker.Invoke((ThreadInvoker)delegate()
            {

                //Get the Shell Service
                IVsShell shellService = VsIdeTestHostContext.ServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
                Assert.IsNotNull(shellService);

                //Validate package load
                IVsPackage package;
                Guid packageGuid = new Guid(GuidList.guidInstallBakerPkgString);
                Assert.IsTrue(0 == shellService.LoadPackage(ref packageGuid, out package));
                Assert.IsNotNull(package, "Package failed to load");

            });
        }

        #endregion Public Methods
    }
}