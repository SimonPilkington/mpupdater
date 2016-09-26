using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace mpupdater.Properties
{
	public class AppDirectorySettingsProvider : SettingsProvider
	{
		private readonly string configPath = Path.Combine("./mpupdater.config");
		private readonly XmlDocument settingsDocument;
		private readonly XmlNode rootNode;

		private const string rootNodeName = "settings";
		private const string settingNodeName = "setting";
		private const string settingNodeIdAttributeName = "name";

		public AppDirectorySettingsProvider()
		{
			settingsDocument = new XmlDocument();

			try
			{
				settingsDocument.Load(configPath);
			}
			catch (FileNotFoundException)
			{
				SetupXml();
			}

			rootNode = settingsDocument.SelectSingleNode(rootNodeName);
			if (rootNode == null)
			{
				settingsDocument.RemoveAll();
				SetupXml();

				rootNode = settingsDocument.ChildNodes[1];
			}
		}

		private void SetupXml()
		{
			settingsDocument.AppendChild(settingsDocument.CreateXmlDeclaration("1.0", "utf-8", string.Empty));
			settingsDocument.AppendChild(settingsDocument.CreateElement(rootNodeName));
		}

		public override string ApplicationName
		{
			get
			{
				return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
			}

			set { }
		}

		public override string Name => nameof(AppDirectorySettingsProvider);

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			base.Initialize(Name, config);
		}

		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
		{
			var result = new SettingsPropertyValueCollection();

			foreach (SettingsProperty property in collection)
			{
				var settingNode = rootNode.SelectSingleNode($@"/{rootNodeName}/{settingNodeName}[@{settingNodeIdAttributeName}='{property.Name}']");
				var value = new SettingsPropertyValue(property);

				if (settingNode == null)
					value.SerializedValue = property.DefaultValue ?? string.Empty;
				else
					value.SerializedValue = settingNode.InnerText;

				result.Add(value);
			}

			return result;
		}

		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
		{
			foreach(SettingsPropertyValue value in collection)
			{
				var settingNode = rootNode.SelectSingleNode($@"/{rootNodeName}/{settingNodeName}[@{settingNodeIdAttributeName}='{value.Name}']");

				if (settingNode == null)
				{
					settingNode = settingsDocument.CreateElement(settingNodeName);

					var idAttribute = settingsDocument.CreateAttribute(settingNodeIdAttributeName);
					idAttribute.InnerText = value.Name;

					settingNode.Attributes.Append(idAttribute);
					rootNode.AppendChild(settingNode);
				}

				settingNode.InnerText = GetSerializedValueString(value);
			}

			settingsDocument.Save(configPath);
		}

		private string GetSerializedValueString(SettingsPropertyValue value)
		{
			string serializedValue = value.SerializedValue as string;

			if (serializedValue == null && value.Property.SerializeAs == SettingsSerializeAs.Binary)
			{
				var buf = value.SerializedValue as byte[];
				if (buf != null)
					serializedValue = Convert.ToBase64String(buf);
			}

			return serializedValue ?? string.Empty;
		}
	}
}
