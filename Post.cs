/*
 * Created by SharpDevelop.
 * User: tomhoracek
 * Date: 07/10/2015
 * Time: 11:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml;

namespace Konachan
{
	/// <summary>
	/// Description of Post.
	/// </summary>
	public class Post
	{
		public double actual_preview_height;
		public double actual_preview_width;
		public string author;
		public string change;
		public string created_at;
		public string creator_id;
		public string file_size;
		public string file_url;
		public string frames;
		public string frames_pending;
		public string frames_pending_string;
		public string frames_string;
		public string has_children;
		public double height;
		public int id;
		public string is_held;
		public string is_shown_in_index;
		public string jpeg_file_size;
		public double jpeg_height;
		public string jpeg_url;
		public double jpeg_width;
		public string md5;
		public double preview_height;
		public string preview_url;
		public double preview_width;
		public string rating;
		public string sample_file_size;
		public double sample_height;
		public string sample_url;
		public double sample_width;
		public string score;
		public string source;
		public string status;
		public string tags;
		public double width;
		
		public Post()
		{
		
		}
		
		public Post(XmlAttributeCollection collection)
		{	
			if(collection == null)
				return;
			var type = typeof(Post);
			foreach (XmlNode attribute in collection)
			{
				var field = type.GetField(attribute.Name);
				if(field == null)
					continue;
				
				field.SetValue(this, Convert.ChangeType(attribute.Value, field.FieldType));
			}
		}
	}
}
