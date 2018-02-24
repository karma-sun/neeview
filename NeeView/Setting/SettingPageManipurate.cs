using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    public class SettingPageManipurate : SettingPage
    {
        public SettingPageManipurate() : base("画像操作")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageManipurateGeneral(),
                new SettingPageManipurateMouse(),
                new SettingPageManipurateTouch(),
                new SettingPageManipurateLoupe(),
            };
        }
    }

    public class SettingPageManipurateGeneral : SettingPage
    {
        public SettingPageManipurateGeneral() : base("画像操作全般")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("ビュー操作",
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransform.Current, nameof(DragTransform.IsLimitMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsViewStartPositionCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsControlCenterImage))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsKeepScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsKeepAngle))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsKeepFlip)))),

                new SettingItemSection("詳細設定",
                    new SettingItemIndexValue<double>(PropertyMemberElement.Create(DragTransform.Current, nameof(DragTransform.AngleFrequency)), new AngleFrequency(), false)),
            };
        }

        /// <summary>
        /// ビュー回転スナップ値
        /// </summary>
        public class AngleFrequency : IndexDoubleValue
        {
            private static List<double> _values = new List<double>
        {
            0, 5, 10, 15, 20, 30, 45, 60, 90
        };

            //
            public AngleFrequency() : base(_values)
            {
            }

            //
            public AngleFrequency(double value) : base(_values)
            {
                Value = value;
            }


            //
            public override string ValueString => Value == 0 ? "無段階" : $"{Value}度";
        }
    }

    public class SettingPageManipurateMouse : SettingPage
    {
        public SettingPageManipurateMouse() : base("マウス操作")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("マウスドラッグ操作",
                    new SettingItemMouseDrag()),

                new SettingItemSection("マウス長押し操作",
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.LongButtonDownMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.LongButtonMask))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.LongButtonDownTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.LongButtonRepeatTime)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.IsGestureEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Gesture, nameof(MouseInputGesture.GestureMinimumDistance)))),
            };
        }
    }

    public class SettingPageManipurateTouch : SettingPage
    {
        public SettingPageManipurateTouch() : base("タッチ操作")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("機能",
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current, nameof(TouchInput.IsEnabled)))),

                new SettingItemGroup(

                new SettingItemSection("全般",
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Normal, nameof(TouchInputNormal.DragAction))),
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Normal, nameof(TouchInputNormal.HoldAction))),
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Drag.Manipulation, nameof(TouchDragManipulation.IsAngleEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Drag.Manipulation, nameof(TouchDragManipulation.IsScaleEnabled)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Gesture, nameof(TouchInputGesture.GestureMinimumDistance))),
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Drag.Manipulation, nameof(TouchDragManipulation.MinimumManipulationRadius))),
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Drag.Manipulation, nameof(TouchDragManipulation.MinimumManipulationDistance))))
                
                )
                {
                    IsEnabled = new IsEnabledPropertyValue(TouchInput.Current, nameof(TouchInput.IsEnabled)),
                }
            };
        }
    }

    public class SettingPageManipurateLoupe : SettingPage
    {
        public SettingPageManipurateLoupe() : base("ルーペ")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("全般",
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.IsResetByRestart))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.IsResetByPageChanged))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.IsWheelScalingEnabled)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.DefaultScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.MinimumScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.MaximumScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.ScaleStep))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.Speed)))),

            };
        }
    }
}
