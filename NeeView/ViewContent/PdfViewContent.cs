// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// Pdf ViewContent
    /// </summary>
    public class PdfViewContent : BitmapViewContent
    {
        #region Fields

        // 現在使用中のコンテンツがスケールされたリソースか
        private bool _isScaled;

        // スケールされたリソースのサイズ
        private Size _size;

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
            this.Color = bitmapContent.BitmapInfo.Color;
        }

        //
        public override bool Rebuild(double scale)
        {
            if (_rebuilding) return false;

            try
            {
                var maxSize = new Size(Math.Min(this.Width * scale, PdfArchiverProfile.Current.RenderMaxSize.Width), Math.Min(this.Height * scale, PdfArchiverProfile.Current.RenderMaxSize.Height));

                if (this.Page.Width >= maxSize.Width || this.Page.Height >= maxSize.Height)
                {
                    if (_isScaled)
                    {
                        Debug.WriteLine($"PDF: Default");
                        _isScaled = false;
                        _size = new Size();
                        this.View = CreateView(this.Source, CreateBindingParameter());
                    }
                }
                else
                {
                    if (!_isScaled || _size != maxSize)
                    {
                        Debug.WriteLine($"PDF: Scaled: {(int)maxSize.Width}x{(int)maxSize.Height}");
                        _isScaled = true;
                        _size = maxSize;
                        _rebuilding = true;

                        Task.Run(() =>
                        {
                            var pdfArchiver = this.Page.Entry.Archiver as PdfArchiver;
                            var bitmapSource = pdfArchiver.CraeteBitmapSource(this.Page.Entry, maxSize);
                            App.Current.Dispatcher.BeginInvoke((Action)(() => this.View = CreateView(this.Source, CreateBindingParameter(), bitmapSource)));
                            _rebuilding = false;
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

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
