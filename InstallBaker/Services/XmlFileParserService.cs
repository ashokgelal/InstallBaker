using System;
using System.IO;
using System.Xml.Linq;

using AshokGelal.InstallBaker.Models;

namespace AshokGelal.InstallBaker.Services
{
    internal class XmlFileParserService
    {
        #region Fields

        private const string CustomTableXml = @"
        <CustomTableXml  xmlns='http://schemas.microsoft.com/wix/2006/wi'>
          <CustomTable Id='_VsdLaunchCondition'>
            <Column Id='Condition' PrimaryKey='yes' Type='string' Width='255' Category='Condition'
                Description='Expression which must evaluate to TRUE in order for install to commence.' Modularize='Condition' />
            <Column Id='Description' Type='string' Width='255' Localizable='yes' Category='Formatted'
                Description='Localizable text to display when condition fails and install must abort.' Modularize='Property' />
            <Column Id='Url' Type='string' Width='0' Category='Text'
                Description='URL to navigate to when condition fails and install must abort.' />
            <Row>
              <Data Column='Condition'>VSDFXAvailable</Data>
              <Data Column='Description'>[VSDNETURLMSG]</Data>
              <Data Column='Url'>http://go.microsoft.com/fwlink/?LinkId=76617</Data>
            </Row>
          </CustomTable>
        </CustomTableXml>";
        private const string InstallUiXml = @"
        <InstallUiXml  xmlns='http://schemas.microsoft.com/wix/2006/wi'>
          <UIRef Id='WixUI_InstallDir' />
          <UIRef Id='WixUI_ErrorProgressText' />
          <Property Id='WIXUI_INSTALLDIR' Value='INSTALLDIR' />
          <UI Id='MyWixUI'>
            <UIRef Id='WixUI_InstallDir' />
          </UI>
        </InstallUiXml>";
        public static readonly string RegistryKeyFormat = @"Software\[Manufacturer]\[ProductName]\{{0}}";
        public static readonly string VersionFormat = "!{bind.FileVersion.{0}";
        private static XNamespace xn = "http://schemas.microsoft.com/wix/2006/wi";

        #endregion Fields

        #region Public Methods

