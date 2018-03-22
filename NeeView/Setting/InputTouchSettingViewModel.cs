using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace NeeView.Setting
{

    /// <summary>
    /// MouseGestureSetting ViewModel
    /// </summary>
    public class InputTouchSettingViewModel : BindableBase
    {
        private Dictionary<CommandType, CommandElement.Memento> _sources;
        private CommandType _key;

        /// <summary>
        /// GestureElements property.
        /// </summary>
        public ObservableCollection<GestureElement> GestureToken
        {
            get { return _gestureToken; }
            set { if (_gestureToken != value) { _gestureToken = value; RaisePropertyChanged(); } }
        }

        private ObservableCollection<GestureElement> _gestureToken = new ObservableCollection<GestureElement>();

        //
        /// <summary>
        /// GestoreTokenNote property.
        /// </summary>
        public string GestureTokenNote
        {
            get { return _gestureTokenNote; }
            set { if (_gestureTokenNote != value) { _gestureTokenNote = value; RaisePropertyChanged(); } }
        }

        private string _gestureTokenNote;


        //
        public TouchAreaMap TouchAreaMap { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="gestureSender"></param>
        public InputTouchSettingViewModel(CommandTable.Memento memento, CommandType key, FrameworkElement gestureSender)
        {
            _sources = memento.Elements;
            _key = key;

            this.TouchAreaMap = new TouchAreaMap(_sources[_key].TouchGesture);
            UpdateGestureToken(this.TouchAreaMap);
        }

        //
        internal void SetTouchGesture(Point pos, double width, double height)
        {
            var gesture = TouchGestureExtensions.GetTouchGesture(pos.X / width, pos.Y / height);

            this.TouchAreaMap.Toggle(gesture);
            RaisePropertyChanged(nameof(TouchAreaMap));

            UpdateGestureToken(this.TouchAreaMap);
        }


        /// <summary>
        /// Update Gesture Information
        /// </summary>
        /// <param name="map"></param>
        public void UpdateGestureToken(TouchAreaMap map)
        {
            string gestures = map.ToString();
            this.GestureTokenNote = null;

            if (!string.IsNullOrEmpty(gestures))
            {
                var shortcuts = new ObservableCollection<GestureElement>();
                foreach (var key in gestures.Split(','))
                {
                    var overlaps = _sources
                        .Where(i => i.Key != _key && i.Value.TouchGesture.Split(',').Contains(key))
                        .Select(e => e.Key.ToDispLongString())
                        .ToList();

                    if (overlaps.Count > 0)
                    {
                        if (this.GestureTokenNote != null) this.GestureTokenNote += "\n";
                        this.GestureTokenNote += string.Format(Properties.Resources.NotifyConflictWith, key, ResourceService.Join(overlaps));
                    }

                    var element = new GestureElement();
                    element.Gesture = key;
                    element.IsConflict = overlaps.Count > 0;
                    element.Splitter = ",";

                    shortcuts.Add(element);
                }

                if (shortcuts.Count > 0)
                {
                    shortcuts.Last().Splitter = null;
                }

                this.GestureToken = shortcuts;
            }
            else
            {
                this.GestureToken = new ObservableCollection<GestureElement>();
            }
        }


        /// <summary>
        /// 決定
        /// </summary>
        public void Flush()
        {
            _sources[_key].TouchGesture = this.TouchAreaMap.ToString();
        }
    }


    /// <summary>
    /// タッチエリア管理用
    /// </summary>
    public class TouchAreaMap
    {
        //
        private Dictionary<TouchGesture, bool> _map;

        //
        public TouchAreaMap(string gestureString)
        {
            _map = Enum.GetValues(typeof(TouchGesture)).Cast<TouchGesture>().ToDictionary(e => e, e => false);

            if (gestureString != null)
            {
                foreach (var token in gestureString.Split(','))
                {
                    if (Enum.TryParse(token, out TouchGesture key))
                    {
                        _map[key] = true;
                    }
                }
            }
        }

        //
        public bool this[TouchGesture gesture]
        {
            get { return _map[gesture]; }
            set { _map[gesture] = value; }
        }

        //
        public void Toggle(TouchGesture gesture)
        {
            _map[gesture] = !_map[gesture];
        }

        //
        public void Clear()
        {
            foreach (var key in _map.Keys)
            {
                _map[key] = false;
            }
        }

        //
        public override string ToString()
        {
            return string.Join(",", _map.Where(e => e.Key != TouchGesture.None && e.Value == true).Select(e => e.Key.ToString()));
        }
    }

}
