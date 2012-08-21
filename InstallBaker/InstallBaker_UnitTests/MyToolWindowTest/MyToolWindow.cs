#region Header

/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

#endregion Header

using System;
using System.Collections;
using System.Reflection;
using System.Text;

using AshokGelal.InstallBaker;
using AshokGelal.InstallBaker.UI;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Microsoft.VsSDK.UnitTestLibrary;

namespace InstallBaker_UnitTests.MyToolWindowTest
{
    /// <summary>
    ///This is a test class for MyToolWindowTest and is intended
    ///to contain all MyToolWindowTest Unit Tests
    ///</summary>
    [TestClass]
    public class MyToolWindowTest
    {
        #region Public Methods

        /// <summary>
        ///MyToolWindow Constructor test
        ///</summary>
        [TestMethod]
        public void MyToolWindowConstructorTest()
        {
            MyToolWindow target = new MyToolWindow();
            Assert.IsNotNull(target, "Failed to create an instance of MyToolWindow");

            MethodInfo method = target.GetType().GetMethod("get_Content", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(method.Invoke(target, null), "MyControl object was not instantiated");
        }

        /// <summary>
        ///Verify the Content property is valid.
        ///</summary>
        [TestMethod]
        public void WindowPropertyTest()
        {
            MyToolWindow target = new MyToolWindow();
            Assert.IsNotNull(target.Content, "Content property was null");
        }

        #endregion Public Methods
    }
}