        public static BakeMetadata ReadBakeFile(string bakeFile)
        {
            var xdoc = XDocument.Load(bakeFile);
            var metadata = new BakeMetadata();

            try
            {
                var root = xdoc.Element("metadata");

                // ReSharper disable PossibleNullReferenceException
                var company_name = root.Element("company_name").Value;
                var icon_name = root.Element("icon_name").Value;
                var executable_display_name = root.Element("executable_display_name").Value;
                var executable_source = root.Element("executable_source").Value;
                var manufacturer = root.Element("manufacturer").Value;
                var product_name = root.Element("product_name").Value;
                var upgrade_code = root.Element("upgrade_code").Value;

                metadata.ItsCompanyName = company_name;
                metadata.ItsIconName = icon_name;
                metadata.ItsMainExecutableDisplayName = executable_display_name;
                metadata.ItsMainExecutableSource = executable_source;
                metadata.ItsManufacturer = manufacturer;
                metadata.ItsProductName = product_name;
                metadata.ItsUpgradeCode = new Guid(upgrade_code);

                var executableCompElem = root.Element("executable_component");
                metadata.ItsMainExecutableComponent = new BakeComponent(executableCompElem.Attribute("Id").Value, new Guid(executableCompElem.Attribute("Guid").Value));

                executableCompElem = root.Element("program_menu_dir");
                metadata.ItsProgramMenuComponent = new BakeComponent(executableCompElem.Attribute("Id").Value, new Guid(executableCompElem.Attribute("Guid").Value));

                // ReSharper restore PossibleNullReferenceException
                return metadata;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public static void WriteBakeFile(Stream vf, BakeMetadata data)
        {
            var xdoc = new XDocument();
            var metadata = new XElement(xn + "metadata");
            metadata.Add(new XElement(xn + "company_name", data.ItsCompanyName));
            metadata.Add(new XElement(xn + "icon_name", data.ItsIconName));
            metadata.Add(new XElement(xn + "executable_display_name", data.ItsMainExecutableDisplayName));
            metadata.Add(new XElement(xn + "executable_source", data.ItsMainExecutableSource));
            metadata.Add(new XElement(xn + "manufacturer", data.ItsManufacturer));
            metadata.Add(new XElement(xn + "addLicense", data.ItsAddLicenseFlag));
            metadata.Add(new XElement(xn + "addBanner", data.ItsAddBannerFlag));
            metadata.Add(new XElement(xn + "product_name", data.ItsProductName));
            metadata.Add(new XElement(xn + "upgrade_code", data.ItsUpgradeCode.ToString()));
            metadata.Add(new XElement(xn + "executable_component", new XAttribute("Id", data.ItsMainExecutableComponent.ItsId), new XAttribute("Guid", data.ItsMainExecutableComponent.ItsGuid.ToString())));
            metadata.Add(new XElement(xn + "program_menu_dir", new XAttribute("Id", data.ItsProgramMenuComponent.ItsId), new XAttribute("Guid", data.ItsProgramMenuComponent.ItsGuid.ToString())));
            xdoc.Add(metadata);
            xdoc.Save(vf);
        }

        public static void WriteWixFile(BakeMetadata metadata, string wixfile)
        {
            using (var fs = File.Create(wixfile))
            {
                // ReSharper disable PossibleNullReferenceException
                var xdoc = new XDocument();

                var wix = new XElement(xn + "Wix");
                var product = new XElement(xn + "Product", new XAttribute("Id", "*"), new XAttribute("Name", metadata.ItsProductName),
                                           new XAttribute("Language", "1033"), new XAttribute("Version", "!(bind.FileVersion.inSSIDer_EXE)"),
                                           new XAttribute("Manufacturer", metadata.ItsManufacturer),
                                           new XAttribute("UpgradeCode", metadata.ItsUpgradeCode.ToString())
                    );

                var package = new XElement(xn+"Package", new XAttribute("Id", "*"), new XAttribute("Languages", "1033"),
                                           new XAttribute("Manufacturer", metadata.ItsCompanyName),
                                           new XAttribute("Platform", "x86"), new XAttribute("InstallerVersion", "200"),
                                           new XAttribute("Compressed", "yes"));

                var customTable = XElement.Parse(CustomTableXml).Elements();

                var alluserProp = new XElement(xn + "Property", new XAttribute("Id", "ALLUSERS"), new XAttribute("Value", "1"));
                var applicationFolderName = new XElement(xn + "Property", new XAttribute("Id", "ApplicationFolderName"), new XAttribute("Value", string.Format("{0}\\{1}", metadata.ItsManufacturer, metadata.ItsProductName)));
                var installUi = XElement.Parse(InstallUiXml).Elements();

                var license = new XElement(xn + "WixVariable", new XAttribute("Id", "WixUILicenseRtf"), new XAttribute("Value", "License.rtf"));
                var banner = new XElement(xn + "WixVariable", new XAttribute("Id", "WixUIBannerBmp"), new XAttribute("Value", "banner.bmp"));

                var media = new XElement(xn + "Media", new XAttribute("Id", "1"), new XAttribute("Cabinet", "media1.cab"), new XAttribute("EmbedCab", "yes"));
                var icon = new XElement(xn + "Icon", new XAttribute("Id", "Icon.ico"), new XAttribute("SourceFile", metadata.ItsIconName));
                var iconProp = new XElement(xn + "Property", new XAttribute("Id", "ARPPRODUCTICON"), new XAttribute("Value", "Icon.ico"));

                var targetDir = new XElement(xn + "Directory", new XAttribute("Id", "TARGETDIR"),
                                             new XAttribute("Name", "SourceDir"));

                var desktopFolder = new XElement(xn + "Directory", new XAttribute("Id", "DesktopFolder"),
                                                 new XAttribute("Name", "Desktop"));

                var programFilesFolder = new XElement(xn + "Directory", new XAttribute("Id", "ProgramFilesFolder"),
                                                      new XAttribute("Name", "PFiles"));

                var programOutputFolder = new XElement(xn + "Directory", new XAttribute("Id", "ProgramOutputFolder"),
                                                       new XAttribute("Name", metadata.ItsManufacturer));

                var installDir = new XElement(xn + "Directory", new XAttribute("Id", "INSTALLDIR"),
                                              new XAttribute("Name", metadata.ItsProductName));

                #region SHORTCUT INTO START

                var programMenuFolder = new XElement(xn + "Directory", new XAttribute("Id", "ProgramMenuFolder"),
                                                     new XAttribute("Name", "Programs"));

                var programMenuDir = new XElement(xn + "Directory", new XAttribute("Id", "ProgramMenuDir"),
                                                  new XAttribute("Name", metadata.ItsManufacturer));

                var programMenuComponent = new XElement(xn + "Component", new XAttribute("Id", metadata.ItsProgramMenuComponent.ItsId),
                                                         new XAttribute("Guid", metadata.ItsProgramMenuComponent.ItsGuid.ToString()));

                var createFolder = new XElement(xn + "CreateFolder", new XAttribute("Directory", "ProgramMenuDir"));

                var registryValue = new XElement(xn + "RegistryValue", new XAttribute("Root", "HKCU"),
                                                 new XAttribute("Key", string.Format(RegistryKeyFormat, metadata.ItsProgramMenuComponent.ItsGuid)),
                                                 new XAttribute("Name", "ProgramMenuFolder"),
                                                 new XAttribute("Value", "[ProgramMenuFolder]"),
                                                 new XAttribute("Type", "string"),
                                                 new XAttribute("KeyPath", "yes")
                    );

                var removeFoler = new XElement(xn + "RemoveFolder", new XAttribute("Id", "ProgramMenuDir"),
                                               new XAttribute("On", "uninstall"));

                programMenuComponent.Add(createFolder);
                programMenuComponent.Add(registryValue);
                programMenuComponent.Add(removeFoler);

                programMenuDir.Add(programMenuComponent);
                programMenuFolder.Add(programMenuDir);

                #endregion

                #region MAIN FILES

                installDir.Add(programMenuFolder);
                AddMainComponentFiles(metadata, installDir);
                AddSubDirectoryFiles(metadata, installDir);

                programOutputFolder.Add(installDir);
                programFilesFolder.Add(programOutputFolder);
                targetDir.Add(desktopFolder);
                targetDir.Add(programFilesFolder);

                // TODO: Add feature

                product.Add(package);
                product.Add(customTable);
                product.Add(alluserProp);
                product.Add(applicationFolderName);
                product.Add(installUi);
                if(metadata.ItsAddLicenseFlag)
                  product.Add(license);

                if(metadata.ItsAddBannerFlag)
                  product.Add(banner);
                product.Add(media);
                product.Add(icon);
                product.Add(iconProp);
                product.Add(targetDir);

                AddCompleteInstallFeature(metadata, product);

                wix.Add(product);
                xdoc.Add(wix);
                xdoc.Save(fs);
                #endregion

                // ReSharper restore PossibleNullReferenceException
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static void AddCompleteInstallFeature(BakeMetadata metadata, XElement parent)
        {
            var feature = new XElement(xn + "Feature", new XAttribute("Id", "CompleteInstall"),
                                       new XAttribute("Title", metadata.ItsProductName),
                                       new XAttribute("Description", "Install all the components"),
                                       new XAttribute("Display", "expand"), new XAttribute("Level", "1"),
                                       new XAttribute("ConfigurableDirectory", "INSTALLDIR"));
            AddComponentRef("ProgramMenuDir", feature);
            AddComponentRef(metadata.ItsMainExecutableComponent.ItsId, feature, true);
            foreach (var directory in metadata.ItsSubDirectories)
              AddComponentRef(directory.ItsId, feature);
            parent.Add(feature);
        }

        private static void AddComponentRef(string id, XElement parent, bool primary = false)
        {
            var component = new XElement(xn + "ComponentRef", new XAttribute("Id", id));
            if(primary)
                component.Add(new XAttribute("Primary", "yes"));
            parent.Add(component);
        }

        private static void AddFileElements(BakeComponent component, XElement parent)
        {
            foreach (var bakeFile in component.ItsBakeFiles)
            {
                var source = bakeFile.ItsSource;
                var filename = Path.GetFileName(source);
                if(filename== null)
                    continue;
                var fileElement = new XElement(xn + "File", new XAttribute("Id", filename),
                                                new XAttribute("Source", source));
                parent.Add(fileElement);
            }
        }

        private static void AddMainComponentFiles(BakeMetadata metadata, XElement parent)
        {
            var mainExecutableComponent = new XElement(xn + "Component", new XAttribute("Id", metadata.ItsMainExecutableComponent.ItsId),
                                         new XAttribute("Guid", metadata.ItsMainExecutableComponent.ItsGuid.ToString()));
            #region SHORTCUTS

                var shortcutFile = new XElement(xn + "File", new XAttribute("Id", "inSSIDer_EXE"),
                                                new XAttribute("Name", metadata.ItsMainExecutableDisplayName),
                                                new XAttribute("DiskId", "1"), new XAttribute("KeyPath", "yes"),
                                                new XAttribute("Source", metadata.ItsMainExecutableSource));

                var startmenuShortcut = new XElement(xn + "Shortcut", new XAttribute("Id", "startmenu"), new XAttribute("Directory", "ProgramMenuDir"),
                                                     new XAttribute("Name", metadata.ItsProductName), new XAttribute("WorkingDirectory", "INSTALLDIR"),
                                                     new XAttribute("Icon", "Icon.ico"), new XAttribute("Show", "normal"), new XAttribute("Advertise", "yes"));
                var desktopShortcut = new XElement(xn + "Shortcut", new XAttribute("Id", "desktop"), new XAttribute("Directory", "DesktopFolder"),
                                                     new XAttribute("Name", metadata.ItsProductName), new XAttribute("WorkingDirectory", "INSTALLDIR"),
                                                     new XAttribute("Icon", "Icon.ico"), new XAttribute("Advertise", "yes"));
                shortcutFile.Add(startmenuShortcut);
                shortcutFile.Add(desktopShortcut);

                #endregion

            mainExecutableComponent.Add(shortcutFile);
            AddFileElements(metadata.ItsMainExecutableComponent, mainExecutableComponent);
            parent.Add(mainExecutableComponent);
        }

        private static void AddSubDirectoryFiles(BakeMetadata metadata, XElement parent)
        {
            foreach (var bakeDirectory in metadata.ItsSubDirectories)
            {
                var directory = new XElement(xn + "Directory", new XAttribute("Id", string.Format("{0}Dir", bakeDirectory.ItsName)), new XAttribute("Name", bakeDirectory.ItsName));
                var component = new XElement(xn + "Component", new XAttribute("Id", bakeDirectory.ItsComponent.ItsId),
                                         new XAttribute("Guid", bakeDirectory.ItsComponent.ItsGuid));
                AddFileElements(bakeDirectory.ItsComponent, component);
                directory.Add(component);
                parent.Add(directory);
            }
        }

        #endregion Private Methods
    }
}