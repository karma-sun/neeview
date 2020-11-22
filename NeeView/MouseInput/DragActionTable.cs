using NeeView.Windows.Property;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    public class DragActionTable : IEnumerable<KeyValuePair<string, DragAction>>
    {
        static DragActionTable() => Current = new DragActionTable();
        public static DragActionTable Current { get; }

        public const string GestureDragActionName = "Gesture";
        private DragActionCollection _defaultMemento;

        private Dictionary<string, DragAction> _elements;
        private DragTransformControl _drag;


        private DragActionTable()
        {
            _elements = new Dictionary<string, DragAction>()
            {
                [GestureDragActionName] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeGesture,
                    IsLocked = true,
                    DragKey = new DragKey("RightButton"),
                },

                ["Move"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeMove,
                    DragKey = new DragKey("LeftButton"),
                    Exec = (s, e) => _drag.DragMove(s, e),
                    Group = DragActionGroup.Move,
                },
                ["MoveScale"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeMoveScale,
                    Exec = (s, e) => _drag.DragMoveScale(s, e),
                    Group = DragActionGroup.Move,
                },
                ["Angle"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeAngle,
                    DragKey = new DragKey("Shift+LeftButton"),
                    Exec = (s, e) => _drag.DragAngle(s, e),
                },
                ["AngleSlider"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeAngleSlider,
                    Exec = (s, e) => _drag.DragAngleSlider(s, e),
                },
                ["Scale"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeScale,
                    Exec = (s, e) => _drag.DragScale(s, e),
                },
                ["ScaleSlider"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeScaleSlider,
                    DragKey = new DragKey("Ctrl+LeftButton"),
                    Exec = (s, e) => _drag.DragScaleSlider(s, e),
                },
                ["ScaleSliderCentered"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeScaleSliderCentered,
                    Exec = (s, e) => _drag.DragScaleSliderCentered(s, e),
                },
                ["MarqueeZoom"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeMarqueeZoom,
                    Exec = (s, e) => _drag.DragMarqueeZoom(s, e),
                    ExecEnd = (s, e) => _drag.DragMarqueeZoomEnd(s, e),
                },
                ["FlipHorizontal"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeFlipHorizontal,
                    DragKey = new DragKey("Alt+LeftButton"),
                    Exec = (s, e) => _drag.DragFlipHorizontal(s, e),
                },
                ["FlipVertical"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeFlipVertical,
                    Exec = (s, e) => _drag.DragFlipVertical(s, e),
                },

                ["WindowMove"] = new DragAction
                {
                    Note = Properties.Resources.EnumDragActionTypeWindowMove,
                    DragKey = new DragKey("RightButton+LeftButton"),
                    Exec = (s, e) => _drag.DragWindowMove(s, e),
                },

            };

            _defaultMemento = CreateDragActionCollection();

            Config.Current.Mouse.AddPropertyChanged(nameof(MouseConfig.IsGestureEnabled),
                (s, e) => UpdateGestureDragAction());

            UpdateGestureDragAction();
        }


        public event EventHandler GestureDragActionChanged;


        public DragAction this[string key]
        {
            get
            {
                if (!_elements.ContainsKey(key)) throw new ArgumentOutOfRangeException(key.ToString());
                return _elements[key];
            }
            set { _elements[key] = value; }
        }

        // コマンドリスト
        [PropertyMember("@ParamDragActionElements")]
        public Dictionary<string, DragAction> Elements => _elements;


        public IEnumerator<KeyValuePair<string, DragAction>> GetEnumerator()
        {
            foreach (var pair in _elements)
            {
                yield return pair;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        public DragActionCollection CreateDefaultMemento()
        {
            return (DragActionCollection)_defaultMemento.Clone();
        }

        public void UpdateGestureDragAction()
        {
            _elements[GestureDragActionName].DragKey = Config.Current.Mouse.IsGestureEnabled ? new DragKey("RightButton") : new DragKey();
            GestureDragActionChanged?.Invoke(this, null);
        }

        // コマンドターゲット設定
        public void SetTarget(DragTransformControl drag)
        {
            _drag = drag;
        }

        /// <summary>
        /// 入力からアクション取得
        /// </summary>
        public string GetActionType(DragKey key)
        {
            if (!key.IsValid) return "";
            return _elements.FirstOrDefault(e => e.Value.DragKey == key).Key;
        }

        /// <summary>
        /// 入力からアクション取得
        /// </summary>
        public DragAction GetAction(DragKey key)
        {
            return _elements.Values.FirstOrDefault(e => e.DragKey == key);
        }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
#pragma warning disable CS0612
            [DataMember(Name = "Elements", EmitDefaultValue = false)]
            public Dictionary<DragActionType, DragAction.Memento> Elements { get; set; } = new Dictionary<DragActionType, DragAction.Memento>();
#pragma warning restore CS0612

            public void RestoreConfig(Config config)
            {
            }

            public DragActionCollection CreateDragActionCollectioin()
            {
                if (Elements == null) return null;

                var collection = new DragActionCollection();
                foreach (var element in Elements)
                {
                    collection.Add(element.Key.ToString(), element.Value);
                }
                return collection;
            }
        }

        #endregion


        public DragActionCollection CreateDragActionCollection()
        {
            var collection = new DragActionCollection();

            foreach (var pair in _elements)
            {
                collection.Add(pair.Key.ToString(), pair.Value.CreateMemento());
            }

            return collection;
        }

        public void RestoreDragActionCollection(DragActionCollection collection)
        {
            if (collection == null) return;

            foreach (var pair in collection)
            {
                if (_elements.ContainsKey(pair.Key))
                {
                    _elements[pair.Key].Restore(pair.Value);
                }
            }
        }
    }


    public class DragActionCollection : Dictionary<string, DragAction.Memento>, ICloneable
    {
        public object Clone()
        {
            var clone = new DragActionCollection();
            foreach (var pair in this)
            {
                clone.Add(pair.Key, pair.Value.Clone());
            }
            return clone;
        }
    }

}
