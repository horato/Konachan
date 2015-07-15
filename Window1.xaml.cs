/*
 * Created by SharpDevelop.
 * User: TomHoracek
 * Date: 9.7.2015
 * Time: 11:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
		private List<Post> Imgs = new List<Post>();
		private int Page = 1;
		
		public Window1()
		{
			InitializeComponent();
			
			WriteToLog.ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			WriteToLog.LogfileName = "Konachan.Log";
			WriteToLog.CreateLogFile();
			
			AppDomain.CurrentDomain.FirstChanceException += Log.CurrentDomain_FirstChanceException;
			AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomain_UnhandledException;
			
			TagsManager.GetInstance();
			Pinger.GetInstance().WebsiteStatusChanged += WebsiteStatusChanged;
		}
		
		private void getImages()
		{
			if (!Directory.Exists("cache"))
				Directory.CreateDirectory("cache");
			
			Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
					{
						ProgressBar.Visibility = Visibility.Hidden;
						ProgressbarText.Text = "Contacting konachan...";
						ProgressbarText.Visibility = Visibility.Visible;
					}));
			
			var downloadString = "http://konachan.com/post.xml?limit=100&page=" + Page;
			if (TagsManager.GetInstance().HasTags())
				downloadString += "&tags=" + TagsManager.GetInstance().GetTagsAsString();
			
			var client = new WebClient();
			var xml = client.DownloadString(downloadString);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			
			XmlNodeList elements = xmlDocument.GetElementsByTagName("post");
			
			Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
					{
						if (elements.Count > 0)
						{
							ProgressbarText.Text = "";
							ProgressbarText.Visibility = Visibility.Hidden;
							ProgressBar.Visibility = Visibility.Visible;
						}
						else
						{
							ProgressbarText.Text = "No images found.";
							return;
						}
					}));
			
			foreach (XmlElement element in elements)
			{
				Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
						{
							var post = new Post(element.Attributes);
							var img = new CachedImage();
							
							img.Init(post);
							img.MouseDown += img_MouseDown;
							Imgs.Add(post);
							ImgListView.Items.Add(img);
							
							ProgressBar.Value = (double)Imgs.Count - ((Page - 1) * 100.0);
						}));
			}
			Page++;
			Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
					{
						ProgressBar.Visibility = Visibility.Hidden;
						ProgressBar.Value = 0;
						SearchButton.IsEnabled = true;
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
		
		void BackgroudImg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			backgroudImg.ImageUrl = null;
			backgroudImg.Visibility = Visibility.Hidden;
			backgroundImgGrid.Visibility = Visibility.Collapsed;
		}
		
		void MetroWindow_Closing(object sender, CancelEventArgs e)
		{
			if (Directory.Exists("cache"))
				Directory.Delete("cache", true);
		}

		void ImgListView_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.VerticalOffset > 0 && e.VerticalOffset + e.ViewportHeight == e.ExtentHeight)
			{
				new Thread(getImages).Start();
			}
		}
		
		void TagTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (TagTextBox.Text == "")
					return;
				
				var tagName = TagTextBox.Text;
				var tag = new FormTag();
				
				tag.TagName.Text = tagName;
				tag.RemoveButton.Click += tag_RemoveButton_Click;
				TagsPanel.Children.Add(tag);
				TagsManager.GetInstance().AddTag(tagName);
				ClearTagsButton.Visibility = Visibility.Visible;
				TagTextBox.Text = "";
				return;
			}
		}

		void TagTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (TagTextBox.Text.Length < 2)
				return;
			
			TagTextBox.ItemsSource = TagsManager.GetInstance().GetTagHint(TagTextBox.Text + e.Key);
		}
		
		void tag_RemoveButton_Click(object sender, RoutedEventArgs e)
		{
			var tag = (Button)sender;		
			var toRemove = from r in TagsPanel.Children.OfType<FormTag>()
			               where ((FormTag)r).RemoveButton == tag
			               select r;
			var element = toRemove.First();
			
			TagsPanel.Children.Remove(element);
			TagsManager.GetInstance().RemoveTag(element.TagName.Text);
			
			if (TagsPanel.Children.Count == 0)
				ClearTagsButton.Visibility = Visibility.Collapsed;
		}
		
		void SearchButton_Click(object sender, RoutedEventArgs e)
		{
			SearchButton.IsEnabled = false;
			Page = 1;
			ImgListView.Items.Clear();
			new Thread(getImages).Start();
		}
		
		void ClearTagsButton_Click(object sender, RoutedEventArgs e)
		{
			TagsPanel.Children.Clear();
			TagsManager.GetInstance().Clear();
			ClearTagsButton.Visibility = Visibility.Collapsed;
		}

		void WebsiteStatusChanged(object sender, WebsiteStatusEventArgs e)
		{
			Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
					{
						switch (e.ChangedTo)
						{
							case WebsiteStatus.Up:
								StatusImage.Source = (BitmapImage)Resources["status_up"];
								break;
							case WebsiteStatus.Down:
								StatusImage.Source = (BitmapImage)Resources["status_down"];
								break;
						}
					}));		
		}
		void TagTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			AddTagTextBox.Visibility = Visibility.Hidden;
		}
		void TagTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			AddTagTextBox.Visibility = Visibility.Visible;
		}
		void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			
		}
	}
}
