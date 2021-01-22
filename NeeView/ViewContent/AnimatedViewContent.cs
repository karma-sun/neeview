using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// Animated ViewContent
    /// </summary>
    public class AnimatedViewContent : BitmapViewContent
    {
        private AnimatedContent _animatedContent;
        private ViewContentParameters _parameter;
        private AnimatedView _animatedView;


        public AnimatedViewContent(MainViewComponent viewComponent, ViewContentSource source) : base(viewComponent, source)
        {
            _animatedContent = (AnimatedContent)source.Content;
        }


        private void Initialize()
        {
            // binding parameter
            _parameter = CreateBindingParameter();

            // create view
            this.View = new ViewContentControl(CreateAnimatedView(this.Source, _parameter));

            // content setting
            this.Color = _animatedContent.Color;
            this.FileProxy = _animatedContent.FileProxy;
        }

        /// <summary>
        /// アニメーションビュー生成
        /// </summary>
        private FrameworkElement CreateAnimatedView(ViewContentSource source, ViewContentParameters parameter)
        {
            var imageView = base.CreateView(source, parameter);

            // NOTE: アニメーション画像ではない場合は画像ビューにする
            if (!_animatedContent.IsAnimated)
            {
                return imageView;
            }

#pragma warning disable CS0618 // 型またはメンバーが旧型式です
            var uri = new Uri(_animatedContent.FileProxy.Path, true);
#pragma warning restore CS0618 // 型またはメンバーが旧型式です

            _animatedView = new AnimatedView(source, parameter, uri, imageView);
            return _animatedView.View;
        }

        public override void OnAttached()
        {
            base.OnAttached();
            _animatedView?.OnAttached();
        }

        public override void OnDetached()
        {
            base.OnDetached();
            _animatedView?.OnDetached();
        }

        public override bool Rebuild(double scale)
        {
            if (_animatedView != null)
            {
                return true;
            }
            else
            {
                return base.Rebuild(scale);
            }
        }

        public override void UpdateViewBox()
        {
            if (_animatedView != null)
            {
                _animatedView.UpdateViewBox();
            }
            else
            {
                base.UpdateViewBox();
            }
        }


        public new static AnimatedViewContent Create(MainViewComponent viewComponent, ViewContentSource source)
        {
            var viewContent = new AnimatedViewContent(viewComponent, source);
            viewContent.Initialize();
            return viewContent;
        }
    }

}
