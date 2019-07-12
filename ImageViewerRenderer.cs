using System.ComponentModel;
using System.Linq;
using Biz4.Core.Controls;
using CoreGraphics;
using Fin1.iOS.Renderers;
using Foundation;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Diagnostics;

[assembly: ExportRenderer(typeof(ImageViewer), typeof(ImageViewerRenderer))]
namespace Fin1.iOS.Renderers
{
    public class ImageViewerRenderer : ViewRenderer<ImageViewer, UIView>
    {
        private ImageViewer _imageViewer => Element as ImageViewer;
        private UIImageView _imageView;
        private UIImage _image;
        private UIScrollView _scrollView;
        private UITapGestureRecognizer zoomGestureRecognizer;


        NSLayoutConstraint _imageViewTop;
        NSLayoutConstraint _imageViewBottom;
        NSLayoutConstraint _imageViewLeading;
        NSLayoutConstraint _imageViewTrailing;


        protected override void OnElementChanged(ElementChangedEventArgs<ImageViewer> e)
        {
            try
            {
                base.OnElementChanged(e);
                if (e.NewElement != null)
                {
                    var ctrl = CreateNativeControl();
                    SetNativeControl(ctrl);
                    Setup();
                    GetPhoneNumber();
                }
                  
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            } 
        }


        protected override UIView CreateNativeControl()
        {
            return new UIView();
        }

