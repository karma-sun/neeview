namespace NeeView.Media.Imaging.Metadata
{
    public enum ExifFlashMode
    {
        FlashDidNotFire = 0x0000,
        FlashFired = 0x0001,
        StrobeReturnLightNotDetected = 0x0005,
        StrobeReturnLightDetected = 0x0007,
        FlashFired_CompulsoryFlashMode = 0x0009,
        FlashFired_CompulsoryFlashMode_ReturnLightNotDetected = 0x000D,
        FlashFired_CompulsoryFlashMode_ReturnLightDetected = 0x000F,
        FlashDidNotFire_CompulsoryFlashMode = 0x0010,
        FlashDidNotFire_AutoMode = 0x0018,
        FlashFired_AutoMode = 0x0019,
        FlashFired_AutoMode_ReturnLightNotDetected = 0x001D,
        FlashFired_AutoMode_ReturnLightDetected = 0x001F,
        NoFlashFunction = 0x0020,
        FlashFired_RedEyeReductionMode = 0x0041,
        FlashFired_RedEyeReductionMode_ReturnLightNotDetected = 0x0045,
        FlashFired_RedEyeReductionMode_ReturnLightDetected = 0x0047,
        FlashFired_CompulsoryFlashMode_RedEyeReductionMode = 0x0049,
        FlashFired_CompulsoryFlashMode_RedEyeReductionMode_ReturnLightNotDetected = 0x004D,
        FlashFired_CompulsoryFlashMode_RedEyeReductionMode_ReturnLightDetected = 0x004F,
        FlashFired_AutoMode_RedEyeReductionMode = 0x0059,
        FlashFired_AutoMode_ReturnLightNotDetected_RedEyeReductionMode = 0x005D,
        FlashFired_AutoMode_ReturnLightDetected_RedEyeReductionMode = 0x005F,
    }

    public static class ExifFlashModeExtensions
    {
        public static ExifFlashMode ToExifFlashMode(int value)
        {
            if (System.Enum.IsDefined(typeof(ExifFlashMode), value))
            {
                return (ExifFlashMode)value;
            }
            else
            {
                // 未定義の場合にフラッシュのON/OFFのみを返す
                return (ExifFlashMode)(value & 0x0001);
            }
        }
    }
}
