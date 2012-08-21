using System;
using System.ComponentModel.Design;

using AshokGelal.InstallBaker.Integration;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.IntegrationTestLibrary;
using Microsoft.VSSDK.Tools.VsIdeTesting;

namespace InstallBaker_IntegrationTests
{
    [TestClass]
    public class ToolWindowTest
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

        /// <summary>
        ///A test for showing the toolwindow
        ///</summary>
        [TestMethod]
        [HostType("VS IDE")]
        public void ShowToolWindow()
        {
            UIThreadInvoker.Invoke((ThreadInvoker)delegate()
            {
                CommandID toolWindowCmd = new CommandID(GuidList.guidInstallBakerCmdSet, (int)PkgCmdIDList.cmdIdInstallBakerToolWindow);

                TestUtils testUtils = new TestUtils();
                testUtils.ExecuteCommand(toolWindowCmd);

                Assert.IsTrue(testUtils.CanFindToolwindow(new Guid(GuidList.guidToolWindowPersistanceString)));

            });
        }

        #endregion Public Methods
    }
}