namespace NeeView
{
    public class ResetTransformConditionBuilder
    {
        public bool IsResetScale { get; set; }
        public bool IsResetAngle { get; set; }
        public bool IsResetFlip { get; set; }

        public ResetTransformConditionBuilder()
        {
        }

        public ResetTransformConditionBuilder Force(bool isForce = true)
        {
            if (isForce)
            {
                IsResetScale = true;
                IsResetAngle = true;
                IsResetFlip = true;
            }
            return this;
        }

        public ResetTransformConditionBuilder BookChanged()
        {
            IsResetScale = IsResetScale || !(Config.Current.View.IsKeepScale && Config.Current.View.IsKeepScaleBooks);
            IsResetAngle = IsResetAngle || !(Config.Current.View.IsKeepAngle && Config.Current.View.IsKeepAngleBooks);
            IsResetFlip = IsResetFlip || !(Config.Current.View.IsKeepFlip && Config.Current.View.IsKeepFlipBooks);
            return this;
        }

        public ResetTransformConditionBuilder PageChanged()
        {
            IsResetScale = IsResetScale || !Config.Current.View.IsKeepScale;
            IsResetAngle = IsResetAngle || !Config.Current.View.IsKeepAngle;
            IsResetFlip = IsResetFlip || !Config.Current.View.IsKeepFlip;
            return this;
        }

        public ResetTransformConditionBuilder AngleResetMode(AngleResetMode angleResetMode)
        {
            IsResetAngle = IsResetAngle || angleResetMode != NeeView.AngleResetMode.None;
            return this;
        }

        public ResetTransformCondition ToResult()
        {
            return new ResetTransformCondition(IsResetScale, IsResetAngle, IsResetFlip);
        }
    }
}
