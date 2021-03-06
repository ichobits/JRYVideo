﻿using System;
using System.Windows;
using System.Windows.Controls;
using Jasily.Diagnostics;
using JryVideo.Common;
using JryVideo.Common.Dialogs;
using JryVideo.Model;
using MahApps.Metro.Controls;

namespace JryVideo.Selectors.SeriesSelector
{
    /// <summary>
    /// SelectSeriesPage.xaml 的交互逻辑
    /// </summary>
    public partial class SeriesSelectorPage : Page
    {
        public SeriesSelectorPage()
        {
            JasilyDebug.Pointer();
            this.InitializeComponent();
            JasilyDebug.Pointer();

            this.EditSeriesUserControl.ViewModel.Created += this.EditSeriesUserControl_ViewModel_Created;
        }

        public SeriesSelectorViewModel SelectorViewModel { get; } = new SeriesSelectorViewModel();

        void EditSeriesUserControl_ViewModel_Created(object sender, Series e)
        {
            if (this.GetUIDispatcher().CheckAccessOrBeginInvoke(this.EditSeriesUserControl_ViewModel_Created, sender, e))
            {
                if (this.SelectorViewModel != null)
                {
                    var vm = new SeriesViewModel(e);
                    this.SelectorViewModel.Items.Collection.Add(vm);
                    this.SelectorViewModel.Items.Selected = vm;
                    this.SeriesListView.ScrollIntoView(vm);
                }
            }
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.FrameworkElement.Initialized"/> 事件。 每当在内部将 <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> 设置为 true 时，都将调用此方法。
        /// </summary>
        /// <param name="e">包含事件数据的 <see cref="T:System.Windows.RoutedEventArgs"/>。</param>
        protected override async void OnInitialized(EventArgs e)
        {
            JasilyDebug.Pointer();
            base.OnInitialized(e);

            this.EditSeriesUserControl.ViewModel.CreateMode();
            this.DataContext = this.SelectorViewModel;

            await this.SelectorViewModel.LoadAsync();
            JasilyDebug.Pointer();
        }

        private void EditMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var seriesViewModel = ((FrameworkElement)sender).DataContext as SeriesViewModel;
            seriesViewModel?.OpenEditorWindows(this.TryFindParent<Window>());
        }

        private void RemoveMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var series = ((FrameworkElement)sender).DataContext as SeriesViewModel;
            if (series == null) return;
            if (series.Source.Videos.Count > 0)
            {
                this.ShowJryVideoMessage("ERROR", "before remove series, please remove all video in the series.");
            }
            else
            {
                this.SelectorViewModel.Remove(series);
            }
        }
    }
}
