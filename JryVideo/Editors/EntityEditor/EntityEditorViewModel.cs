﻿using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Controls.SelectFlag;
using JryVideo.Core;
using JryVideo.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Enums;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace JryVideo.Editors.EntityEditor
{
    public class EntityEditorViewModel : EditorItemViewModel<JryEntity>
    {
        private string resolution;
        private string filmSource;
        private string audioSource;
        private string extension;
        private bool isWildcardChecked = true;
        private bool isRegexChecked;
        private string format;

        public EntityEditorViewModel()
        {
            this.FansubsViewModel = new SelectFlagViewModel(
                JryFlagType.EntityFansub, new ObservableCollection<string>());

            this.SubTitleLanguagesViewModel = new SelectFlagViewModel(
                JryFlagType.EntitySubTitleLanguage, new ObservableCollection<string>());

            this.TrackLanguagesViewModel = new SelectFlagViewModel(
                JryFlagType.EntityTrackLanguage, new ObservableCollection<string>());

            this.TagsViewModel = new SelectFlagViewModel(
                JryFlagType.EntityTag, new ObservableCollection<string>());
        }

        public Model.JryVideo Video { get; private set; }

        public void Initialize(Model.JryVideo video)
        {
            this.Video = video;
        }

        public ObservableCollection<string> this[JryFlagType flagType]
        {
            get
            {
                switch (flagType)
                {
                    case JryFlagType.EntityFansub:
                        return this.FansubsViewModel.Collection;

                    case JryFlagType.EntitySubTitleLanguage:
                        return this.SubTitleLanguagesViewModel.Collection;

                    case JryFlagType.EntityTrackLanguage:
                        return this.TrackLanguagesViewModel.Collection;

                    case JryFlagType.EntityTag:
                        return this.TagsViewModel.Collection;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(flagType), flagType, null);
                }
            }
        }

        public SelectFlagViewModel FansubsViewModel { get; }

        public SelectFlagViewModel SubTitleLanguagesViewModel { get; }

        public SelectFlagViewModel TrackLanguagesViewModel { get; }

        public SelectFlagViewModel TagsViewModel { get; }

        public List<string> Resolutions { get; } = new List<string>();

        public List<string> FilmSources { get; } = new List<string>();

        public List<string> AudioSources { get; } = new List<string>();

        public List<string> Extensions { get; } = new List<string>();

        public async Task LoadAsync()
        {
            var manager = JryVideoCore.Current.CurrentDataCenter.FlagManager;

            this.Resolutions.AddRange((await manager.LoadAsync(JryFlagType.EntityResolution)).Select(z => z.Value));
            this.FilmSources.AddRange((await manager.LoadAsync(JryFlagType.EntityQuality)).Select(z => z.Value));
            this.AudioSources.AddRange((await manager.LoadAsync(JryFlagType.EntityAudioSource)).Select(z => z.Value));
            this.Extensions.AddRange((await manager.LoadAsync(JryFlagType.EntityExtension)).Select(z => z.Value));
        }

        [EditableField]
        public string Resolution
        {
            get { return this.resolution; }
            set { this.SetPropertyRef(ref this.resolution, value.IsNullOrWhiteSpace() ? null : value.Trim()); }
        }

        [EditableField]
        public string Quality
        {
            get { return this.filmSource; }
            set { this.SetPropertyRef(ref this.filmSource, value.IsNullOrWhiteSpace() ? null : value.Trim()); }
        }

        [EditableField]
        public string AudioSource
        {
            get { return this.audioSource; }
            set { this.SetPropertyRef(ref this.audioSource, value.IsNullOrWhiteSpace() ? null : value.Trim()); }
        }

        [EditableField]
        public string Extension
        {
            get { return this.extension; }
            set { this.SetPropertyRef(ref this.extension, value.IsNullOrWhiteSpace() ? null : value.Trim()); }
        }

        public bool IsWildcardChecked
        {
            get { return this.isWildcardChecked; }
            set { this.SetPropertyRef(ref this.isWildcardChecked, value); }
        }

        public bool IsRegexChecked
        {
            get { return this.isRegexChecked; }
            set { this.SetPropertyRef(ref this.isRegexChecked, value); }
        }

        public string Format
        {
            get { return this.format; }
            set
            {
                this.SetPropertyRef(ref this.format, value);
                this.TryParseFromFormatString();
            }
        }

        public override void ReadFromObject(JryEntity obj)
        {
            base.ReadFromObject(obj);

            this[JryFlagType.EntityFansub].Reset(obj.Fansubs);
            this[JryFlagType.EntitySubTitleLanguage].Reset(obj.SubTitleLanguages);
            this[JryFlagType.EntityTrackLanguage].Reset(obj.TrackLanguages);
            this[JryFlagType.EntityTag].Reset(obj.Tags);

            if (obj.Format != null)
            {
                switch (obj.Format.Type)
                {
                    case JryFormatType.Regex:
                        this.IsRegexChecked = true;
                        break;

                    case JryFormatType.Wildcard:
                        this.IsWildcardChecked = true;
                        break;
                }

                Debug.Assert(this.IsRegexChecked != this.IsWildcardChecked);

                this.Format = obj.Format.Value;
            }
        }

        public override void WriteToObject(JryEntity obj)
        {
            base.WriteToObject(obj);

            obj.Fansubs = this[JryFlagType.EntityFansub].Distinct().OrderBy(z => z).ToList();
            obj.SubTitleLanguages = this[JryFlagType.EntitySubTitleLanguage].Distinct().OrderBy(z => z).ToList();
            obj.TrackLanguages = this[JryFlagType.EntityTrackLanguage].Distinct().OrderBy(z => z).ToList();
            obj.Tags = this[JryFlagType.EntityTag].Distinct().OrderBy(z => z).ToList();

            if (!this.Format.IsNullOrWhiteSpace())
            {
                Debug.Assert(this.IsRegexChecked != this.IsWildcardChecked);

                obj.Format = new JryFormat()
                {
                    Type = this.IsRegexChecked ? JryFormatType.Regex : JryFormatType.Wildcard,
                    Value = this.Format
                };
            }
        }

        public async Task<JryEntity> CommitAsync(MetroWindow window)
        {
            if (JryEntity.IsExtensionInvalid(this.Extension))
            {
                await window.ShowMessageAsync("error", "invalid extension.");
                return null;
            }

            if (JryEntity.IsResolutionInvalid(this.Resolution))
            {
                await window.ShowMessageAsync("error", "invalid resolution.");
                return null;
            }

            var entity = this.GetCommitObject();

            this.WriteToObject(entity);

            var provider = JryVideoCore.Current.CurrentDataCenter.VideoManager.GetEntityManager(this.Video);

            if (this.Action == ObjectChangedAction.Create)
            {
                entity.BuildMetaData();

                if (provider.IsExists(entity))
                {
                    await window.ShowMessageAsync("error", "had same entity.");
                    return null;
                }
            }

            return await base.CommitAsync(provider, entity);
        }

        public async void ParseFiles(string[] files)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));

            files = files.Where(File.Exists).Select(Path.GetFileName).ToArray();
            if (files.Length == 0) return;
            this.Format = files.Length == 1 ? files[0] : await Task.Run(() => this.ParseCommonFileName(files));
        }

        private string[] fansubFlags;
        private string[] subTitleLanguagesFlags;
        private string[] trackLanguageFlags;
        private string[] tagFlags;

        private async void TryParseFromFormatString()
        {
            var format = this.Format;
            if (format.IsNullOrWhiteSpace() || this.Action != ObjectChangedAction.Create) return;

            if (this.Resolution.IsNullOrWhiteSpace())
            {
                this.Resolution = this.Resolutions
                    .FirstOrDefault(z => format.Contains(z.Replace("P", ""), StringComparison.OrdinalIgnoreCase))
                    ?? string.Empty;
            }
            if (this.Quality.IsNullOrWhiteSpace())
            {
                this.Quality = this.FilmSources
                    .FirstOrDefault(z => format.Contains(z, StringComparison.OrdinalIgnoreCase))
                    ?? string.Empty;
            }
            if (this.AudioSource.IsNullOrWhiteSpace())
            {
                this.AudioSource = this.AudioSources
                    .FirstOrDefault(z => format.Contains(z, StringComparison.OrdinalIgnoreCase))
                    ?? string.Empty;
            }
            if (this.Extension.IsNullOrWhiteSpace())
            {
                this.Extension = this.Extensions
                    .FirstOrDefault(z => format.Contains(z, StringComparison.OrdinalIgnoreCase))
                    ?? string.Empty;
            }

            var flagManager = this.GetManagers().FlagManager;
            if (this.fansubFlags == null)
            {
                this.fansubFlags = (await flagManager.LoadAsync(JryFlagType.EntityFansub))
                    .Select(z => z.Value).ToArray();
            }
            if (this.subTitleLanguagesFlags == null)
            {
                this.subTitleLanguagesFlags = (await flagManager.LoadAsync(JryFlagType.EntitySubTitleLanguage))
                    .Select(z => z.Value).ToArray();
            }
            if (this.trackLanguageFlags == null)
            {
                this.trackLanguageFlags = (await flagManager.LoadAsync(JryFlagType.EntityTrackLanguage))
                    .Select(z => z.Value).ToArray();
            }
            if (this.tagFlags == null)
            {
                this.tagFlags = (await flagManager.LoadAsync(JryFlagType.EntityTag))
                    .Select(z => z.Value).ToArray();
            }

            var mapper = ((App)Application.Current).UserConfig?.Mapper;
            if (mapper != null)
            {
                var filter = new Func<string, bool>(z => !z.IsNullOrWhiteSpace());
                foreach (var flagTemplate in new[]
                {
                    new { Type = JryFlagType.EntityFansub, Flags = this.fansubFlags },
                    new { Type = JryFlagType.EntitySubTitleLanguage, Flags = this.subTitleLanguagesFlags },
                    new { Type = JryFlagType.EntityTrackLanguage, Flags = this.trackLanguageFlags },
                    new { Type = JryFlagType.EntityTag, Flags = this.tagFlags },
                })
                {
                    var col = this[flagTemplate.Type];
                    col.Reset(col.ToArray()
                        .Concat(await mapper.TryFireAsync(flagTemplate.Type, format, filter))
                        .Concat(flagTemplate.Flags.Where(z => format.Contains(z, StringComparison.OrdinalIgnoreCase)))
                        .Distinct());
                }
            }
        }

        private static readonly Regex Crc32 = new Regex(@"(.*)(\[[a-f0-9]{7,8}\]|\([a-f0-9]{7,8}\))$", RegexOptions.IgnoreCase);

        private string ParseCommonFileName(string[] source)
        {
            if (source.Length == 0 || source.Length == 1) throw new ArgumentOutOfRangeException();

            var configExtensions = ((App)Application.Current).UserConfig?.SubTitleExtensions;
            if (configExtensions != null)
            {
                var es = configExtensions
                    .Select(z => (z.StartsWith(".") ? z : "." + z).ToLower())
                    .ToArray();
                var subTitles = new List<string>();
                source = source.Where(z =>
                {
                    if (es.Any(x => z.ToLower().EndsWith(x)))
                    {
                        subTitles.Add(Path.GetFileNameWithoutExtension(z));
                        return false;
                    }
                    return true;
                }).ToArray();

                var mapper = ((App)Application.Current).UserConfig?.Mapper;
                if (mapper != null && subTitles.Count > 0)
                {
                    var filter = new Func<string, bool>(z => !z.IsNullOrWhiteSpace());
                    var sttags = subTitles.Select(Path.GetExtension).Distinct().ToArray();
                    var langs = sttags
                        .SelectMany(z => mapper.TryFire(mapper.ExtendSubTitleLanguages, z, filter))
                        .Distinct()
                        .ToArray();
                    var fansubs = sttags
                        .SelectMany(z => mapper.TryFire(mapper.Fansubs, z, filter))
                        .Distinct()
                        .ToArray();
                    this.GetUIDispatcher().BeginInvoke(() =>
                    {
                        this[JryFlagType.EntityFansub].Reset(this[JryFlagType.EntityFansub]
                            .ToArray()
                           .Concat(fansubs)
                           .Distinct());
                        this[JryFlagType.EntitySubTitleLanguage].Reset(this[JryFlagType.EntitySubTitleLanguage]
                            .ToArray()
                            .Concat(langs)
                            .Distinct());
                    });
                }
            }

            if (source.Length == 0) return string.Empty;
            if (source.Length == 1) return source[0];

            var exts = source.Select(Path.GetExtension).ToArray();
            var ext = exts[0];
            if (exts.Skip(1).All(z => z == ext))
            {
                var names = source.Select(Path.GetFileNameWithoutExtension).ToArray();
                var matchs = names.Select(z => Crc32.Match(z)).ToArray();
                if (matchs.All(z => z.Success))
                {
                    var left = matchs[0].Groups[2].Value[0];
                    if (matchs.All(z => z.Groups[2].Value[0] == left))
                    {
                        var strsL = matchs.Select(z => z.Groups[1].Value).ToArray();
                        return ParseCommonString(strsL) +
                            matchs[0].Groups[2].Value[0] +
                            "*" +
                            matchs[0].Groups[2].Value.Last() +
                            ext;
                    }
                }
            }

            return ParseCommonString(source);
        }

        private static string ParseCommonString(string[] source)
            => source.CommonStart() + "*" + source.CommonEnd();
    }
}