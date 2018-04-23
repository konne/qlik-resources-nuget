using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Qlik.Resources
{
    class Program
    {
        static void Main(string[] args)
        {
            var QlikSenseClientPath = @"C:\Program Files\Qlik\Sense\Client\";
            var translatePath = QlikSenseClientPath + @"translate\";

                        var fontPath = QlikSenseClientPath + @"fonts\";
            var destPath = @"..\..\..\..\qlik-resources-nuget\";

            var generatedCultures = QlikTranslateJS.GenerateQlikTranslateJS(translatePath, destPath);

            TranslateCS.WriteTranslateCS(generatedCultures, destPath);

            foreach (var ttfFile in Directory.EnumerateFiles(fontPath, "*.ttf"))
            {
                File.Copy(ttfFile, destPath + @"\" + Path.GetFileName(ttfFile), true);
                //cproj.AddItemFast("Resource", Path.GetFileName(ttfFile));
            }

        }
    }

}