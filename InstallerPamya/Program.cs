using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WixSharp;

namespace InstallerPamya
{
    class Script 
    {
        static public void Main()
        {
            Feature binaries = new Feature("Pamya Binaries");
            Feature docs = new Feature("Documentation");
            var project = new Project("Pamya",
                                new Dir(@"%ProgramFiles%\Pamya",
                                    new File(@"..\LICENSE"),
                                    new File(binaries,@"..\Pamya\bin\Release\Pamya.exe",
                                        new FileShortcut(binaries, "Pamya", @"%ProgramMenu%\Pamya")
                                    ),
                                    new File(binaries, @"..\Pamya\PamyaIcon.ico"),
                                    new File(binaries, @"..\Pamya\bin\Release\System.Data.SQLite.dll"),
                                    new File(binaries, @"..\Pamya\bin\Release\SQLite.Interop.dll")
                                ),
                                new Dir(@"%ProgramFiles%\Pamya\Resources",
                                        new File(binaries,@"..\Pamya\Resources\Localise.eo-Eo.xaml"),
                                        new File(binaries,@"..\Pamya\Resources\Localise.ru-RU.xaml"),
                                        new File(binaries,@"..\Pamya\Resources\Localise.xaml")
                                    ),
                                new Dir("%Startup%",
                                    new ExeFileShortcut(binaries, "Pamya", "[INSTALLDIR]Pamya.exe", "")),

                                new Dir(@"%ProgramMenu%\Pamya",
                                    new ExeFileShortcut(binaries, "Uninstall Pamya", "[System64Folder]msiexec.exe", "/x [ProductCode]"))       
                          );
            
 
            project.GUID = new Guid("ad4e532e-5832-4ec8-bbff-09f3dfdcc427");
            project.UI = WUI.WixUI_Mondo;
            project.SourceBaseDir = Environment.CurrentDirectory;
            project.OutFileName = "Pamya";

            //Compiler.AllowNonRtfLicense = true;

            project.LicenceFile = @"..\LICENSE.rtf";


            try
            {
                Compiler.BuildMsi(project);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }
    } 
}


  
