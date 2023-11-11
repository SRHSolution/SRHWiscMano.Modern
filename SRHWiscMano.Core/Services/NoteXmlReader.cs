using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.ViewModels;
using System.IO;
using System.Xml;
using Microsoft.Extensions.Logging;
using NLog;

namespace SRHWiscMano.Core.Services
{
    public class NoteXmlReader
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<FrameNote> LoadRelative(string dataFilePath, string extension = "examination.xml")
        {
            var notefilePath = Path.ChangeExtension(dataFilePath, extension);
            try
            {
                return LoadFile(notefilePath);
            }
            finally
            {
                logger.Debug($"Loaded note file {Path.GetFileName(notefilePath)}");
            }            
        }

        public IEnumerable<FrameNote> LoadFile(string path)
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

        internal static IEnumerable<FrameNote> LoadXml(XmlDocument xmlDoc)
        {
            long offset = long.Parse(xmlDoc.SelectSingleNode("/examination/startTimeOffset").InnerText);
            return xmlDoc.SelectNodes("/examination/notes/note").Cast<XmlNode>().Select(n => LoadNote(n, offset))
                .ToList();
        }

        private static FrameNote LoadNote(XmlNode node, long offset)
        {
            long milliseconds = long.Parse(node.SelectSingleNode("time").InnerText) - offset;
            string innerText = node.SelectSingleNode("text").InnerText;
            return new FrameNote(InstantUtils.InstantFromMilliseconds(milliseconds), innerText);
        }
    }
}
