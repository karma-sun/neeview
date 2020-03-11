namespace NeeView
{
    /// <summary>
    /// 本の状態
    /// </summary>
    public class BookAccessor
    {
        public string Address => Book()?.Address;

        public bool IsMedia => Book()?.IsMedia == true;

        public bool IsNew => Book()?.IsNew == true;


        private Book Book() => BookOperation.Current.Book;
    }

}
