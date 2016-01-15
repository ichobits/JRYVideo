﻿using JryVideo.Common;
using JryVideo.Viewer.VideoViewer;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Enums;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JryVideo.Main
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public const string Caption = "JRY VIDEO";
        private MainPage MainPage;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.Window.SourceInitialized"/> 事件。
        /// </summary>
        /// <param name="e">一个 <see cref="T:System.EventArgs"/>，其中包含事件数据。</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.CaptionTextBlock.Text = Caption;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.MainPage = new MainPage();
                this.MainPage.VideoSelected += this.MainPage_VideoSelected;
                this.MainFrame.Navigate(this.MainPage);
                this.BeginRefresh();
            }
        }

        private void BeginRefresh()
        {
            Task.Run(async () =>
            {
                var lastDay = DateTime.Now;
                while (true)
                {
                    var bs = Process.GetCurrentProcess().WorkingSet64.GetByteSize();
                    if (bs.OriginValue > 1024 * 1024 * 500)
                    {
                        GC.Collect();
                        bs = Process.GetCurrentProcess().WorkingSet64.GetByteSize();
                    }
                    await this.Dispatcher.BeginInvoke(() => this.MemoryTextBlock.Text = bs.ToString());
                    var now = DateTime.Now;
                    if (now.Day != lastDay.Day)
                    {
                        lastDay = now;
                        await this.Dispatcher.BeginInvoke(() => this.MainPage?.Refresh());
                    }
                    await Task.Delay(1000);
                }
            });
        }

        void MainPage_VideoSelected(object sender, VideoInfoViewModel e)
        {
            this.NavigateToVideoViewerPage(e);
        }

        private async void NavigateToVideoViewerPage(VideoInfoViewModel info)
        {
            var page = VideoViewerPage.BuildPage(info);
            page.GoBackButton.Click += this.VideoViewerPage_GoBackButton_Click;
            page.ViewModel.InfoView.SeriesView.PropertiesRefreshed += this.InfoView_PropertiesRefreshed;
            this.CaptionTextBlock.Text = Caption + " | " + info.SeriesView.DisplayNameFirstLine;
            this.MainFrame.Navigate(page);
            await page.ViewModel.LoadAsync();
        }

        private void InfoView_PropertiesRefreshed(object sender, EventArgs e)
        {
            this.CaptionTextBlock.Text = Caption + " | " + ((SeriesViewModel)sender).DisplayNameFirstLine;
        }

        void VideoViewerPage_GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Click -= this.VideoViewerPage_GoBackButton_Click;

            var page = (VideoViewerPage) this.MainFrame.Content;
            Debug.Assert(page != null);
            page.ViewModel.InfoView.SeriesView.PropertiesRefreshed -= this.InfoView_PropertiesRefreshed;
            page.ViewModel.Flush();
            this.CaptionTextBlock.Text = Caption;
            Debug.Assert(this.MainFrame.CanGoBack);
            this.MainFrame.GoBack();
        }
    }
}
