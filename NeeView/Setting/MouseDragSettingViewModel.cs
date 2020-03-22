using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView.Setting
{
    /// <summary>
    /// MouseDragSetting ViewModel
    /// </summary>
    public class MouseDragSettingViewModel : BindableBase
    {
        private DragActionCollection _sources;
        private string _key;

        /// <summary>
        /// Property: DragToken
        /// </summary>
        private DragToken _dragToken = new DragToken();
        public DragToken DragToken
        {
            get { return _dragToken; }
            set { if (_dragToken != value) { _dragToken = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Property: Original Drag
        /// </summary>
        public string OriginalDrag { get; set; }

        /// <summary>
        /// NewGesture property.
        /// </summary>
        private string _NewDrag;
        public string NewDrag
        {
            get { return _NewDrag; }
            set { if (_NewDrag != value) { _NewDrag = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public MouseDragSettingViewModel(DragActionCollection memento, string key, FrameworkElement gestureSender)
        {
            _sources = memento;
            _key = key;

            gestureSender.MouseDown += GestureSender_MouseDown;

            OriginalDrag = NewDrag = _sources[_key].MouseButton; // _context.Gesture;
            UpdateGestureToken(NewDrag);
        }

        private void GestureSender_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var dragKey = new DragKey(MouseButtonBitsExtensions.Create(e), Keyboard.Modifiers);

            UpdateGestureToken(dragKey.ToString());
        }


        /// <summary>
        /// Update Gesture Information
        /// </summary>
        /// <param name="gesture"></param>
        public void UpdateGestureToken(string gesture)
        {
            NewDrag = gesture;

            // Check Conflict
            var token = new DragToken();
            token.Gesture = gesture;

            if (!string.IsNullOrEmpty(token.Gesture))
            {
                token.Conflicts = _sources 
                    .Where(i => i.Key != _key && i.Value.MouseButton == token.Gesture)
                    .Select(i => i.Key)
                    .ToList();

                if (token.Conflicts.Count > 0)
                {
                    token.OverlapsText = string.Format(Properties.Resources.NotifyConflict, ResourceService.Join(token.Conflicts.Select(i => DragActionTable.Current.Elements[i].Note)));
                }
            }

            DragToken = token;
        }


        /// <summary>
        /// 決定
        /// </summary>
        public void Decide()
        {
            _sources[_key].MouseButton = NewDrag;
        }


        /// <summary>
        /// Command: ClearCommand
        /// </summary>
        private RelayCommand _clearCommand;
        private DragActionCollection memento;
        private string key;
        private Grid gestureBox;

        public RelayCommand ClearCommand
        {
            get { return _clearCommand = _clearCommand ?? new RelayCommand(ClearCommand_Executed); }
        }

        private void ClearCommand_Executed()
        {
            UpdateGestureToken(null);
        }
    }
}
