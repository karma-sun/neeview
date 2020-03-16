using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// データ互換用
    /// </summary>
    [Obsolete]
    public class Preference
    {
        /// <summary>
        /// Properties
        /// </summary>

        public int _Version { get; set; } = Environment.ProductVersionNumber;

        [DataMember, DefaultValue(true)]
        [PropertyMember]
        public bool _configure_enabled { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        public bool openbook_begin_current { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(2)]
        public int loader_thread_size { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        public bool dpi_image_ignore { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        public bool dpi_window_ignore { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(1.0)]
        public double view_image_wideratio { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        public bool userdata_save_disable { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(30.0)]
        public double input_gesture_minimumdistance_x { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(30.0)]
        public double input_gesture_minimumdistance_y { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(1.0)]
        public double panel_autohide_delaytime { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        public bool book_is_prioritize_pagemove { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        public bool book_allow_multiple_pagemove { get; set; }

        [Obsolete]
        [DataMember, DefaultValue("4096x4096")]
        public string book_preload_limitsize { get; set; }

        [Obsolete]
        [DataMember, DefaultValue("")]
        public string loader_archiver_7z_dllpath { get; set; }

        [Obsolete]
        [DataMember, DefaultValue("")]
        public string loader_archiver_7z_dllpath_x64 { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(".7z;.rar;.lzh")]
        public string loader_archiver_7z_supprtfiletypes { get; set; }

        [Obsolete]
        [DataMember, DefaultValue("__MACOSX;.DS_Store")]
        public string loader_archiver_exclude { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(-1.0)]
        public double loader_archiver_7z_locktime { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(80)]
        public int thumbnail_quality { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        public bool thumbnail_cache { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(1000)]
        public int thumbnail_book_capacity { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(200)]
        public int thumbnail_folder_capacity { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        public bool folderlist_addfile_insert { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(2.0)]
        public double loupe_scale_default { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(2.0)]
        public double loupe_scale_min { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(10.0)]
        public double loupe_scale_max { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(1.0)]
        public double loupe_scale_step { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        public bool loupe_scale_reset { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        public bool loupe_pagechange_reset { get; set; }


        [Obsolete]
        [DataMember, DefaultValue(true)]
        public bool file_remove_confirm { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        public bool file_permit_command { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        public bool network_enabled { get; set; }

        [Obsolete]
        [DataMember(Name = "WindowChromeFrame")]
        public WindowChromeFrameV1 window_chrome_frame { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        public bool window_captionemunate_fullscreen { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(1.0)]
        public double input_longbuttondown_time { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(200)]
        public int banner_width { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        public bool bootup_lastfolder { get; set; }



        /// <summary>
        /// 
        /// </summary>
        public Preference()
        {
            _document = new PropertyDocument(this);
        }

        private PropertyDocument _document;



        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember]
            public Dictionary<string, string> Items { get; set; } = new Dictionary<string, string>();

            public void Add(string key, string value)
            {
                Items[key] = value;
            }
        }


        /// <summary>
        /// Memento適用
        /// </summary>
        /// <param name="memento"></param>
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this._document.Reset();

            this._Version = memento._Version;

            if (memento.Items != null)
            {
                foreach (var item in memento.Items)
                {
                    try
                    {
                        var path = item.Key.Replace('.', '_');
                        var element = _document.GetPropertyMember(path);
                        if (element != null)
                        {
                            element.SetValueFromString(item.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }

        // Appへの適用のみ
        public void RestoreCompatibleApp()
        {
            // compatible before ver.23
            if (_Version < Environment.GenerateProductVersionNumber(1, 23, 0))
            {
                App.Current.IsNetworkEnabled = this.network_enabled;
                App.Current.IsIgnoreImageDpi = this.dpi_image_ignore;
                App.Current.AutoHideDelayTime = this.panel_autohide_delaytime;
                App.Current.WindowChromeFrame = this.window_chrome_frame == WindowChromeFrameV1.None ? WindowChromeFrame.None : WindowChromeFrame.WindowFrame;
                App.Current.IsOpenLastBook = this.bootup_lastfolder;
            }
        }

        // App以外への適用のみ
        public void RestoreCompatible()
        {
            // compatible before ver.23
            if (_Version < Environment.GenerateProductVersionNumber(1, 23, 0))
            {
                FileIOProfile.Current.IsRemoveConfirmed = this.file_remove_confirm;
                FileIOProfile.Current.IsEnabled = this.file_permit_command;

                Config.Current.Performance.JobWorkerSize = this.loader_thread_size;

                SevenZipArchiverProfile.Current.X86DllPath = this.loader_archiver_7z_dllpath;
                SevenZipArchiverProfile.Current.X64DllPath = this.loader_archiver_7z_dllpath_x64;
                SevenZipArchiverProfile.Current.SupportFileTypes.OneLine = this.loader_archiver_7z_supprtfiletypes;
                SevenZipArchiverProfile.Current.SupportFileTypes.Add(".cbr");
                SevenZipArchiverProfile.Current.SupportFileTypes.Add(".cbz");

                ThumbnailProfile.Current.Quality = this.thumbnail_quality;
                ThumbnailProfile.Current.IsCacheEnabled = this.thumbnail_cache;
                Config.Current.Performance.ThumbnailPageCapacity = this.thumbnail_book_capacity;
                Config.Current.Performance.ThumbnailBookCapacity = this.thumbnail_folder_capacity;
                SidePanelProfile.Current.BannerItemImageWidth = this.banner_width;

                MainWindowModel.Current.IsOpenbookAtCurrentPlace = this.openbook_begin_current;

                BookProfile.Current.IsPrioritizePageMove = this.book_is_prioritize_pagemove;
                BookProfile.Current.IsMultiplePageMove = this.book_allow_multiple_pagemove;
                BookProfile.Current.WideRatio = this.view_image_wideratio;
                BookProfile.Current.Excludes.OneLine = this.loader_archiver_exclude;

                MouseInput.Current.Normal.LongButtonDownTime = this.input_longbuttondown_time;

                MouseInput.Current.Gesture.GestureMinimumDistance = this.input_gesture_minimumdistance_x;

                MouseInput.Current.Loupe.MinimumScale = this.loupe_scale_min;
                MouseInput.Current.Loupe.MaximumScale = this.loupe_scale_max;
                MouseInput.Current.Loupe.DefaultScale = this.loupe_scale_default;
                MouseInput.Current.Loupe.ScaleStep = this.loupe_scale_step;
                MouseInput.Current.Loupe.IsResetByRestart = this.loupe_scale_reset;
                MouseInput.Current.Loupe.IsResetByPageChanged = this.loupe_pagechange_reset;

                MenuBar.Current.IsCaptionEmulateInFullScreen = this.window_captionemunate_fullscreen;

                BookshelfFolderList.Current.IsInsertItem = this.folderlist_addfile_insert;
            }
        }

        #endregion
    }
}
