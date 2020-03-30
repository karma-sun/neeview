using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageEffectConfig : BindableBase
    {
        private bool _isEnabled;
        private EffectType _effectType = EffectType.Level;
        private bool _isHsvMode;

        /// <summary>
        /// エフェクト有効
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// 適用するエフェクトの種類
        /// </summary>
        public EffectType EffectType
        {
            get { return _effectType; }
            set { SetProperty(ref _effectType, value); }
        }

        /// <summary>
        /// 色をHSV表示
        /// </summary>
        [PropertyMember("@ParamImageEffectIsHsvMode", Tips = "@ParamImageEffectIsHsvModeTips")]
        public bool IsHsvMode
        {
            get { return _isHsvMode; }
            set { SetProperty(ref _isHsvMode, value); }
        }

        [PropertyMapLabelAttribute("@EnumEffectTypeLevel")]
        public LevelEffectUnit LevelEffect { get; set; } = new LevelEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeHsv")]
        public HsvEffectUnit HsvEffect { get; set; } = new HsvEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeColorSelect")]
        public ColorSelectEffectUnit ColorSelectEffect { get; set; } = new ColorSelectEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeBlur")]
        public BlurEffectUnit BlurEffect { get; set; } = new BlurEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeBloom")]
        public BloomEffectUnit BloomEffect { get; set; } = new BloomEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeMonochrome")]
        public MonochromeEffectUnit MonochromeEffect { get; set; } = new MonochromeEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeColorTone")]
        public ColorToneEffectUnit ColorToneEffect { get; set; } = new ColorToneEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeSharpen")]
        public SharpenEffectUnit SharpenEffect { get; set; } = new SharpenEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeEmbossed")]
        public EmbossedEffectUnit EmbossedEffect { get; set; } = new EmbossedEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypePixelate")]
        public PixelateEffectUnit PixelateEffect { get; set; } = new PixelateEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeMagnify")]
        public MagnifyEffectUnit MagnifyEffect { get; set; } = new MagnifyEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeRipple")]
        public RippleEffectUnit RippleEffect { get; set; } = new RippleEffectUnit();

        [PropertyMapLabelAttribute("@EnumEffectTypeSwirl")]
        public SwirlEffectUnit SwirlEffect { get; set; } = new SwirlEffectUnit();
    }
}