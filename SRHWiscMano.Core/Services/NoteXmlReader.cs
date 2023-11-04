using SRHWiscMano.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SRHWiscMano.Core.Services
{
    public class NoteXmlReader
    {
        public IEnumerable<Note> LoadRelative(string dataFilePath, string extension = "examination.xml")
        {
            return LoadFile(Path.ChangeExtension(dataFilePath, extension));
        }

        public IEnumerable<Note> LoadFile(string path)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                return LoadXml(xmlDoc);
            }
            catch (FileNotFoundException ex)
            {
                return null;
            }
        }

        internal static IEnumerable<Note> LoadXml(XmlDocument xmlDoc)
        {
            long offset = long.Parse(xmlDoc.SelectSingleNode("/examination/startTimeOffset").InnerText);
            return xmlDoc.SelectNodes("/examination/notes/note").Cast<XmlNode>().Select(n => LoadNote(n, offset))
                .ToList();
        }

        private static Note LoadNote(XmlNode node, long offset)
        {
            long milliseconds = long.Parse(node.SelectSingleNode("time").InnerText) - offset;
            string innerText = node.SelectSingleNode("text").InnerText;
            return new Note(SampleTime.InstantFromMilliseconds(milliseconds), innerText);
        }
    }
}
