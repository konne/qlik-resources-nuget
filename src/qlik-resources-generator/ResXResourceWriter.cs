namespace Qlik.Resources
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    #endregion

    public class ResXResourceWriter: IDisposable
    {
        XmlDocument doc;
        string filename;
        
        public ResXResourceWriter(string filename)
        {
            this.filename = filename;
            var res = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var restStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Qlik.Resources.TemplateResx.txt");
            doc = new XmlDocument();
            doc.Load(restStream);
        }

        public void AddResource(string key, string value)
        {
            var newNode = doc.CreateElement("data");
            var newValue = doc.CreateElement("value");
            newValue.InnerText = value;
            newNode.SetAttribute("name", key);
            newNode.SetAttribute("xml:space", "preserve");
            newNode.AppendChild(newValue);

            XmlNode root = doc.DocumentElement;
            root.InsertAfter(newNode, root.LastChild);            
        }

        public void Dispose()
        {
            doc.Save(filename);
        }
    }
}
