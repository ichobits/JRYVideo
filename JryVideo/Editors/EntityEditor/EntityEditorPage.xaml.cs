﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using JryVideo.Model;
using JryVideo.Selectors.FlagSelector;
using MahApps.Metro.Controls;

namespace JryVideo.Editors.EntityEditor
{
    /// <summary>
    /// EntityCreatorPage.xaml 的交互逻辑
    /// </summary>
    public partial class EntityEditorPage : Page
    {
        public EntityEditorViewModel ViewModel { get; private set; }

        public EntityEditorPage()
        {
            this.InitializeComponent();
        }

        public EntityEditorPage(Model.JryVideo video)
            : this()
        {
            this.DataContext = this.ViewModel = new EntityEditorViewModel(video);
            this.ViewModel.LoadAsync();
        }

        private void SelectFansubButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.SelectFlag(JryFlagType.EntityFansub);
        }

        private void SelectSubTitleLanguageButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.SelectFlag(JryFlagType.EntitySubTitleLanguage);
        }

        private void SelectTrackLanguageButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.SelectFlag(JryFlagType.EntityTrackLanguage);
        }

        private void SelectFlag(JryFlagType type)
        {
            var flags = this.ViewModel[type];

            var dlg = new FlagSelectorWindow(type, flags)
            {
                Owner = this.TryFindParent<Window>()
            };

            if (dlg.ShowDialog() == true)
            {
                flags.Clear();
                flags.AddRange(dlg.ViewModel.SelectedItems.Select(z => z.Source.Value));
            }
        }


        private void CopyFlagMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var flag = ((FrameworkElement) sender).DataContext as string;

            if (flag != null)
            {
                Clipboard.SetText(flag);
            }
        }
    }
}