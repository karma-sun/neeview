// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// フォルダーリストの表示方法
    /// </summary>
    public enum PanelListItemStyle
    {
        Normal, // テキストのみ
        Content, // コンテンツ
        Banner, // バナー
    };

    public static class PanelListItemStyleExtensions
    {
        public static bool HasThumbnail(this PanelListItemStyle my)
        {
            return (my == PanelListItemStyle.Content || my == PanelListItemStyle.Banner);
        }
    }

    //
    public class PanelListItemStyleToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PanelListItemStyle v0))
                return false;

            if (!(parameter is PanelListItemStyle v1))
                return false;

            return v0 == v1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }




    /// <summary>
    /// ファイル情報ペイン設定
    /// </summary>
    [DataContract]
    public class FolderListSetting
    {
        [DataMember]
        public Dock Dock { get; set; }

        [DataMember]
        public bool IsVisibleHistoryMark { get; set; }

        [DataMember]
        public bool IsVisibleBookmarkMark { get; set; }

        //
        private void Constructor()
        {
            Dock = Dock.Left;
            IsVisibleHistoryMark = true;
            IsVisibleBookmarkMark = true;
        }

        public FolderListSetting()
        {
            Constructor();
        }

        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

        //
        public FolderListSetting Clone()
        {
            return (FolderListSetting)MemberwiseClone();
        }
    }
}
