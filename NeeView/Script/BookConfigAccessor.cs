using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeeView
{
    public class BookConfigAccessor
    {
        // PageMode
        [WordNodeMember]
        public int ViewPageSize
        {
            get { return BookSettingPresenter.Current.LatestSetting.PageMode == PageMode.WidePage ? 2 : 1; }
            set { AppDispatcher.Invoke(() => BookSettingPresenter.Current.SetPageMode(value == 2 ? PageMode.WidePage : PageMode.SinglePage)); }
        }

        // [Parameter(typeof(BookReadOrder))]
        [WordNodeMember]
        public string BookReadOrder
        {
            get { return BookSettingPresenter.Current.LatestSetting.BookReadOrder.ToString(); }
            set { AppDispatcher.Invoke(() => BookSettingPresenter.Current.SetBookReadOrder(value.ToEnum<PageReadOrder>())); }
        }

        [WordNodeMember]
        public bool IsSupportedDividePage
        {
            get { return BookSettingPresenter.Current.LatestSetting.IsSupportedDividePage; }
            set { AppDispatcher.Invoke(() => BookSettingPresenter.Current.SetIsSupportedDividePage(value)); }
        }

        [WordNodeMember]
        public bool IsSupportedSingleFirstPage
        {
            get { return BookSettingPresenter.Current.LatestSetting.IsSupportedSingleFirstPage; }
            set { AppDispatcher.Invoke(() => BookSettingPresenter.Current.SetIsSupportedSingleFirstPage(value)); }
        }

        [WordNodeMember]
        public bool IsSupportedSingleLastPage
        {
            get { return BookSettingPresenter.Current.LatestSetting.IsSupportedSingleLastPage; }
            set { AppDispatcher.Invoke(() => BookSettingPresenter.Current.SetIsSupportedSingleLastPage(value)); }
        }

        [WordNodeMember]
        public bool IsSupportedWidePage
        {
            get { return BookSettingPresenter.Current.LatestSetting.IsSupportedWidePage; }
            set { AppDispatcher.Invoke(() => BookSettingPresenter.Current.SetIsSupportedWidePage(value)); }
        }

        [WordNodeMember]
        public bool IsRecursiveFolder
        {
            get { return BookSettingPresenter.Current.LatestSetting.IsRecursiveFolder; }
            set { AppDispatcher.Invoke(() => BookSettingPresenter.Current.SetIsRecursiveFolder(value)); }
        }

        // [Parameter(typeof(PageSortMode))]
        [WordNodeMember]
        public string SortMode
        {
            get { return BookSettingPresenter.Current.LatestSetting.SortMode.ToString(); }
            set { AppDispatcher.Invoke(() => BookSettingPresenter.Current.SetSortMode(value.ToEnum<PageSortMode>())); }
        }


        internal WordNode CreateWordNode(string name)
        {
            var node = new WordNode(name);
            node.Children = new List<WordNode>();

            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                node.Children.Add(new WordNode(property.Name));
            }

            return node;
        }
    }
}
