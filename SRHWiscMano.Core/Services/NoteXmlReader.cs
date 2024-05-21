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
            return xmlDoc.SelectNodes("/examination/notes/note").Cast<XmlNode>().Select(n => LoaderSelector(n, offset))
                .ToList();
        }

        private static FrameNote LoaderSelector(XmlNode node, long offset)
        {
            if (node.ChildNodes.Count == 2)
            {
                return LoadNote(node, offset);
            }
            else if(node.ChildNodes.Count == 3)
            {
                return LoadNoteV2(node, offset);
            }
            return null;
        }

        private static FrameNote LoadNote(XmlNode node, long offset)
        {
            long milliseconds = long.Parse(node.SelectSingleNode("time").InnerText) - offset;
            string innerText = node.SelectSingleNode("text").InnerText;
            return new FrameNote(InstantUtils.InstantFromMilliseconds(milliseconds), innerText);
        }


        private static FrameNote LoadNoteV2(XmlNode node, long offset)
        {
            long milliseconds = long.Parse(node.SelectSingleNode("time").InnerText) - offset;
            string innerText = node.SelectSingleNode("text").InnerText;
            string innerBolus = node.SelectSingleNode("bolus").InnerText;
            return new FrameNote(InstantUtils.InstantFromMilliseconds(milliseconds), innerBolus, innerText);
        }
    }
}
