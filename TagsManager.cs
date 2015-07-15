/*
 * Created by SharpDevelop.
 * User: TomHoracek
 * Date: 13.7.2015
 * Time: 9:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows.Documents;
using System.Xml;

namespace Konachan
{
	/// <summary>
	/// Description of TagsManager.
	/// </summary>
	public class TagsManager
	{
		private static TagsManager _instance;
		private List<string> Tags = new List<string>();
		
		private TagsManager()
		{
			
		}
		
		public static TagsManager GetInstance()
		{
			if (_instance == null)
				_instance = new TagsManager();
			
			return _instance;
		}
		
		private List<string> fetchTagHint(string tag)
		{
			var ret = new List<string>();
			var dlString = "http://konachan.com/tag.xml?order=count&limit=10";
			if (!String.IsNullOrWhiteSpace(tag))
				dlString += "&name=" + tag;
			
			var client = new WebClient();
			var xml = client.DownloadString(dlString);
			
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			
			XmlNodeList elements = xmlDocument.GetElementsByTagName("tag");
			foreach (XmlElement element in elements)
			{
				var t = new Tag(element.Attributes);
				ret.Add(convertFromUnderscore(t.name));
			}
			ret.Sort((a, b) => a.CompareTo(b));
			return ret;
		}
		
		public List<string> GetTagHint(string tag)
		{
			if (tag == null)
				return new List<string>();
			
			var fetch = convertToUnderscore(tag.ToLower());
			return fetchTagHint(fetch);
		}
		
		public List<string> GetTags()
		{
			return Tags;
		}
		
		public string GetTagsAsString()
		{
			var builder = new StringBuilder();
			
			foreach(var tag in Tags)
				builder.Append(tag + " ");
			
			return builder.ToString().TrimEnd(' ');
		}
		
		public void AddTag(string tag)
		{
			if (tag == null)
				return;
			
			Tags.Add(convertToUnderscore(tag));
		}
		
		public void RemoveTag(string tag)
		{
			if (tag == null)
				return;
			
			Tags.Remove(convertToUnderscore(tag));
		}
		
		public void Clear()
		{
			Tags.Clear();
		}
		
		public bool HasTags()
		{
			return Tags.Count > 0;
		}
		
		private string convertToUnderscore(string tag)
		{
			var split = tag.Split(' ');
			var builder = new StringBuilder();
			
			for (int i = 0; i < split.Length; i++)
			{
				builder.Append(split[i]);
				if (i < split.Length - 1)
					builder.Append("_");
			}
			
			return builder.ToString();
		}
		
		private string convertFromUnderscore(string tag)
		{
			var split = tag.Split('_');
			var builder = new StringBuilder();
			
			for (int i = 0; i < split.Length; i++)
			{
				builder.Append(split[i]);
				if (i < split.Length - 1)
					builder.Append(" ");
			}
			
			return builder.ToString();
		}
	}
}
