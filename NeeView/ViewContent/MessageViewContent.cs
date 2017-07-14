// Copyright (c) 2016 Mitsuhiro Ito (nee)
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

        public MessageViewContent(ViewContentSource source) : base(source)
        {
        }

        #endregion

        #region Methods

        public void Initialize(ViewContent oldViewContent)
        {
            Debug.Assert(this.Source.GetContentType() == ViewContentType.Message);

            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            var view = new PageContentView(LoosePath.GetFileName(this.Source.Page.FullPath));
            view.Content = CreateView(this.Source, parameter);
            this.View = view;

            // content setting
            this.Size = new Size(480, 480);
        }

        //
        private FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter)
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

        public static MessageViewContent Create(ViewContentSource source, ViewContent oldViewContent)
        {
            var viewContent = new MessageViewContent(source);
            viewContent.Initialize(oldViewContent);
            return viewContent;
        }

        #endregion
    }
}
