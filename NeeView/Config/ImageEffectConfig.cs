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
        [PropertyMember("@ParamImageEffectEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// 適用するエフェクトの種類
        /// </summary>
        [PropertyMember("@ParamImageEffectType")]
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

        [PropertyMapLabel("@EnumEffectTypeLevel")]
        public LevelEffectUnit LevelEffect { get; set; } = new LevelEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeHsv")]
        public HsvEffectUnit HsvEffect { get; set; } = new HsvEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeColorSelect")]
        public ColorSelectEffectUnit ColorSelectEffect { get; set; } = new ColorSelectEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeBlur")]
        public BlurEffectUnit BlurEffect { get; set; } = new BlurEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeBloom")]
        public BloomEffectUnit BloomEffect { get; set; } = new BloomEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeMonochrome")]
        public MonochromeEffectUnit MonochromeEffect { get; set; } = new MonochromeEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeColorTone")]
        public ColorToneEffectUnit ColorToneEffect { get; set; } = new ColorToneEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeSharpen")]
        public SharpenEffectUnit SharpenEffect { get; set; } = new SharpenEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeEmbossed")]
        public EmbossedEffectUnit EmbossedEffect { get; set; } = new EmbossedEffectUnit();

        [PropertyMapLabel("@EnumEffectTypePixelate")]
        public PixelateEffectUnit PixelateEffect { get; set; } = new PixelateEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeMagnify")]
        public MagnifyEffectUnit MagnifyEffect { get; set; } = new MagnifyEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeRipple")]
        public RippleEffectUnit RippleEffect { get; set; } = new RippleEffectUnit();

        [PropertyMapLabel("@EnumEffectTypeSwirl")]
        public SwirlEffectUnit SwirlEffect { get; set; } = new SwirlEffectUnit();
    }
}