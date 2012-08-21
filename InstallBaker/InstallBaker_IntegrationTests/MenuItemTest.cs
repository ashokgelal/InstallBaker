using System;
using System.ComponentModel.Design;
using System.Globalization;

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
    public class MenuItemTest
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
        ///A test for lauching the command and closing the associated dialogbox
        ///</summary>
        [TestMethod]
        [HostType("VS IDE")]
        public void LaunchCommand()
        {
            UIThreadInvoker.Invoke((ThreadInvoker)delegate()
            {
                CommandID menuItemCmd = new CommandID(GuidList.guidInstallBakerCmdSet, (int)PkgCmdIDList.cmdIdInstallBaker);

                // Create the DialogBoxListener Thread.
                string expectedDialogBoxText = string.Format(CultureInfo.CurrentCulture, "{0}\n\nInside {1}.MenuItemCallback()", "InstallBaker", "AshokGelal.InstallBaker.InstallBakerPackage");
                DialogBoxPurger purger = new DialogBoxPurger(NativeMethods.IDOK, expectedDialogBoxText);

                try
                {
                    purger.Start();

                    TestUtils testUtils = new TestUtils();
                    testUtils.ExecuteCommand(menuItemCmd);
                }
                finally
                {
                    Assert.IsTrue(purger.WaitForDialogThreadToTerminate(), "The dialog box has not shown");
                }
            });
        }

        #endregion Public Methods
    }
}