        private void zoomWhenDoubleTapped()
        {
            try
            {
               
                // Nếu zoomScale hiện tại > minimumZoomScale tức là ảnh đang bị zoom, douple tap sẽ zoom out về kích thước nhỏ nhất
                if (_scrollView.ZoomScale > _scrollView.MinimumZoomScale)
                {
                    _scrollView.SetZoomScale(_scrollView.MinimumZoomScale, animated: true);
                    
                }
                else
                {
                    // Nếu zoomScale hiện tại > minimumZoomScale tức là ảnh đang bị zoom, douple tap sẽ zoom in đến kích thước lớn nhất
                    var tapPoint = zoomGestureRecognizer.LocationInView(Control);
                    var imageSize = _imageView.Frame.Size;
                    
                    var width = imageSize.Width / _scrollView.MaximumZoomScale;
                    var height = imageSize.Height / _scrollView.MaximumZoomScale;

                    var center = _imageView.ConvertPointFromView(tapPoint, _scrollView);

                    var x = center.X - (width / 2.0);
                    var y = center.Y - (height / 2.0);

                    _scrollView.ZoomToRect(new CGRect(x,y,width,height), animated: true);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
        }

        private void GetPhoneNumber()
        {
            //var num = NSUserDefaults.StandardUserDefaults.StringForKey(@"SBFormattedPhoneNumber");
        }

        private void ScrollViewInit()
        {
            try
            {
                _scrollView = new UIScrollView();
                _scrollView.TranslatesAutoresizingMaskIntoConstraints = false;
                Control.AddSubview(_scrollView);

                _scrollView.TopAnchor.ConstraintEqualTo(TopAnchor, 0).Active = true;
                _scrollView.BottomAnchor.ConstraintEqualTo(BottomAnchor, 0).Active = true;
                _scrollView.LeadingAnchor.ConstraintEqualTo(LeadingAnchor, 0).Active = true;
                _scrollView.TrailingAnchor.ConstraintEqualTo(TrailingAnchor, 0).Active = true;
                _scrollView.ScrollEnabled = true;
                _scrollView.ShowsHorizontalScrollIndicator = false;
                _scrollView.ShowsVerticalScrollIndicator = false;

                _scrollView.ViewForZoomingInScrollView = GetZoomSubView;



                _scrollView.DidZoom += ScrollDidZoom;

                _scrollView.BackgroundColor = UIColor.Black;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
        }

    

        private void ImageViewInit()
        {
            try
            {
                _image = FromUrl(_imageViewer.ImageSource);

                _imageView = new UIImageView()
                {
                    Image = _image,

                };
                _scrollView.AddSubview(_imageView);
                //NSLayoutConstraint.Create(imageView,NSLayoutAttribute.Top,NSLayoutRelation.Equal,this, NSLayoutAttribute.LeadingMargin, 1.0f, 0.0f).Active = true;
                //NSLayoutConstraint.Create(imageView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.LeadingMargin, 1.0f, 0.0f).Active = true;
                //NSLayoutConstraint.Create(imageView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.LeadingMargin, 1.0f, 0.0f).Active = true;
                //NSLayoutConstraint.Create(imageView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this, NSLayoutAttribute.LeadingMargin, 1.0f, 0.0f).Active = true;
                _imageView.TranslatesAutoresizingMaskIntoConstraints = false;




                _imageViewTop = _imageView.TopAnchor.ConstraintEqualTo(_scrollView.TopAnchor);
                _imageViewBottom = _imageView.BottomAnchor.ConstraintEqualTo(_scrollView.BottomAnchor);
                _imageViewLeading = _imageView.LeadingAnchor.ConstraintEqualTo(_scrollView.LeadingAnchor);
                _imageViewTrailing = _imageView.TrailingAnchor.ConstraintEqualTo(_scrollView.TrailingAnchor);


                _imageViewTop.Active = true;
                _imageViewBottom.Active = true;
                _imageViewLeading.Active = true;
                _imageViewTrailing.Active = true;

                
                

                _imageView.ClipsToBounds = true;
                _imageView.AutosizesSubviews = true;
                _imageView.Opaque = true;

                _imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                _imageView.BackgroundColor = UIColor.Black;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            

        }

        private void Setup()
        {
            try
            {
                ScrollViewInit();
                ImageViewInit();


                zoomGestureRecognizer = new UITapGestureRecognizer();
                zoomGestureRecognizer.AddTarget(zoomWhenDoubleTapped);
                zoomGestureRecognizer.NumberOfTapsRequired = 2;

                

                Control.AddGestureRecognizer(zoomGestureRecognizer);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
        }
        private UIImage FromUrl(string uri)
        {
            try
            {
                if(!string.IsNullOrEmpty(uri))
                {
                    using (var url = new NSUrl(uri))
                    using (var data = NSData.FromUrl(url))
                        return UIImage.LoadFromData(data);
                }
                
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return new UIImage();
            
        }


        private void ScrollDidZoom(object sender, EventArgs e)
        {
            updateImageViewConstraints();
        }
        
        private void updateImageViewConstraints()
        {
            try
            {
                var yOffset = NMath.Max(0, (Frame.Height - _imageView.Frame.Height) / 2);

                _imageViewTop.Constant = yOffset;
                _imageViewBottom.Constant = yOffset;


                var xOffset = NMath.Max(0, (Frame.Width - _imageView.Frame.Width) / 2);
                _imageViewLeading.Constant = xOffset;
                _imageViewTrailing.Constant = xOffset;

                LayoutIfNeeded();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }

            

        }
        //private void CropImage()
        //{
        //    UIGraphics.BeginImageContextWithOptions(Bounds.Size, true, UIScreen.MainScreen.Scale);
        //    UIGraphics.GetCurrentContext().TranslateCTM(-ContentOffset.X, -ContentOffset.Y);

        //    Layer.RenderInContext(UIGraphics.GetCurrentContext());

        //    var image = UIGraphics.GetImageFromCurrentImageContext();
        //    UIGraphics.EndImageContext();
        //    image.SaveToPhotosAlbum(image)

        //}
        private void updateMinZoomScale()
        {
            try
            {
                var widthScale = Bounds.Width / _imageView.Image.Size.Width;
                var heightScale = Bounds.Height / _imageView.Image.Size.Height;
                var minScale = NMath.Min(widthScale, heightScale);
                _scrollView.MinimumZoomScale = minScale;
                _scrollView.ZoomScale = minScale;
                _scrollView.MaximumZoomScale = NMath.Max(1, minScale * 3);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
        }
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

        }


        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == ImageViewer.MinimumZoomScaleProperty.PropertyName ||
                e.PropertyName == ImageViewer.MaximumZoomScaleProperty.PropertyName)
            {
                UpdateMinMaxScale();
            }
            if(e.PropertyName == ImageViewer.ImageSourceProperty.PropertyName)
            {
                if (string.IsNullOrEmpty(_imageViewer.ImageSource))
                    return;
                ScrollViewInit();
                ImageViewInit();
                updateMinZoomScale();
                updateImageViewConstraints();
            }
            if(e.PropertyName == VisualElement.WidthProperty.PropertyName || e.PropertyName == VisualElement.HeightProperty.PropertyName)
            {
                this.Frame = new CGRect(Frame.X, Frame.Y, this.Element.Width, this.Element.Height);
                ScrollViewInit();
                ImageViewInit();
                updateMinZoomScale();
                updateImageViewConstraints();
               
            }
            if(e.PropertyName == "Renderer")
            {
                //this.Frame = new CGRect(Frame.X, Frame.Y, this.Element.Width, this.Element.Height);
                //ScrollViewInit();
                //ImageViewInit();
                //updateMinZoomScale();
                //updateImageViewConstraints();
            }

        }
        private void SetImageSoure(string src)
        {
            if(_imageView != null && !string.IsNullOrEmpty(src))
            {
                _imageView.Image = FromUrl(Element.ImageSource);
                _imageView.UpdateFocusIfNeeded();
            }
        }
        private UIView GetZoomSubView(UIScrollView scrollView)
        {
            return _imageView;
        }

        private void UpdateMinMaxScale()
        {
            if (_imageViewer != null && _scrollView != null)
            {
                _scrollView.MinimumZoomScale = _imageViewer.MinimumZoomScale;
                _scrollView.MaximumZoomScale = _imageViewer.MaximumZoomScale;
            }
        }
    }
}
