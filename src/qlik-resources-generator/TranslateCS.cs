using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Qlik.Resources
{
    class TranslateCS
    {
        public static void WriteTranslateCS(List<CultureInfo> cultList, string path)
        {
            #region Generate Translate.CS
            var langCS =
 @"namespace Qlik.Resources" + "\r\n" +
 @"{" + "\r\n" +
 @"    using System.Collections.Generic;" + "\r\n" +
 @"    using System.Globalization; " + "\r\n" +
 @"" + "\r\n" +
 @"    public static class Translate" + "\r\n" +
 @"    {" + "\r\n" +
 @"        public static IEnumerable<CultureInfo> AvailableCultures()" + "\r\n" +
 @"        {" + "\r\n";

            foreach (var item in cultList)
                langCS += @"            yield return new CultureInfo(" + "\"" + item.ToString() + "\");" + "\r\n";

            langCS +=

@"        }" + "\r\n" +
@"    }" + "\r\n" +
@"}" + "\r\n";
            File.WriteAllText(path + @"\Translate.cs", langCS);            
            #endregion
        }
    }
}
