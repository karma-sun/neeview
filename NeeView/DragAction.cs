// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    // ドラッグアクションの種類
    public enum DragActionType
    {
        None,
        Gesture,
        Move,
        MoveScale,
        Angle,
        Scale,
        ScaleSlider,
        FlipHorizontal,
        FlipVertical,
        WindowMove,
    }

    public static class DragActionTypeExtension
    {
        public static Dictionary<DragActionType, string> LabelList { get; } = new Dictionary<DragActionType, string>
        {
            [DragActionType.None] = "なし",
            [DragActionType.Gesture] = "マウスジェスチャー",
            [DragActionType.Move] = "移動",
            [DragActionType.MoveScale] = "移動(スケール依存)",
            [DragActionType.Angle] = "回転",
            [DragActionType.Scale] = "拡大縮小",
            [DragActionType.ScaleSlider] = "拡大縮小(スライド式)",
            [DragActionType.FlipHorizontal] = "左右反転",
            [DragActionType.FlipVertical] = "上下反転",
            [DragActionType.WindowMove] = "ウィンドウ移動",
        };

        public static string ToLabel(this DragActionType action)
        {
            return LabelList[action];
        }

        public static Dictionary<DragActionType, string> TipsList = new Dictionary<DragActionType, string>()
        {
            [DragActionType.None] = null,
            [DragActionType.Gesture] = "マウス移動の組み合わせでコマンドを実行します",
            [DragActionType.Move] = "ドラッグで画像を移動させます",
            [DragActionType.MoveScale] = "画像の大きさに応じて移動速度を変えます",
            [DragActionType.Angle] = "ドラッグで回転させます",
            [DragActionType.Scale] = "ドラッグで拡縮。中心を基準に拡大率を変化させます",
            [DragActionType.ScaleSlider] = "左右ドラッグで拡大率を変化させます",
            [DragActionType.FlipHorizontal] = "左右ドラッグで左右反転させます",
            [DragActionType.FlipVertical] = "上下ドラッグで上下反転させます",
            [DragActionType.WindowMove] = "ドラッグでウィンドウを移動させます",
        };

        public static string ToTips(this DragActionType action)
        {
            return TipsList[action];
        }
    }

    // ドラッグアクショングループ
    public enum DragActionGroup
    {
        None, // どのグループにも属さない
        Move,
    };


    // ドラッグアクション
    public class DragAction
    {
        /// <summary>
        /// IsLocked property.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Name property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DragKey property.
        /// </summary>
        public DragKey DragKey { get; set; } = new DragKey();

        /// <summary>
        /// Exec property.
        /// </summary>
        public Action<Point, Point> Exec { get; set; }

        /// <summary>
        /// DragActionGroup property.
        /// </summary>
        public DragActionGroup Group { get; set; }

        // グループ判定
        public bool IsGroupCompatible(DragAction target)
        {
            return Group != DragActionGroup.None && Group == target.Group;
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public string Key { get; set; }

            //
            private void Constructor()
            {
                Key = "";
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
            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
                if (Key != null)
                {
                    Key = Key.Replace("Drag", "");
                }
            }

            //
            public Memento Clone()
            {
                return (Memento)MemberwiseClone();
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Key = DragKey.ToString();
            return memento;
        }

        //
        public void Restore(Memento element)
        {
            DragKey = new DragKey(element.Key);
        }

        #endregion
    }


    /// <summary>
    /// ドラッグキー
    /// </summary>
    public class DragKey : IEquatable<DragKey>
    {
        public MouseButtonBits MouseButtonBits;
        public ModifierKeys ModifierKeys;


        /// <summary>
        /// コンストラクター
        /// </summary>
        public DragKey()
        {
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="modifiers"></param>
        public DragKey(MouseButtonBits bits, ModifierKeys modifiers)
        {
            MouseButtonBits = bits;
            ModifierKeys = modifiers;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="gesture"></param>
        public DragKey(string gesture)
        {
            if (string.IsNullOrWhiteSpace(gesture)) return;

            try
            {
                var key = DragKeyConverter.ConvertFromString(gesture);
                MouseButtonBits = key.MouseButtonBits;
                ModifierKeys = key.ModifierKeys;
            }
            catch (Exception)
            { }
        }

        #region IEquatable

        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            DragKey p = obj as DragKey;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (MouseButtonBits == p.MouseButtonBits) && (ModifierKeys == p.ModifierKeys);
        }

        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Equals(DragKey p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (MouseButtonBits == p.MouseButtonBits) && (ModifierKeys == p.ModifierKeys);
        }

        /// <summary>
        /// ハッシュ値
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return MouseButtonBits.GetHashCode() ^ ModifierKeys.GetHashCode();
        }

        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(DragKey a, DragKey b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return (a.MouseButtonBits == b.MouseButtonBits) && (a.ModifierKeys == b.ModifierKeys);
        }

        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(DragKey a, DragKey b)
        {
            return !(a == b);
        }

        #endregion

        public bool IsValid => MouseButtonBits != MouseButtonBits.None;

        public override string ToString()
        {
            return DragKeyConverter.ConvertToString(this);
        }
    }


    /// <summary>
    /// マウスドラッグ コンバータ
    /// </summary>
    public class DragKeyConverter
    {
        /// <summary>
        ///  文字列からマウスドラッグアクションに変換する
        /// </summary>
        /// <param name="source">ジェスチャ文字列</param>
        /// <returns>DragKey。変換に失敗したときは NotSupportedException 例外が発生</returns>
        public static DragKey ConvertFromString(string source)
        {
            // ex. LeftDrag
            // ex. Ctrl+XButton1+LeftDrag

            // １操作のみサポート
            source = source.Split(',').First();

            // Drag削除
            source = source.Replace("Drag", "");

            var keys = source.Split('+');

            ModifierKeys modifierKeys = ModifierKeys.None;
            MouseButtonBits mouseButtonBits = MouseButtonBits.None;

            foreach (var key in keys)
            {
                if (key == "Ctrl")
                {
                    modifierKeys |= ModifierKeys.Control;
                    continue;
                }

                if (Enum.TryParse<ModifierKeys>(key, out ModifierKeys modifierKeysOne))
                {
                    modifierKeys |= modifierKeysOne;
                    continue;
                }

                if (Enum.TryParse<MouseButtonBits>(key, out MouseButtonBits bit))
                {
                    mouseButtonBits |= bit;
                    continue;
                }

                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、DragKey ではサポートされていません。");
            }

            //
            if (mouseButtonBits == MouseButtonBits.None)
            {
                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、DragKey ではサポートされていません。");
            }

            return new DragKey(mouseButtonBits, modifierKeys);
        }

        /// <summary>
        ///  マウスドラッグアクションから文字列に変換する
        /// </summary>
        public static string ConvertToString(DragKey gesture)
        {
            if (!gesture.IsValid) return "";

            string text = "";

            foreach (ModifierKeys key in Enum.GetValues(typeof(ModifierKeys)))
            {
                if ((gesture.ModifierKeys & key) != ModifierKeys.None)
                {
                    text += "+" + ((key == ModifierKeys.Control) ? "Ctrl" : key.ToString());
                }
            }

            text += "+" + string.Join("+", gesture.MouseButtonBits.ToString().Split(',').Select(e => e.Trim()).Reverse());

            return text.TrimStart('+');
        }
    }
}
