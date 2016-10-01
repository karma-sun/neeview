using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class GestureToken
    {
        // ジェスチャー文字列（１ジェスチャー）
        public string Gesture { get; set; }

        // 競合しているコマンド群
        public List<CommandType> Conflicts { get; set; }

        // 競合メッセージ
        public string OverlapsText { get; set; }

        public bool IsConflict => Conflicts != null && Conflicts.Count > 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public class InputGestureSettingWindowVM : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        // すべてのコマンドのショートカット
        private Dictionary<CommandType, string> _Sources;

        // 編集するコマンド
        public CommandType Command { get; set; }

        /// <summary>
        /// Property: GestureTokens
        /// ショートカットテキストのリスト
        /// </summary>
        private ObservableCollection<GestureToken> _GestureTokens;
        public ObservableCollection<GestureToken> GestureTokens
        {
            get { return _GestureTokens; }
            set { if (_GestureTokens != value) { _GestureTokens = value; OnPropertyChanged(); } }
        }

        // ウィンドウタイトル？
        public string Header { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public InputGestureSettingWindowVM(Dictionary<CommandType, string> sources, CommandType command)
        {
            _Sources = sources;
            Command = command;
            Header = $"キーの設定 - {Command.ToDispString()}";

            UpdateGestures();
        }

        /// <summary>
        /// ジェスチャーリスト更新
        /// </summary>
        public void UpdateGestures()
        {
            var items = new ObservableCollection<GestureToken>();
            if (_Sources[Command] != null)
            {
                foreach (var gesture in _Sources[Command].Split(','))
                {
                    var element = CreateShortCutElement(gesture);
                    items.Add(element);
                }
            }
            GestureTokens = items;
        }

        /// <summary>
        /// GestureToken 作成
        /// </summary>
        /// <param name="gesture"></param>
        /// <returns></returns>
        public GestureToken CreateShortCutElement(string gesture)
        {
            var element = new GestureToken() { Gesture = gesture };

            var overlaps = _Sources
                .Where(e => !string.IsNullOrEmpty(e.Value) && e.Key != Command && e.Value.Split(',').Contains(gesture))
                .Select(e => e.Key)
                .ToList();

            if (overlaps.Count > 0)
            {
                element.Conflicts = overlaps;
                element.OverlapsText = string.Join("", overlaps.Select(e => $"「{e.ToDispString()}」")) + "と競合しています";
            }

            return element;
        }

        /// <summary>
        /// ジェスチャーの追加
        /// </summary>
        /// <param name="gesture"></param>
        public void AddGesture(string gesture)
        {
            if (string.IsNullOrEmpty(gesture)) return;

            if (!GestureTokens.Any(item => item.Gesture == gesture))
            {
                var element = CreateShortCutElement(gesture);
                GestureTokens.Add(element);
            }
        }

        /// <summary>
        /// ジェスチャーの削除
        /// </summary>
        /// <param name="gesture"></param>
        public void RemoveGesture(string gesture)
        {
            var token = GestureTokens.FirstOrDefault(e => e.Gesture == gesture);
            if (token != null)
            {
                GestureTokens.Remove(token);
            }
        }

        /// <summary>
        /// GestureTokensから元の情報に書き戻し
        /// </summary>
        public void Flush()
        {
            _Sources[Command] = GestureTokens.Count > 0
                ? string.Join(",", GestureTokens.Select(e => e.Gesture))
                : null;
        }


        /// <summary>
        /// 競合の解決
        /// </summary>
        public void ResolveConflict(GestureToken item, System.Windows.Window owner)
        {
            Flush();

            var conflicts = new List<CommandType>(item.Conflicts);
            conflicts.Insert(0, Command);
            var context = new ResolveConflictDialogContext(item.Gesture, conflicts, Command);

            // 競合解消用ダイアログ表示。本来はViewで行うべき
            var dialog = new ResolveConflictDialog(context);
            dialog.Owner = owner;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                foreach (var conflictItem in context.Conflicts)
                {
                    if (!conflictItem.IsChecked)
                    {
                        var newGesture = string.Join(",", this._Sources[conflictItem.Command].Split(',').Where(i => i != item.Gesture));
                        this._Sources[conflictItem.Command] = string.IsNullOrEmpty(newGesture) ? null : newGesture;
                    }
                }
                UpdateGestures();
            }
        }

    }
}
