﻿using System;
using System.Diagnostics;
using System.Windows;
using Jasily.Diagnostics;
using JryVideo.Add.VideoCreator;
using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Selectors.SeriesSelector;
using JryVideo.Viewer.SeriesItemViewer;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace JryVideo.Add
{
    /// <summary>
    /// SelectSeriesWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddWindow : MetroWindow
    {
        private readonly SeriesSelectorPage seriesSelectorPage;
        private SeriesItemViewerPage seriesItemViewerPage;
        private VideoCreatorPage videoCreatorPage;

        public JryVideoInfo DialogResultObject { get; set; }

        public bool IsCommited { get; set; }

        public AddWindow()
        {
            this.InitializeComponent();

            this.seriesSelectorPage = new SeriesSelectorPage();
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.Window.SourceInitialized"/> 事件。
        /// </summary>
        /// <param name="e">一个 <see cref="T:System.EventArgs"/>，其中包含事件数据。</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.NavigateToSeriesSelectorPage();
        }

        private async void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = this.seriesSelectorPage.SelectorViewModel.Items.Selected;

            if (selected == null)
            {
                await this.ShowMessageAsync("error", "must select a series");
                return;
            }

            if (this.ContentFrame.Content == this.seriesSelectorPage)
            {
                this.NavigateToSeriesItemViewerPage(selected);
                return;
            }
            else if (this.ContentFrame.Content == this.seriesItemViewerPage)
            {
                this.NavigateToCreateVideoPage(selected);
                return;
            }
        }

        private void NavigateToSeriesSelectorPage()
        {
            this.TitleTextBlock.Text = "select series";
            this.LastButton.Visibility = Visibility.Hidden;

            JasilyDebug.Pointer();
            this.ContentFrame.Navigate(this.seriesSelectorPage);
            JasilyDebug.Pointer();
        }

        private void NavigateToSeriesItemViewerPage(SeriesViewModel series)
        {
            Debug.Assert(series != null);
            this.TitleTextBlock.Text = "sure video was not exists";
            this.LastButton.Visibility = this.NextButton.Visibility = Visibility.Visible;

            if (this.seriesItemViewerPage?.GetCurrentSeriesViewModel() != series)
            {
                var page = new SeriesItemViewerPage();
                page.SelectVideoControl.OnCommited += this.SelectVideoControl_OnCommited;
                page.Initialize(series);
                this.seriesItemViewerPage = page;
            }

            this.ContentFrame.Navigate(this.seriesItemViewerPage);
        }

        private void SelectVideoControl_OnCommited(object sender, EventArgs e) => this.IsCommited = true;

        private void NavigateToCreateVideoPage(SeriesViewModel series)
        {
            this.TitleTextBlock.Text = "create video";
            this.NextButton.Visibility = Visibility.Hidden;

            if (this.videoCreatorPage == null || this.videoCreatorPage.CreatorViewModel.Source != series)
            {
                this.videoCreatorPage = new VideoCreatorPage(series);
            }

            this.ContentFrame.Navigate(this.videoCreatorPage);
        }

        private void LastButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ContentFrame.CanGoBack)
            {
                if (this.ContentFrame.Content is VideoCreatorPage)
                {
                    this.TitleTextBlock.Text = "sure video was not exists";
                }
                else if (this.ContentFrame.Content is SeriesItemViewerPage)
                {
                    this.TitleTextBlock.Text = "select series";
                }

                this.ContentFrame.GoBack();
            }

            this.NextButton.Visibility = Visibility.Visible;
            this.LastButton.Visibility = this.ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
