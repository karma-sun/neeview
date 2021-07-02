using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// RenameManager.xaml の相互作用ロジック
    /// </summary>
    public partial class RenameManager : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        private bool _isRenaming;


        public RenameManager()
        {
            InitializeComponent();
        }


        public bool IsRenaming
        {
            get { return _isRenaming; }
            set { SetProperty(ref _isRenaming, value); }
        }

        public UIElement RenameElement
        {
            get
            {
                if (this.Root.Children != null && this.Root.Children.Count > 0)
                {
                    var renameControl = (RenameControl)this.Root.Children[0];
                    return renameControl.Target;
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// コントロールの所属するウィンドウの RenameManager を取得する
        /// </summary>
        /// <param name="obj">コントロール</param>
        /// <returns>RenameManager</returns>
        public static RenameManager GetRenameManager(DependencyObject obj)
        {
            RenameManager renameMabager = null;

            var window = Window.GetWindow(obj);
            if (window is IHasRenameManager hasRenameManager)
            {
                renameMabager = hasRenameManager.GetRenameManager();
            }

            Debug.Assert(renameMabager != null);
            return renameMabager;
        }


        public void Open(RenameControl rename)
        {
            rename.Close += Rename_Close;

            var pos = rename.Target.TranslatePoint(new Point(0, 0), this) - new Vector(3, 2);
            Canvas.SetLeft(rename, pos.X);
            Canvas.SetTop(rename, pos.Y);

            rename.MaxWidth = this.ActualWidth - pos.X - 8;

            this.Root.Children.Add(rename);

            rename.Target.Visibility = Visibility.Hidden;

            IsRenaming = true;
        }

        private void Rename_Close(object sender, EventArgs e)
        {
            var rename = (RenameControl)sender;
            rename.Target.Visibility = Visibility.Visible;

            // NOTE: ウィンドウのディアクティブタイミングで閉じたときに再度アクティブ化するのを防ぐためにタイミングをずらす。動作原理不明。
            AppDispatcher.BeginInvoke(() => this.Root.Children.Remove(sender as RenameControl));

            IsRenaming = this.Root.Children != null && this.Root.Children.Count > 0;
        }

        public void Stop()
        {
            if (this.Root.Children != null && this.Root.Children.Count > 0)
            {
                var renames = this.Root.Children.OfType<RenameControl>().ToList();
                foreach (var rename in renames)
                {
                    rename.Stop(true);
                }
            }
        }
    }

    public interface IHasRenameManager
    {
        RenameManager GetRenameManager();
    }
}
