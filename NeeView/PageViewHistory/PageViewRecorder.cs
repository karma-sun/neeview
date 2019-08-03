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
using System.Text;

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
        private FileStream _file;
        private StringBuilder _writeBuffer;
        private DateTime _viewedPagesDateTime;
        private List<Page> _viewedPages;
        private DateTime _viewedBookDateTime;
        private string _viewedBookAddress;
        private string _viewedBookName;
        private object _lock = new object();

        #endregion Fields

        #region Constructors

        private PageViewRecorder()
        {
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

        private void WritePageViewedRecord(DateTime now)
        {
            lock (_lock)
            {
                if (_file == null)
                {
                    return;
                }

                _writeBuffer.Clear();
                foreach (var page in _viewedPages)
                {
                    _writeBuffer.Append(_viewedPagesDateTime.ToString("O"));
                    _writeBuffer.Append('\t');
                    switch (page.PageType)
                    {
                        case PageType.Folder:
                            _writeBuffer.Append("Folder");
                            break;
                        case PageType.File:
                            _writeBuffer.Append("File");
                            break;
                        default:
                            _writeBuffer.Append("Unknown");
                            break;
                    }
                    _writeBuffer.Append('\t');
                    _writeBuffer.Append((now - _viewedPagesDateTime).TotalSeconds.ToString("#0.0000000"));
                    _writeBuffer.Append('\t');
                    _writeBuffer.Append(page.BookPrefix);
                    _writeBuffer.Append('\t');
                    _writeBuffer.Append(page.EntryFullName);
                    _writeBuffer.AppendLine();
                }

                WriteString(_writeBuffer.ToString());
            }
        }

        private void WriteBookViewedRecord(DateTime now)
        {
            lock (_lock)
            {
                if (_file == null)
                {
                    return;
                }

                if (_viewedBookAddress == null)
                {
                    return;
                }

                _writeBuffer.Clear();
                _writeBuffer.Append(_viewedBookDateTime.ToString("O"));
                _writeBuffer.Append('\t');
                _writeBuffer.Append("Book");
                _writeBuffer.Append('\t');
                _writeBuffer.Append((now - _viewedBookDateTime).TotalSeconds.ToString("#0.0000000"));
                _writeBuffer.Append('\t');
                _writeBuffer.Append(_viewedBookAddress);
                _writeBuffer.Append('\t');
                _writeBuffer.Append(_viewedBookName);
                _writeBuffer.AppendLine();

                WriteString(_writeBuffer.ToString());
            }
        }

        private void WriteString(string text)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                _file.Seek(0L, SeekOrigin.End);
                _file.Write(bytes, 0, bytes.Length);
                _file.Flush();
            }
            catch (IOException err)
            {
                Debug.WriteLine("[Error] {0}", err.Message);
                ToastService.Current.Show(new Toast(Resources.DialogPageViewRecordWriteError, null, ToastIcon.Error));
            }
        }

        private void OpenFile()
        {
            lock (_lock)
            {
                try
                {
                    _file = File.Open(PageViewRecordPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    _writeBuffer = new StringBuilder(1024);
                }
                catch (IOException err)
                {
                    Debug.WriteLine("[Error] {0}", err.Message);
                    ToastService.Current.Show(new Toast(Resources.DialogPageViewRecordWriteError, null, ToastIcon.Error));
                }
                _viewedBookAddress = null;
                _viewedPages = new List<Page>();
                BookHub.Current.BookChanged += OnViewBookChanged;
                BookHub.Current.ViewContentsChanged += OnViewContentsChanged;
            }
        }

        private void CloseFile()
        {
            lock (_lock)
            {
                BookHub.Current.ViewContentsChanged -= OnViewContentsChanged;
                BookHub.Current.BookChanged -= OnViewBookChanged;
                try
                {
                    _file?.Close();
                }
                catch (Exception err)
                {
                    Debug.WriteLine("[Skip] {0}", err.Message);
                }
                finally
                {
                    _writeBuffer = null;
                    _file = null;
                }
            }
        }

        #region IDisposable Support
        public void Dispose()
        {
            var now = DateTime.Now;
            WritePageViewedRecord(now);
            WriteBookViewedRecord(now);
            CloseFile();
        }

        #endregion

        #region Callback Methods

        private void OnViewBookChanged(object sender, BookChangedEventArgs e)
        {
            var now = DateTime.Now;
            var book = BookHub.Current.Book;
            if (book == null)
            {
                return;
            }

            AppDispatcher.Invoke(() =>
            {
                WriteBookViewedRecord(now);

                _viewedBookDateTime = now;
                _viewedBookAddress = book.Address;
                if (book?.NotFoundStartPage != null && book.Pages.Count > 0)
                {
                    _viewedBookName = string.Format(Resources.NotifyCannotOpen, LoosePath.GetFileName(book.NotFoundStartPage));
                }
                else
                {
                    _viewedBookName = LoosePath.GetFileName(book.Address);
                }

            });
        }

        private void OnViewContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            var now = DateTime.Now;
            var viewedPages = e?.ViewPageCollection?.Collection.Where(x => x != null).Select(x => x.Page).ToList() ?? new List<Page>();

            AppDispatcher.Invoke(() =>
            {
                WritePageViewedRecord(now);
                _viewedPagesDateTime = now;
                _viewedPages = viewedPages;
            });
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                CloseFile();

                if (!IsRecordPageView)
                {
                    return;
                }

                if (String.IsNullOrEmpty(PageViewRecordPath))
                {
                    return;
                }

                OpenFile();
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