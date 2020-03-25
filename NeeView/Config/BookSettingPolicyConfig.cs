using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    public class BookSettingPolicyConfig : BindableBase, ICloneable
    {
        [PropertyMember]
        public BookSettingPageSelectMode Page { get; set; } = BookSettingPageSelectMode.RestoreOrDefault;

        [PropertyMember]
        public BookSettingSelectMode PageMode { get; set; } = BookSettingSelectMode.RestoreOrDefault;

        [PropertyMember]
        public BookSettingSelectMode BookReadOrder { get; set; } = BookSettingSelectMode.RestoreOrDefault;

        [PropertyMember]
        public BookSettingSelectMode IsSupportedDividePage { get; set; } = BookSettingSelectMode.RestoreOrDefault;

        [PropertyMember]
        public BookSettingSelectMode IsSupportedSingleFirstPage { get; set; } = BookSettingSelectMode.RestoreOrDefault;

        [PropertyMember]
        public BookSettingSelectMode IsSupportedSingleLastPage { get; set; } = BookSettingSelectMode.RestoreOrDefault;

        [PropertyMember]
        public BookSettingSelectMode IsSupportedWidePage { get; set; } = BookSettingSelectMode.RestoreOrDefault;

        [PropertyMember]
        public BookSettingSelectMode IsRecursiveFolder { get; set; } = BookSettingSelectMode.RestoreOrDefault;

        [PropertyMember]
        public BookSettingSelectMode SortMode { get; set; } = BookSettingSelectMode.RestoreOrDefault;


        public object Clone()
        {
            return MemberwiseClone();
        }
    }

}