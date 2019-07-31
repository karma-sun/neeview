using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class PageViewRecorder : BindableBase, IDisposable
    {
        // Singleton
        static PageViewRecorder() => Current = new PageViewRecorder();
        public static PageViewRecorder Current { get; }

        #region Fields

        private bool _isRecordPageView;
        private string _pageViewRecordPath;
        private TextWriter _writer;
        private object _lock = new object();

        #endregion Fields

        #region Constructors

        private PageViewRecorder()
        {
            BookHub.Current.ViewContentsChanged += OnViewContentsChanged;
            this.PropertyChanged += OnPropertyChanged;

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }

        #endregion Constructors

        #region Properties

        // 履歴を保存するか
        [PropertyMember("@ParamIsRecordPageView")]
        public bool IsRecordPageView
        {
            get { return _isRecordPageView; }
            set { SetProperty(ref _isRecordPageView, value); }
        }

        // 履歴データの保存場所
        [PropertyPath("@ParamPageViewRecordPath", FileDialogType = FileDialogType.SaveFile, Filter = "TSV|*.tsv")]
        public string PageViewRecordPath
        {
            get { return _pageViewRecordPath; }
            set { SetProperty(ref _pageViewRecordPath, value); }
        }

        #endregion Properties

        #region IDisposable Support
        public void Dispose()
        {
            lock (_lock)
            {
                if (_writer != null)
                {
                    _writer.Write(DateTime.Now.ToString("O"));
                    _writer.Write('\t');
                    _writer.Write("[Exit] ");
                    _writer.Write(Config.Current.ApplicationName);
                    _writer.Write(' ');
                    _writer.Write(Config.Current.DispVersion);
                    _writer.WriteLine();
                }
            }
            CloseWriter();
        }

        private void CloseWriter()
        {
            lock (_lock)
            {
                try
                {
                    _writer?.Close();
                }
                catch (Exception err)
                {
                    Debug.WriteLine("[Skip] {0}", err.Message);
                }
                finally
                {
                    _writer = null;
                }
            }
        }
        #endregion

        #region Callback Methods

        private void OnViewContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            var now = DateTime.Now;

            AppDispatcher.Invoke(() =>
            {
                var viewPages = e?.ViewPageCollection?.Collection.Where(x => x != null).Select(x => x.Page).ToList() ?? new List<Page>();

                lock (_lock)
                {
                    if (_writer == null)
                    {
                        return;
                    }

                    try
                    {
                        foreach (var page in viewPages)
                        {
                            _writer.Write(now.ToString("O"));
                            _writer.Write('\t');
                            _writer.Write(page.BookPrefix);
                            _writer.Write('\t');
                            _writer.Write(page.EntryFullName);
                            _writer.WriteLine();
                        }
                    }
                    catch (IOException err)
                    {
                        Debug.WriteLine("[Error] {0}", err.Message);
                        ToastService.Current.Show(new Toast(Resources.DialogPageViewRecordWriteError, null, ToastIcon.Error));
                    }
                }
            });
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                lock (_lock)
                {
                    CloseWriter();
                    if (!IsRecordPageView)
                    {
                        return;
                    }

                    if (String.IsNullOrEmpty(PageViewRecordPath))
                    {
                        return;
                    }

                    try
                    {
                        _writer = new StreamWriter(PageViewRecordPath, true);
                    }
                    catch (IOException err)
                    {
                        Debug.WriteLine("[Error] {0}", err.Message);
                        ToastService.Current.Show(new Toast(Resources.DialogPageViewRecordWriteError, null, ToastIcon.Error));
                    }
                }
            });
        }

        #endregion Callback Methods


        #region Memento

        /// <summary>
        /// PageViewRecorder Memento
        /// </summary>
        [DataContract]
        public class Memento : BindableBase
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember, DefaultValue(false)]
            public bool IsRecordPageView { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string PageViewRecordPath { get; set; }
        }

        // memento作成
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento._Version = Config.Current.ProductVersionNumber;

            memento.IsRecordPageView = IsRecordPageView;
            memento.PageViewRecordPath = PageViewRecordPath;

            return memento;
        }

        // memento反映
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            IsRecordPageView = memento.IsRecordPageView;
            PageViewRecordPath = memento.PageViewRecordPath;

        }

        #endregion
    }
}