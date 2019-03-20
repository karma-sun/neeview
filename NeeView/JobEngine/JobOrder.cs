namespace NeeView
{
    /// <summary>
    /// JOB要求票
    /// </summary>
    public class JobOrder
    {
        public JobOrder(JobCategory category, object key)
        {
            Category = category;
            Key = key;
        }

        public JobCategory Category { get; }
        public object Key { get; }

        public JobSource CreateJobSource()
        {
            return new JobSource(Category, Key);
        }
    }
}
