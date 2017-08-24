// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// Message ViewContent
    /// </summary>
    public class MessageViewContent : ViewContent
    {
        #region Constructors

        public MessageViewContent(ViewPage source, ViewContent old) : base(source, old)
        {
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = CreateView(this.Source, parameter);

            // content setting
            this.Size = new Size(480, 480);
        }

        //
        private FrameworkElement CreateView(ViewPage source, ViewContentParameters parameter)
        {
            var filepage = new FilePageContent()
            {
                Icon = Content.PageMessage.Icon,
                FileName = Content.Entry.EntryName,
                Message = Content.PageMessage.Message,
            };

            var control = new FilePageControl(filepage);
            control.SetBinding(FilePageControl.DefaultBrushProperty, parameter.ForegroundBrush);
            return control;
        }

        //
        public override bool IsBitmapScalingModeSupported() => false;

        #endregion
    
        #region Static Methods

        public static MessageViewContent Create(ViewPage source, ViewContent oldViewContent)
        {
            var viewContent = new MessageViewContent(source, oldViewContent);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }
}
