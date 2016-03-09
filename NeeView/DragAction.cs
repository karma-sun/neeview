// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
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
        Move,
        MoveScale,
        Angle,
        Scale,
        ScaleSlider,
        FlipHorizontal,
        FlipVertical,
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
        public string Name;
        public Action<Point, Point> Exec;
        public DragActionGroup Group;

        // グループ判定
        public bool IsGroupCompatible(DragAction target)
        {
            return Group != DragActionGroup.None && Group == target.Group;
        }
    }



    /// <summary>
    /// キーバインド保存用
    /// </summary>
    [DataContract]
    public class MouseDragControllerSetting
    {
        [DataMember]
        public Dictionary<ModifierKeys, DragActionType> KeyBindings { get; set; }

        private void Constructor()
        {
            KeyBindings = new Dictionary<ModifierKeys, DragActionType>
            {
                [ModifierKeys.None] = DragActionType.Move,
                [ModifierKeys.Shift] = DragActionType.Angle,
                [ModifierKeys.Control] = DragActionType.Scale,
                [ModifierKeys.Alt] = DragActionType.FlipHorizontal,
            };
        }

        public MouseDragControllerSetting()
        {
            Constructor();
        }

        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

        //
        public MouseDragControllerSetting Clone()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(MouseDragControllerSetting));
                serializer.WriteObject(ms, this);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                return (MouseDragControllerSetting)serializer.ReadObject(ms);
            }
        }

        //
        public MouseDragControllerSetting CreateMemento()
        {
            return Clone();
        }

        //
        public void Restore(MouseDragControllerSetting memento)
        {
            if (memento == null) return;

            foreach (var pair in memento.KeyBindings)
            {
                if (KeyBindings.ContainsKey(pair.Key))
                {
                    KeyBindings[pair.Key] = pair.Value;
                }
            }
        }
    }
}
