#region Header

// <CustomTable Id='_VsdLaunchCondition'>
//      <Column Id='Condition' PrimaryKey='yes' Type='string' Width='255' Category='Condition'
//              Description='Expression which must evaluate to TRUE in order for install to commence.' Modularize='Condition' />
//      <Column Id='Description' Type='string' Width='255' Localizable='yes' Category='Formatted'
//              Description='Localizable text to display when condition fails and install must abort.' Modularize='Property' />
//      <Column Id='Url' Type='string' Width='0' Category='Text'
//              Description='URL to navigate to when condition fails and install must abort.' />
//      <Row>
//        <Data Column='Condition'>VSDFXAvailable</Data>
//        <Data Column='Description'>[VSDNETURLMSG]</Data>
//        <Data Column='Url'>http://go.microsoft.com/fwlink/?LinkId=76617</Data>
//      </Row>
//    </CustomTable>

#endregion Header

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AshokGelal.InstallBaker.Models
{
    public class BakeComponent
    {
        #region Properties

        public List<BakeFile> ItsBakeFiles
        {
            get;
            set;
        }

        public Guid ItsGuid
        {
            get;
            private set;
        }

        public string ItsId
        {
            get;
            private set;
        }

        public BakeDirectory ItsParentDirectory
        {
            get;
            private set;
        }

        #endregion Properties

        #region Constructors

        public BakeComponent(string id, BakeDirectory bakeDirectory, Guid guid = default(Guid))
        {
            ItsId = id;
            ItsParentDirectory = bakeDirectory;
            if (guid == default(Guid))
                ItsGuid = Guid.NewGuid();

            ItsBakeFiles = new List<BakeFile>();
        }

        #endregion Constructors
    }

    public class BakeDirectory
    {
        #region Properties

        public List<BakeComponent> ItsComponents
        {
            get;
            set;
        }

        public string ItsId
        {
            get;
            private set;
        }

        public string ItsName
        {
            get;
            private set;
        }

        #endregion Properties

        #region Constructors

        public BakeDirectory(string id, string name)
        {
            ItsId = id;
            ItsName = name;
            ItsComponents = new List<BakeComponent>();
        }

        #endregion Constructors
    }

    public class BakeFile
    {
        #region Properties

        public string ItsId
        {
            get;
            private set;
        }

        public bool? ItsKeyPathFlag
        {
            get;
            private set;
        }

        public BakeComponent ItsParentComponent
        {
            get;
            private set;
        }

        public string ItsSource
        {
            get;
            private set;
        }

        #endregion Properties

        #region Constructors

        public BakeFile(string id, string source, BakeComponent parentComponent)
        {
            ItsId = id;
            ItsSource = source;
            ItsParentComponent = parentComponent;
        }

        #endregion Constructors
    }

    public class BakeMetadata : DietMvvm.ViewModelBase
    {
        #region Fields

        private string _companyName;
        private string _iconName;
        private string _mainExecutableDisplayName;
        private string _mainExecutableSource;
        private string _manufacturer;
        private string _productName;

        #endregion Fields

        #region Properties

        public string ItsCompanyName
        {
            get { return _companyName; }
            set
            {
                _companyName = value;
                NotifyPropertyChanged(() => ItsCompanyName);
            }
        }

        public string ItsIconName
        {
            get { return _iconName; }
            set
            {
                _iconName = value;
                NotifyPropertyChanged(() => ItsIconName);
            }
        }

        //[Browsable(false)]
        public BakeComponent ItsMainExecutableComponent
        {
            get;
            set;
        }

        public string ItsMainExecutableDisplayName
        {
            get { return _mainExecutableDisplayName; }
            set
            {
                _mainExecutableDisplayName = value;
                NotifyPropertyChanged(() => ItsMainExecutableDisplayName);
            }
        }

        public string ItsMainExecutableSource
        {
            get { return _mainExecutableSource; }
            set
            {
                _mainExecutableSource = value;
                NotifyPropertyChanged(() => ItsMainExecutableSource);
            }
        }

        public string ItsManufacturer
        {
            get { return _manufacturer; }
            set
            {
                _manufacturer = value;
                NotifyPropertyChanged(() => ItsManufacturer);
            }
        }

        public string ItsProductName
        {
            get { return _productName; }
            set
            {
                _productName = value;
                NotifyPropertyChanged(() => ItsProductName);
            }
        }

        //[Browsable(false)]
        public BakeComponent ItsProgramMenuComponent
        {
            get;
            set;
        }

        //[Browsable(false)]
        public Guid ItsUpgradeCode
        {
            get;
            set;
        }

        #endregion Properties
    }
}