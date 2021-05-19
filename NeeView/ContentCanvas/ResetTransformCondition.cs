namespace NeeView
{
    public class ResetTransformCondition
    {
        public ResetTransformCondition()
        {
        }

        public ResetTransformCondition(bool flag)
        {
            IsResetScale = flag;
            IsResetAngle = flag;
            IsResetFlip = flag;
        }

        public ResetTransformCondition(bool isResetScale, bool isResetAngle, bool isResetFlip)
        {
            IsResetScale = isResetScale;
            IsResetAngle = isResetAngle;
            IsResetFlip = isResetFlip;
        }

        public bool IsResetScale { get; }
        public bool IsResetAngle { get; }
        public bool IsResetFlip { get; }


        public static ResetTransformCondition Create(bool isForce, AngleResetMode angleResetMode)
        {
            return new ResetTransformConditionBuilder()
                .Force(isForce)
                .PageChanged()
                .AngleResetMode(angleResetMode)
                .ToResult();
        }

        public static ResetTransformCondition Create(bool isBookChanged)
        {
            var builder = new ResetTransformConditionBuilder();

            if (isBookChanged)
            {
                builder = builder.BookChanged();
            }
            else
            {
                builder = builder.PageChanged();
            }

            return builder.ToResult();
        }


    }
}
