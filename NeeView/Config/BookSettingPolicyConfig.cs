using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    public class BookSettingPolicyConfig : BindableBase, ICloneable
    {
        private BookSettingPageSelectMode _page = BookSettingPageSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _pageMode = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _bookReadOrder = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _isSupportedDividePage = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _isSupportedSingleFirstPage = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _isSupportedSingleLastPage = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _isSupportedWidePage = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _isRecursiveFolder = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _sortMode = BookSettingSelectMode.RestoreOrDefault;


        [PropertyMember("@ParamBookPolicyPage")]
        public BookSettingPageSelectMode Page
        {
            get { return _page; }
            set { SetProperty(ref _page, value); }
        }

        [PropertyMember("@ParamBookPolicyPageMode")]
        public BookSettingSelectMode PageMode
        {
            get { return _pageMode; }
            set { SetProperty(ref _pageMode, value); }
        }

        [PropertyMember("@ParamBookPolicyBookReadOrder")]
        public BookSettingSelectMode BookReadOrder
        {
            get { return _bookReadOrder; }
            set { SetProperty(ref _bookReadOrder, value); }
        }

        [PropertyMember("@ParamBookPolicyIsSupportedDividePage")]
        public BookSettingSelectMode IsSupportedDividePage
        {
            get { return _isSupportedDividePage; }
            set { SetProperty(ref _isSupportedDividePage, value); }
        }

        [PropertyMember("@ParamBookPolicyIsSupportedSingleFirstPage")]
        public BookSettingSelectMode IsSupportedSingleFirstPage
        {
            get { return _isSupportedSingleFirstPage; }
            set { SetProperty(ref _isSupportedSingleFirstPage, value); }
        }

        [PropertyMember("@ParamBookPolicyIsSupportedSingleLastPage")]
        public BookSettingSelectMode IsSupportedSingleLastPage
        {
            get { return _isSupportedSingleLastPage; }
            set { SetProperty(ref _isSupportedSingleLastPage, value); }
        }

        [PropertyMember("@ParamBookPolicyIsSupportedWidePage")]
        public BookSettingSelectMode IsSupportedWidePage
        {
            get { return _isSupportedWidePage; }
            set { SetProperty(ref _isSupportedWidePage, value); }
        }

        [PropertyMember("@ParamBookPolicyIsRecursiveFolder")]
        public BookSettingSelectMode IsRecursiveFolder
        {
            get { return _isRecursiveFolder; }
            set { SetProperty(ref _isRecursiveFolder, value); }
        }

        [PropertyMember("@ParamBookPolicySortMode")]
        public BookSettingSelectMode SortMode
        {
            get { return _sortMode; }
            set { SetProperty(ref _sortMode, value); }
        }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }

}