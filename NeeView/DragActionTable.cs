// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    public class DragActionTable : IEnumerable<KeyValuePair<DragActionType, DragAction>>
    {
        // インテグザ
        public DragAction this[DragActionType key]
        {
            get
            {
                if (!_elements.ContainsKey(key)) throw new ArgumentOutOfRangeException(key.ToString());
                return _elements[key];
            }
            set { _elements[key] = value; }
        }

        // Enumerator
        public IEnumerator<KeyValuePair<DragActionType, DragAction>> GetEnumerator()
        {
            foreach (var pair in _elements)
            {
                yield return pair;
            }
        }

        // Enumerator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        // コマンドリスト
        private Dictionary<DragActionType, DragAction> _elements;

        // コマンドターゲット
        private MouseDragController _drag;

        // 初期設定
        private static Memento s_defaultMemento;

        // 初期設定取得
        public static Memento CreateDefaultMemento()
        {
            return s_defaultMemento.Clone();
        }

        // コマンドターゲット設定
        public void SetTarget(MouseDragController drag)
        {
            _drag = drag;
        }

        // コンストラクタ
        public DragActionTable()
        {
            _elements = new Dictionary<DragActionType, DragAction>()
            {
                [DragActionType.Move] = new DragAction
                {
                    Name = "移動",
                    Key = "LeftDrag",
                    Exec = (s, e) => _drag.DragMove(s, e),
                    Group = DragActionGroup.Move,
                },
                [DragActionType.MoveScale] = new DragAction
                {
                    Name = "移動(スケール依存)",
                    Exec = (s, e) => _drag.DragMoveScale(s, e),
                    Group = DragActionGroup.Move,
                },
                [DragActionType.Angle] = new DragAction
                {
                    Name = "回転",
                    Key = "Shift+LeftDrag",
                    Exec = (s, e) => _drag.DragAngle(s, e),
                },
                [DragActionType.Scale] = new DragAction
                {
                    Name = "拡大縮小",
                    Exec = (s, e) => _drag.DragScale(s, e),
                },
                [DragActionType.ScaleSlider] = new DragAction
                {
                    Name = "拡大縮小(スライド式)",
                    Key = "Ctrl+LeftDrag",
                    Exec = (s, e) => _drag.DragScaleSlider(s, e),
                },
                [DragActionType.FlipHorizontal] = new DragAction
                {
                    Name = "左右反転",
                    Key = "Alt+LeftDrag",
                    Exec = (s, e) => _drag.DragFlipHorizontal(s, e),
                },
                [DragActionType.FlipVertical] = new DragAction
                {
                    Name = "上下反転",
                    Exec = (s, e) => _drag.DragFlipVertical(s, e),
                },

                [DragActionType.WindowMove] = new DragAction
                {
                    Name = "ウィンドウ移動",
                    Key = "MiddleDrag",
                    Exec = (s, e) => _drag.DragWindowMove(s, e),
                },
            };

            s_defaultMemento = CreateMemento();
        }

        //
        public Dictionary<DragKey, DragAction> GetKeyBinding()
        {
            var binding = new Dictionary<DragKey, DragAction>();
            var keyConverter = new DragKeyConverter();
            foreach (var e in this)
            {
                var keys = e.Value.GetDragKeyCollection();
                foreach (var key in keys)
                {
                    binding.Add(key, e.Value);
                }
            }

            return binding;
        }


        #region Memento

        // 
        [DataContract]
        public class Memento
        {
            [DataMember]
            public Dictionary<DragActionType, DragAction.Memento> Elements { get; set; }

            public DragAction.Memento this[DragActionType type]
            {
                get { return Elements[type]; }
                set { Elements[type] = value; }
            }

            //
            private void Constructor()
            {
                Elements = new Dictionary<DragActionType, DragAction.Memento>();
            }

            //
            public Memento()
            {
                Constructor();
            }

            //
            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            //
            public DragActionType GetAcionFromKey(string key)
            {
                foreach (var pair in Elements)
                {
                    var keys = pair.Value.Key?.Split(',');
                    if (keys != null && keys.Contains(key)) return pair.Key;
                }
                return DragActionType.None;
            }

            //
            public Memento Clone()
            {
                var memento = new Memento();
                foreach (var pair in Elements)
                {
                    memento.Elements.Add(pair.Key, pair.Value.Clone());
                }
                return memento;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            foreach (var pair in _elements)
            {
                memento.Elements.Add(pair.Key, pair.Value.CreateMemento());
            }

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            foreach (var pair in memento.Elements)
            {
                if (_elements.ContainsKey(pair.Key))
                {
                    _elements[pair.Key].Restore(pair.Value);
                }
            }
        }

        #endregion

        // 設定用キーテーブル
        public class KeyTable
        {
            public static List<string> KeyList;

            static KeyTable()
            {
                KeyList = new List<string>()
                {
                    "LeftDrag", "Shift+LeftDrag", "Ctrl+LeftDrag", "Alt+LeftDrag",
                    "MiddleDrag", "Shift+MiddleDrag", "Ctrl+MiddleDrag", "Alt+MiddleDrag"
                };
            }

            public Dictionary<string, DragActionType> Elements { get; set; }

            private Memento _memento;

            public KeyTable(Memento memento)
            {
                _memento = memento;

                Elements = new Dictionary<string, DragActionType>();
                foreach (var key in KeyList)
                {
                    Elements[key] = _memento.GetAcionFromKey(key);
                }
            }

            public void UpdateMemento()
            {
                foreach (var e in _memento.Elements)
                {
                    e.Value.Key = null;
                }
                var converter = new DragKeyConverter();
                foreach (var e in Elements)
                {
                    if (e.Value != DragActionType.None)
                    {
                        bool isEmpty = string.IsNullOrEmpty(_memento.Elements[e.Value].Key);
                        _memento.Elements[e.Value].Key += isEmpty ? e.Key : "," + e.Key;
                    }
                }
            }
        }
    }
}
