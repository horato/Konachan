/*
 * Created by SharpDevelop.
 * User: TomHoracek
 * Date: 13.7.2015
 * Time: 9:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml;

namespace Konachan
{
	/// <summary>
	/// Description of Tag.
	/// </summary>
	public class Tag
	{
		public int id;
		public string name;
		public int count;
		public int type;
		public bool ambiguous;
		
		public Tag(XmlAttributeCollection collection)
		{
			if(collection == null)
				return;
			
			var classType = typeof(Tag);
			
			foreach (XmlNode attribute in collection)
			{
				var field = classType.GetField(attribute.Name);
				if(field == null)
					continue;
				
				field.SetValue(this, Convert.ChangeType(attribute.Value, field.FieldType));
			}
		}
	}
}
