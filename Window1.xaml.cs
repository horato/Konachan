/*
 * Created by SharpDevelop.
 * User: TomHoracek
 * Date: 9.7.2015
 * Time: 11:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using MahApps.Metro.Controls;

namespace Konachan
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : MetroWindow
	{
		private System.Timers.Timer Timer = new System.Timers.Timer();
		private List<Post> Container = new List<Post>();
		private List<Post> Imgs = new List<Post>();
		
		public Window1()
		{
			InitializeComponent();
			
			new Thread(getImages).Start();
			Timer.AutoReset = true;
			Timer.Elapsed += timer_Elapsed;
			Timer.Interval = 2000;
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (Imgs.Count == 0)
				Imgs.AddRange(Container);
			
			var rnd = new Random();
			int index = rnd.Next(0, Imgs.Count - 1);
			Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
					{
						if (index > 0 && index < Imgs.Count)
							backgroudImg.ImageUrl = Imgs[index];
					}));
			
			Imgs.RemoveAt(index);
		}
	
		private void getImages()
		{
			if (!Directory.Exists("cache"))
				Directory.CreateDirectory("cache");
			
			var client = new WebClient();
			var xml = client.DownloadString("https://konachan.com/post.xml?limit=100&tags=loli");
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			
			XmlNodeList elements = xmlDocument.GetElementsByTagName("post");
			
			foreach (XmlElement element in elements)
			{
				Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
						{
							var post = new Post(element.Attributes);
							var img = new CachedImage();
							
							img.Init(post);
							img.MouseDown += img_MouseDown;
							Container.Add(post);
							ImgListView.Items.Add(img);
							
							ProgressBar.Value = ((double)Container.Count / (double)elements.Count) * 100;
						}));
			}
			                                                            	
			Imgs.AddRange(Container);
			Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
					{
						ProgressBar.Visibility = Visibility.Hidden;
						ProgressBar.Value = 0;
					}));
		}

		void img_MouseDown(object sender, MouseButtonEventArgs e)
		{
			backgroundImgGrid.Visibility = Visibility.Visible;
			
			var img = (CachedImage)sender;
			backgroudImg.ImageUrl = img.ImageData;
		}

		void img_DownloadCompleted(object sender, EventArgs e)
		{
			backgroudImg.Visibility = Visibility.Visible;
		}
		
		void BackgroudImg_MouseDown(object sender, MouseButtonEventArgs e)
		{
			backgroudImg.ImageUrl = null;
			backgroudImg.Visibility = Visibility.Hidden;
			backgroundImgGrid.Visibility = Visibility.Collapsed;
		}
		
		void MetroWindow_Closing(object sender, CancelEventArgs e)
		{
			if(Directory.Exists("cache"))
			   Directory.Delete("cache", true);
		}
	}
}
