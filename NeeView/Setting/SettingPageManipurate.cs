using NeeView.Data;
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
                new SettingPageManipurateMouse(),
                new SettingPageManipurateTouch(),
                new SettingPageManipurateLoupe(),
            };

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageManipurateGeneralViewOperation,
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransform.Current, nameof(DragTransform.IsLimitMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsViewStartPositionCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.RotateCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.ScaleCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.FlipCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepAngle))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepFlip)))),

                new SettingItemSection(Properties.Resources.SettingPageManipurateGeneralAdvance,
                    new SettingItemIndexValue<double>(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.AngleFrequency)), new AngleFrequency(), false)),
            };
        }

        /// <summary>
        /// ビュー回転スナップ値
        /// </summary>
        public class AngleFrequency : IndexDoubleValue
        {
            private static List<double> _values = new List<double> { 0, 5, 10, 15, 20, 30, 45, 60, 90 };

            public AngleFrequency() : base(_values)
            {
            }

            public AngleFrequency(double value) : base(_values)
            {
                Value = value;
            }

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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.MinimumDragDistance))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsDragEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragActionTable.Current, nameof(DragActionTable.Elements)), new SettingMouseDragControl())
                    {
                        IsStretch = true,
                        IsEnabled = new IsEnabledPropertyValue(Config.Current.Mouse, nameof(MouseConfig.IsDragEnabled))
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsGestureEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.GestureMinimumDistance)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(Config.Current.Mouse, nameof(MouseConfig.IsGestureEnabled))
                    }),

                new SettingItemSection(Properties.Resources.SettingPageManipurateMouseHold,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonDownMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonMask))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonDownTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonRepeatTime)))),

                new SettingItemSection(Properties.Resources.SettingPageManipurateMouseVisibility, Properties.Resources.SettingPageManipurateMouseVisibilityTips,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsCursorHideEnabled))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.CursorHideTime))),
                        new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.CursorHideReleaseDistance))),
                        new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsCursorHideReleaseAction))))
                    {
                        IsEnabled = new IsEnabledPropertyValue(Config.Current.Mouse, nameof(MouseConfig.IsCursorHideEnabled))
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsEnabled))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.DragAction))),
                        new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.HoldAction))),
                        new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsAngleEnabled))),
                        new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsScaleEnabled))))
                    {
                        IsEnabled = new IsEnabledPropertyValue(Config.Current.Touch, nameof(TouchConfig.IsEnabled)),
                    }),

                new SettingItemSection(Properties.Resources.SettingPageManipurateTouchAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.GestureMinimumDistance))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.MinimumManipulationRadius))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.MinimumManipulationDistance))))
                {
                    IsEnabled = new IsEnabledPropertyValue(Config.Current.Touch, nameof(TouchConfig.IsEnabled)),
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsResetByRestart))),
                    new SettingItemProperty(PropertyMemberElement.Create(LoupeTransform.Current, nameof(LoupeTransform.IsVisibleLoupeInfo))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsResetByPageChanged))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsWheelScalingEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsEscapeKeyEnabled)))),

                new SettingItemSection(Properties.Resources.SettingPageManipurateLoupeAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.DefaultScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.MinimumScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.MaximumScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.ScaleStep))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.Speed)))),

            };
        }
    }
}
