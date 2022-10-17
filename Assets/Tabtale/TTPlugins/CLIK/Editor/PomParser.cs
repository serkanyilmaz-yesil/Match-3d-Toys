using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TTPlugins.PomFile
{
    [XmlRoot(ElementName = "dependency")]
    public class PomDependency
    {
        [XmlElement(ElementName = "groupId")] public string GroupId { get; set; }

        [XmlElement(ElementName = "artifactId")]
        public string ArtifactId { get; set; }

        [XmlElement(ElementName = "version")] public string Version { get; set; }
    }

    [XmlRoot(ElementName = "dependencies")]
    public class PomDependencies
    {
        [XmlElement(ElementName = "dependency")]
        public List<PomDependency> Dependencies { get; set; }
    }

    [XmlRoot(ElementName = "project", Namespace = "http://maven.apache.org/POM/4.0.0")]
    public class PomFile
    {
        [XmlElement(ElementName = "modelVersion")]
        public string ModelVersion { get; set; }

        [XmlElement(ElementName = "groupId")] public string GroupId { get; set; }

        [XmlElement(ElementName = "artifactId")]
        public string ArtifactId { get; set; }

        [XmlElement(ElementName = "version")] public string Version { get; set; }

        [XmlElement(ElementName = "packaging")]
        public string Packaging { get; set; }

        [XmlElement(ElementName = "dependencies")]
        public PomDependencies PomDependencies { get; set; }

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }

        [XmlAttribute(AttributeName = "schemaLocation")]
        public string SchemaLocation { get; set; }

        [XmlAttribute(AttributeName = "xsi")] public string Xsi { get; set; }

        [XmlText] public string Text { get; set; }

        public static PomFile DeserializeFromFile(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PomFile));
            PomFile pomFileData = null;
            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    pomFileData = (PomFile) (serializer.Deserialize(reader));
                }
            }

            return pomFileData;
        }
    }
}
