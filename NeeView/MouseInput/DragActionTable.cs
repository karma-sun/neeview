﻿using NeeView.Windows.Property;
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
        static DragActionTable() => Current = new DragActionTable();
        public static DragActionTable Current { get; }

        private static Memento s_defaultMemento;
        
        // 初期設定取得
        public static Memento CreateDefaultMemento()
        {
            return s_defaultMemento.Clone();
        }


        // コマンドリスト
        private Dictionary<DragActionType, DragAction> _elements;

        // コマンドターゲット
        private DragTransformControl _drag;


        // コンストラクタ
        private DragActionTable()
        {
            _elements = new Dictionary<DragActionType, DragAction>()
            {
                [DragActionType.Gesture] = new DragAction
                {
                    IsLocked = true,
                    DragKey = new DragKey("RightButton"),
                },

                [DragActionType.Move] = new DragAction
                {
                    DragKey = new DragKey("LeftButton"),
                    Exec = (s, e) => _drag.DragMove(s, e),
                    Group = DragActionGroup.Move,
                },
                [DragActionType.MoveScale] = new DragAction
                {
                    Exec = (s, e) => _drag.DragMoveScale(s, e),
                    Group = DragActionGroup.Move,
                },
                [DragActionType.Angle] = new DragAction
                {
                    DragKey = new DragKey("Shift+LeftButton"),
                    Exec = (s, e) => _drag.DragAngle(s, e),
                },
                [DragActionType.Scale] = new DragAction
                {
                    Exec = (s, e) => _drag.DragScale(s, e),
                },
                [DragActionType.ScaleSlider] = new DragAction
                {
                    DragKey = new DragKey("Ctrl+LeftButton"),
                    Exec = (s, e) => _drag.DragScaleSlider(s, e),
                },
                [DragActionType.FlipHorizontal] = new DragAction
                {
                    DragKey = new DragKey("Alt+LeftButton"),
                    Exec = (s, e) => _drag.DragFlipHorizontal(s, e),
                },
                [DragActionType.FlipVertical] = new DragAction
                {
                    Exec = (s, e) => _drag.DragFlipVertical(s, e),
                },

                [DragActionType.WindowMove] = new DragAction
                {
                    DragKey = new DragKey("RightButton+LeftButton"),
                    Exec = (s, e) => _drag.DragWindowMove(s, e),
                },

            };

            s_defaultMemento = CreateMemento();
        }


        // コマンドリスト
        [PropertyMember("@ParamDragActionElements")]
        public Dictionary<DragActionType, DragAction> Elements => _elements;


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

        // コマンドターゲット設定
        public void SetTarget(DragTransformControl drag)
        {
            _drag = drag;
        }

        /// <summary>
        /// 入力からアクション取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DragActionType GetActionType(DragKey key)
        {
            if (!key.IsValid) return DragActionType.None;
            return _elements.FirstOrDefault(e => e.Value.DragKey == key).Key;
        }

        /// <summary>
        /// 入力からアクション取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DragAction GetAction(DragKey key)
        {
            return _elements.Values.FirstOrDefault(e => e.DragKey == key);
        }


        #region Memento

        // 
        [DataContract]
        public class Memento
        {
            [DataMember]
            public Dictionary<DragActionType, DragAction.Memento> Elements { get; set; } = new Dictionary<DragActionType, DragAction.Memento>();

            public DragAction.Memento this[DragActionType type]
            {
                get { return Elements[type]; }
                set { Elements[type] = value; }
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
            if (memento == null) return;

            foreach (var pair in memento.Elements)
            {
                if (_elements.ContainsKey(pair.Key))
                {
                    _elements[pair.Key].Restore(pair.Value);
                }
            }
        }

#endregion
    }
}