using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    /// <summary>
    /// 
    /// </summary>
    public class GestureToken
    {
        // ジェスチャー文字列（１ジェスチャー）
        public string Gesture { get; set; }

        // 競合しているコマンド群
        public List<string> Conflicts { get; set; }

        // 競合メッセージ
        public string OverlapsText { get; set; }

        public bool IsConflict => Conflicts != null && Conflicts.Count > 0;

        public bool IsExist => !string.IsNullOrEmpty(this.Gesture);
    }

    /// <summary>
    /// 
    /// </summary>
    public class InputGestureSettingViewModel : BindableBase
    {
        // すべてのコマンドのショートカット
        private IDictionary<string, CommandElement> _commandMap;

        // 編集するコマンド
        public string Command { get; set; }

        /// <summary>
        /// ショートカットテキストのリスト
        /// </summary>
        private ObservableCollection<GestureToken> _gestureTokens;
        public ObservableCollection<GestureToken> GestureTokens
        {
            get { return _gestureTokens; }
            set { if (_gestureTokens != value) { _gestureTokens = value; RaisePropertyChanged(); } }
        }

        // ウィンドウタイトル？
        public string Header { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public InputGestureSettingViewModel(IDictionary<string, CommandElement> commandMap, string command)
        {
            _commandMap = commandMap;
            Command = command;
            Header = $"{CommandTable.Current.GetElement(Command).Text} - {Properties.Resources.ControlEditShortcutTitle}";

            UpdateGestures();
        }

        /// <summary>
        /// ジェスチャーリスト更新
        /// </summary>
        public void UpdateGestures()
        {
            var items = new ObservableCollection<GestureToken>();
            if (!string.IsNullOrEmpty(_commandMap[Command].ShortCutKey))
            {
                foreach (var gesture in _commandMap[Command].ShortCutKey.Split(','))
                {
                    if (gesture == "") continue;
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

            var overlaps = _commandMap
                .Where(e => !string.IsNullOrEmpty(e.Value.ShortCutKey) && e.Key != Command && e.Value.ShortCutKey.Split(',').Contains(gesture))
                .Select(e => e.Key)
                .ToList();

            if (overlaps.Count > 0)
            {
                element.Conflicts = overlaps;
                element.OverlapsText = string.Format(Properties.Resources.NotifyConflict, ResourceService.Join(overlaps.Select(e => CommandTable.Current.GetElement(e).Text)));
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
            _commandMap[Command].ShortCutKey = GestureTokens.Count > 0
                ? string.Join(",", GestureTokens.Select(e => e.Gesture))
                : null;
        }


        /// <summary>
        /// 競合の解決
        /// </summary>
        public void ResolveConflict(GestureToken item, System.Windows.Window owner)
        {
            Flush();

            var conflicts = new List<string>(item.Conflicts);
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
                        var newGesture = string.Join(",", _commandMap[conflictItem.CommandName].ShortCutKey.Split(',').Where(i => i != item.Gesture));
                        _commandMap[conflictItem.CommandName].ShortCutKey = string.IsNullOrEmpty(newGesture) ? null : newGesture;
                    }
                }
                UpdateGestures();
            }
        }
    }
}
