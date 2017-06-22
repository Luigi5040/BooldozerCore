using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BooldozerCore.Properties;

namespace BooldozerCore.Editor
{
    public static class ResourceManager
    {
        public static Dictionary<string, string> MapSelectData { get; private set; }

        public static void LoadDatabases()
        {
            MapSelectData = new Dictionary<string, string>();

            XDocument reader = XDocument.Parse(Resources.MapSelectDatabase);

            XNode[] node = reader.Document.DescendantNodes() as XNode[];

            foreach (XElement el in reader.Root.Elements())
            {
                string internalName = el.Element("InternalName").Value;
                string englishName = el.Element("EnglishName").Value;
                MapSelectData.Add(internalName, englishName);
            }
        }
    }
}
