// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// Pdf ViewContent
    /// </summary>
    public class PdfViewContent : BitmapViewContent
    {
        #region Fields

        // スケールされたリソースを作成中
        private volatile bool _rebuilding;

        #endregion

        #region Constructors

        public PdfViewContent(ViewContentSource source, ViewContent old) : base(source, old)
        {
        }

        #endregion

        #region Medhots

        public new void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = CreateView(this.Source, parameter);

            // content setting
            var bitmapContent = this.Content as BitmapContent;
            this.Color = bitmapContent.Color;
        }

        //
        public override bool Rebuild(double scale)
        {
            if (_rebuilding) return false;

            var size = new Size(this.Width * scale, this.Height * scale);

            _rebuilding = true;

            Task.Run(() =>
            {
                try
                {
                    Resize(size);
                    App.Current.Dispatcher.Invoke((Action)(() => this.View = CreateView(this.Source, CreateBindingParameter())));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    _rebuilding = false;
                }
            });

            return true;
        }

        #endregion

        #region Static Methods

        public new static PdfViewContent Create(ViewContentSource source, ViewContent oldViewContent)
        {
            var viewContent = new PdfViewContent(source, oldViewContent);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }
}
