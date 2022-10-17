using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace TTPlugins.DependenciesFile
{
	[XmlRoot(ElementName = "repositories")]
	public class Repositories
	{
		[XmlElement(ElementName = "repository")]
		public List<string> Repository { get; set; }
	}

	[XmlRoot(ElementName = "androidSdkPackageIds")]
	public class AndroidSdkPackageIds
	{
		[XmlElement(ElementName = "androidSdkPackageId")]
		public string AndroidSdkPackageId { get; set; }
	}

	[XmlRoot(ElementName = "androidPackage")]
	public class AndroidPackage
	{
		[XmlElement(ElementName = "androidSdkPackageIds")]
		public AndroidSdkPackageIds AndroidSdkPackageIds { get; set; }

		[XmlElement(ElementName = "repositories")]
		public Repositories Repositories { get; set; }

		[XmlAttribute(AttributeName = "spec")] public string Spec { get; set; }

		[XmlText] public string Text { get; set; }
	}

	[XmlRoot(ElementName = "androidPackages")]
	public class AndroidPackages
	{
		[XmlElement(ElementName = "repositories")]
		public Repositories Repositories { get; set; }

		[XmlElement(ElementName = "androidPackage")]
		public List<AndroidPackage> AndroidPackage { get; set; }
	}

	[XmlRoot(ElementName = "sources")]
	public class Sources
	{
		[XmlElement(ElementName = "source")] public List<string> Source { get; set; }
	}

	[XmlRoot(ElementName = "iosPod")]
	public class IosPod
	{
		[XmlElement(ElementName = "sources")] public Sources Sources { get; set; }

		[XmlAttribute(AttributeName = "name")] public string Name { get; set; }

		[XmlAttribute(AttributeName = "path")] public string Path { get; set; }

		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; }

		[XmlAttribute(AttributeName = "bitcodeEnabled")]
		public bool BitcodeEnabled { get; set; }

		[XmlAttribute(AttributeName = "minTargetSdk")]
		public double MinTargetSdk { get; set; }

		[XmlAttribute(AttributeName = "addToAllTargets")]
		public bool AddToAllTargets { get; set; }

		[XmlText] public string Text { get; set; }
	}

	[XmlRoot(ElementName = "iosPods")]
	public class IosPods
	{
		[XmlElement(ElementName = "sources")] public Sources Sources { get; set; }

		[XmlElement(ElementName = "iosPod")] public List<IosPod> IosPod { get; set; }
	}

	[XmlRoot(ElementName = "dependencies", IsNullable = true)]
	public class DependenciesFile
	{
		[XmlElement(ElementName = "androidPackages")]
		public AndroidPackages AndroidPackages { get; set; }

		[XmlElement(ElementName = "iosPods")] public IosPods IosPods { get; set; }

		[XmlIgnore] private string FilePath { get; set; }

		public static DependenciesFile DeserializeFromFile(string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(DependenciesFile));
			DependenciesFile depsFileData = null;
			if (File.Exists(path))
			{
				using (StreamReader reader = new StreamReader(path))
				{
					depsFileData = (DependenciesFile) (serializer.Deserialize(reader));
					depsFileData.FilePath = path;
				}
			}

			return depsFileData;
		}

		public void SerializeToFile(string path = null)
		{
			var serializer = new XmlSerializer(GetType());
			var savePath = path; 
			if (path == null && FilePath != null)
			{
				savePath = FilePath;
			}

			if (savePath != null)
			{
				TextWriter writer = new StreamWriter(savePath);
				serializer.Serialize(writer, this);
				writer.Close();
			}
			else
			{
				Debug.Log("Path to save is empty");
			}
		}
	}
}
