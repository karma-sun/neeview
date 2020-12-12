namespace NeeView
{
    /// <summary>
    /// TAB補完候補属性
    /// </summary>
    public class WordNodeMemberAttribute : DocumentableAttribute
    {
        public bool IsAutoCollect { get; set; } = true;
    }
}
