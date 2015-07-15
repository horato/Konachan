/*
 * Created by SharpDevelop.
 * User: TomHoracek
 * Date: 07/09/2015
 * Time: 15:04
 * cache
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Konachan
{
	/// <summary>
	/// Description of CachedImage.
	/// </summary>
	public class CachedImage : Image
	{
		private readonly string ExecutingDirectory;
		public event EventHandler DownloadCompleted;
		
		public Post ImageData;
		
		public CachedImage()
		{
			ExecutingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(CachedImage), new FrameworkPropertyMetadata(typeof(CachedImage)));
		}
		
		public readonly static DependencyProperty ImageUrlProperty = DependencyProperty.Register("ImageUrl", typeof(Post), typeof(CachedImage), new PropertyMetadata(default(Post), ImageUrlPropertyChanged));
		
		public Post ImageUrl
		{
			get
			{
				return (Post)GetValue(ImageUrlProperty);
			}
			set
			{
				SetValue(ImageUrlProperty, value);
			}
		}
		
		private static readonly object SafeCopy = new object();
		
		private static void ImageUrlPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue == null || e.NewValue.Equals(default(Post)))
				return;
			
			var data = (Post)e.NewValue;
			if (data.id == 0 || String.IsNullOrEmpty(data.jpeg_url))
				return;
			
			((CachedImage)obj).ImageData = data;
			
			var uri = new Uri(data.jpeg_url);
			var localFile = String.Format(Path.Combine("cache", data.id.ToString()));
			var tempFile = String.Format(Path.Combine("cache", Guid.NewGuid().ToString()));
			
			if (File.Exists(localFile))
			{
				SetSource((CachedImage)obj, localFile);
			}
			else
			{
				try
				{
					var webClient = new WebClient();
					webClient.DownloadFile(uri, tempFile);

					if (!File.Exists(localFile))
					{
						lock (SafeCopy)
						{
							File.Move(tempFile, localFile);
						}
					}
					SetSource((CachedImage)obj, localFile);
					
				}
				catch
				{
					File.Delete(tempFile);
					return;
				}
			}
		}
		
		private static void SetSource(CachedImage inst, String path)
		{
			try
			{
				var bitmapImage = new BitmapImage();
				var scale = new ScaleTransform(900 / inst.ImageData.jpeg_width, 600 / inst.ImageData.jpeg_height);
			
				bitmapImage.BeginInit();
				bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
				bitmapImage.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
				bitmapImage.EndInit();
			
				inst.Source = new TransformedBitmap(bitmapImage, scale);
				inst.onDownloadCompleted(inst, null);		
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString());
			}
		}

		protected void onDownloadCompleted(object sender, EventArgs e)
		{
			if (DownloadCompleted != null)
				DownloadCompleted(sender, e);
		}
		
		public void Init(Post post)
		{
			try
			{
				var img = new BitmapImage();
				var scale = new ScaleTransform(150 / post.preview_width, 125 / post.preview_height);
				
				img.BeginInit();
				img.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
				img.UriSource = new Uri(post.preview_url, UriKind.RelativeOrAbsolute);
				img.EndInit();
				
				Width = 150;
				Height = 125;
				Source = new TransformedBitmap(img, scale);	
				ImageData = post;
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString());
			}
		}
	}
}
