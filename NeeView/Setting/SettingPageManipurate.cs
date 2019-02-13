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
        public SettingPageManipurate() : base(Properties.Resources.SettingPageManipurate)
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
        public SettingPageManipurateGeneral() : base(Properties.Resources.SettingPageManipurateGeneral)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageManipurateGeneralViewOperation,
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransform.Current, nameof(DragTransform.IsLimitMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsViewStartPositionCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsControlCenterImage))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsKeepScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsKeepAngle))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsKeepFlip)))),

                new SettingItemSection(Properties.Resources.SettingPageManipurateGeneralAdvance,
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
            public override string ValueString => Value == 0 ? Properties.Resources.WordStepless : $"{Value} {Properties.Resources.WordDegree}";
        }
    }

    public class SettingPageManipurateMouse : SettingPage
    {
        public SettingPageManipurateMouse() : base(Properties.Resources.SettingPageManipurateMouse)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageManipurateMouseDrag,
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.IsDragEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragActionTable.Current, nameof(DragActionTable.Elements)),
                        new SettingMouseDragControl())
                    {
                        IsStretch = true,
                        IsEnabled = new IsEnabledPropertyValue(MouseInput.Current.Normal, nameof(MouseInputNormal.IsDragEnabled))
                    }),

                new SettingItemSection(Properties.Resources.SettingPageManipurateMouseGesture,
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.IsGestureEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Gesture, nameof(MouseInputGesture.GestureMinimumDistance)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(MouseInput.Current.Normal, nameof(MouseInputNormal.IsGestureEnabled))
                    }),

                new SettingItemSection(Properties.Resources.SettingPageManipurateMouseDragCommon,
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.MinimumDragDistance)))),

                new SettingItemSection(Properties.Resources.SettingPageManipurateMouseHold,
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.LongButtonDownMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.LongButtonMask))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.LongButtonDownTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.LongButtonRepeatTime)))),

                new SettingItemSection(Properties.Resources.SettingPageManipurateMouseVisibility, Properties.Resources.SettingPageManipurateMouseVisibilityTips,
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsCursorHideEnabled))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.CursorHideTime))),
                        new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.CursorHideReleaseDistance))),
                        new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsCursorHideReleaseAction))))
                    {
                        IsEnabled = new IsEnabledPropertyValue(MainWindowModel.Current, nameof(MainWindowModel.IsCursorHideEnabled))
                    }),
            };
        }
    }

    public class SettingPageManipurateTouch : SettingPage
    {
        public SettingPageManipurateTouch() : base(Properties.Resources.SettingPageManipurateTouch)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageManipurateTouchGeneral,
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current, nameof(TouchInput.IsEnabled))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Normal, nameof(TouchInputNormal.DragAction))),
                        new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Normal, nameof(TouchInputNormal.HoldAction))),
                        new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Drag.Manipulation, nameof(TouchDragManipulation.IsAngleEnabled))),
                        new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Drag.Manipulation, nameof(TouchDragManipulation.IsScaleEnabled))))
                    {
                        IsEnabled = new IsEnabledPropertyValue(TouchInput.Current, nameof(TouchInput.IsEnabled)),
                    }),

                new SettingItemSection(Properties.Resources.SettingPageManipurateTouchAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Gesture, nameof(TouchInputGesture.GestureMinimumDistance))),
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Drag.Manipulation, nameof(TouchDragManipulation.MinimumManipulationRadius))),
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Drag.Manipulation, nameof(TouchDragManipulation.MinimumManipulationDistance))))
                {
                    IsEnabled = new IsEnabledPropertyValue(TouchInput.Current, nameof(TouchInput.IsEnabled)),
                }
            };
        }
    }

    public class SettingPageManipurateLoupe : SettingPage
    {
        public SettingPageManipurateLoupe() : base(Properties.Resources.SettingPageManipurateLoupe)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageManipurateLoupeGeneral,
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.IsResetByRestart))),
                    new SettingItemProperty(PropertyMemberElement.Create(LoupeTransform.Current, nameof(LoupeTransform.IsVisibleLoupeInfo))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.IsResetByPageChanged))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.IsWheelScalingEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.IsEscapeKeyEnabled)))),

                new SettingItemSection(Properties.Resources.SettingPageManipurateLoupeAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.DefaultScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.MinimumScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.MaximumScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.ScaleStep))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.Speed)))),

            };
        }
    }
}
