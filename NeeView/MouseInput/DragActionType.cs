using System;

namespace NeeView
{
    // ドラッグアクションの種類
    [Obsolete]
    public enum DragActionType
    {
        [AliasName]
        None,

        [AliasName]
        Gesture,

        [AliasName]
        Move,

        [AliasName]
        MoveScale,

        [AliasName]
        Angle,

        [AliasName]
        AngleSlider,

        [AliasName]
        Scale,

        [AliasName]
        ScaleSlider,

        [AliasName]
        ScaleSliderCentered,

        [AliasName]
        MarqueeZoom,

        [AliasName]
        FlipHorizontal,

        [AliasName]
        FlipVertical,

        [AliasName]
        WindowMove,
    }

}
