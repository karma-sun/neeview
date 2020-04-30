using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: Manipurate
    /// </summary>
    public class SettingPageManipurate : SettingPage
    {
        public SettingPageManipurate() : base(Properties.Resources.SettingPageManipurate)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageMouse(),
                new SettingPageTouch(),
                new SettingPageLoupe(),
            };

            var section = new SettingItemSection(Properties.Resources.SettingPageManipurateGeneralViewOperation);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsLimitMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsViewStartPositionCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.RotateCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.ScaleCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.FlipCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepAngle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepFlip))));
            section.Children.Add(new SettingItemIndexValue<double>(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.AngleFrequency)), new AngleFrequency(), false));

            this.Items = new List<SettingItem>() { section };
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

    /// <summary>
    /// Setting: Mouse
    /// </summary>
    public class SettingPageMouse : SettingPage
    {
        public SettingPageMouse() : base(Properties.Resources.SettingPageManipurateMouse)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPageManipurateMouseDrag);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.MinimumDragDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsDragEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(DragActionTable.Current, nameof(DragActionTable.Elements)), new SettingMouseDragControl()) { IsStretch = true, });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsGestureEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.GestureMinimumDistance))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageManipurateMouseHold);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonDownMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonMask))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonDownTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonRepeatTime))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageManipurateMouseVisibility, Properties.Resources.SettingPageManipurateMouseVisibilityTips);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsCursorHideEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.CursorHideTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.CursorHideReleaseDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsCursorHideReleaseAction))));
            this.Items.Add(section);
        }
    }

    /// <summary>
    /// Setting: Touch
    /// </summary>
    public class SettingPageTouch : SettingPage
    {
        public SettingPageTouch() : base(Properties.Resources.SettingPageManipurateTouch)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageManipurateTouchGeneral);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.DragAction))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.HoldAction))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsAngleEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsScaleEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.GestureMinimumDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.MinimumManipulationRadius))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.MinimumManipulationDistance))));

            this.Items = new List<SettingItem>() { section };
        }
    }

    /// <summary>
    /// Setting: Loupte
    /// </summary>
    public class SettingPageLoupe : SettingPage
    {
        public SettingPageLoupe() : base(Properties.Resources.SettingPageManipurateLoupe)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageManipurateLoupeGeneral);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsLoupeCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsResetByRestart))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsVisibleLoupeInfo))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsResetByPageChanged))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsWheelScalingEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsEscapeKeyEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.DefaultScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.MinimumScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.MaximumScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.ScaleStep))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.Speed))));

            this.Items = new List<SettingItem>() { section };
        }
    }
}
