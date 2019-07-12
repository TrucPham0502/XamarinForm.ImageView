using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Biz4.Core.Controls;
using Fin1.Droid.Renderers;
using Xamarin.Forms.Platform.Android;
using static Android.Views.ScaleGestureDetector;
using Android.Media;
using Java.IO;
using Java.Net;
using Android.App;
using Android.Runtime;
using static Android.Widget.ImageView;
using System.Text;
using static Android.Views.View;
using Android.Provider;
using Android.Graphics.Drawables;
using Android.OS;

[assembly: Xamarin.Forms.ExportRenderer(typeof(ImageViewer), typeof(ImageViewerRenderer))]
namespace Fin1.Droid.Renderers
{
    public class ImageViewerRenderer : ViewRenderer<ImageViewer, ImageView>, IOnScaleGestureListener, IOnTouchListener
    {
        private ImageViewer _imageViewer => Element as ImageViewer;
        Matrix matrix = new Matrix();

        const int NONE = 0;
        const int DRAG = 1;
        const int ZOOM = 2;
        const int CLICK = 3;
        int mode = NONE;

        PointF last = new PointF();
        PointF start = new PointF();
        float minScale = 1f;
        float maxScale = 4f;
        float[] m;

        float redundantXSpace, redundantYSpace;
        float width, height;
        float saveScale = 1f;
        float right, bottom, origWidth, origHeight, bmWidth, bmHeight;

        ScaleGestureDetector mScaleDetector;
        GestureDetector mDoubleTapDetector;


        Context context;
        public ImageViewerRenderer(Context context) : base(context)
        {
            this.context = context;
            mScaleDetector = new ScaleGestureDetector(context, this);
            matrix.SetTranslate(1f, 1f);
            m = new float[9];

        }

        protected override void OnElementChanged(ElementChangedEventArgs<ImageViewer> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                var ctrl = CreateNativeControl();
                SetNativeControl(ctrl);


                Control.ImageMatrix = (matrix);
                Control.SetScaleType(ScaleType.Matrix);

                SetImageUri(_imageViewer.ImageSource);

                Control.SetOnTouchListener(this);

                Control.SetBackgroundColor(Color.Black);


                mScaleDetector = new ScaleGestureDetector(this.context, this);

                mDoubleTapDetector = new GestureDetector(this.context, new DoubleTapListener((mo) =>
                {
                    DoubleAction(mo);
                }));
            }



        }

        private void DoubleAction(MotionEvent e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MAIN_TAG", "Double tap detected");
                float origScale = saveScale;
                float mScaleFactor;

                if (saveScale > minScale)
                {
                    ResetDefault();
                }
                else
                {
                    saveScale = maxScale;
                    mScaleFactor = maxScale / origScale;
                    ChangeScale(mScaleFactor, e.GetX(), e.GetY());

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

        }

        protected override ImageView CreateNativeControl()
        {
            var imageView = new ImageView(this.context);
            //imageView.SetImageURI(Android.Net.Uri.Parse(Element.ImageSource));
            return imageView;

        }


        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == ImageViewer.MinimumZoomScaleProperty.PropertyName ||
                e.PropertyName == ImageViewer.MaximumZoomScaleProperty.PropertyName)
            {
                UpdateMinMaxScale();
            }
            if (e.PropertyName == ImageViewer.ImageSourceProperty.PropertyName)
            {
                SetImageUri(_imageViewer.ImageSource);
            }
        }


        private void UpdateMinMaxScale()
        {
            if (_imageViewer != null)
            {
                this.minScale = _imageViewer.MinimumZoomScale;
                this.maxScale = _imageViewer.MaximumZoomScale;
            }
        }

        public bool OnScale(ScaleGestureDetector detector)
        {
            try
            {
                float mScaleFactor = detector.ScaleFactor;
                float origScale = saveScale;
                saveScale *= mScaleFactor;
                if (saveScale > maxScale)
                {
                    saveScale = maxScale;
                    mScaleFactor = maxScale / origScale;
                }
                else if (saveScale < minScale)
                {
                    saveScale = minScale;
                    mScaleFactor = minScale / origScale;
                }
                ChangeScale(mScaleFactor, detector.FocusX, detector.FocusY);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return false;

        }

        private void ChangeScale(float mScaleFactor, float FocusX, float FocusY)
        {
            try
            {
                right = width * saveScale - width - (2 * redundantXSpace * saveScale);
                bottom = height * saveScale - height - (2 * redundantYSpace * saveScale);
                if (origWidth * saveScale <= width || origHeight * saveScale <= height)
                {
                    matrix.PostScale(mScaleFactor, mScaleFactor, width / 2, height / 2);
                    if (mScaleFactor < 1)
                    {
                        matrix.GetValues(m);
                        float x = m[Matrix.MtransX];
                        float y = m[Matrix.MtransY];
                        if (mScaleFactor < 1)
                        {
                            if (Math.Round(origWidth * saveScale) < width)
                            {
                                if (y < -bottom)
                                    matrix.PostTranslate(0, -(y + bottom));
                                else if (y > 0)
                                    matrix.PostTranslate(0, -y);
                            }
                            else
                            {
                                if (x < -right)
                                    matrix.PostTranslate(-(x + right), 0);
                                else if (x > 0)
                                    matrix.PostTranslate(-x, 0);
                            }
                        }
                    }
                }
                else
                {
                    matrix.PostScale(mScaleFactor, mScaleFactor, FocusX, FocusY);
                    matrix.GetValues(m);
                    float x = m[Matrix.MtransX];
                    float y = m[Matrix.MtransY];
                    if (mScaleFactor < 1)
                    {
                        if (x < -right)
                            matrix.PostTranslate(-(x + right), 0);
                        else if (x > 0)
                            matrix.PostTranslate(-x, 0);
                        if (y < -bottom)
                            matrix.PostTranslate(0, -(y + bottom));
                        else if (y > 0)
                            matrix.PostTranslate(0, -y);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            width = MeasureSpec.GetSize(widthMeasureSpec);
            height = MeasureSpec.GetSize(heightMeasureSpec);


            if (saveScale.Equals(minScale))
                ResetDefault();




        }

        private void ResetDefault()
        {
            try
            {
                //Fit to screen.
                float scale;
                float scaleX = width / bmWidth;
                float scaleY = height / bmHeight;
                scale = Math.Min(scaleX, scaleY);
                matrix.SetScale(scale, scale);
                Control.ImageMatrix = (matrix);
                saveScale = minScale;

                // Center the image
                redundantYSpace = height - (scale * bmHeight);
                redundantXSpace = width - (scale * bmWidth);
                redundantYSpace /= 2;
                redundantXSpace /= 2;

                matrix.PostTranslate(redundantXSpace, redundantYSpace);

                origWidth = width - 2 * redundantXSpace;
                origHeight = height - 2 * redundantYSpace;
                right = width * saveScale - width - (2 * redundantXSpace * saveScale);
                bottom = height * saveScale - height - (2 * redundantYSpace * saveScale);
                Control.ImageMatrix = (matrix);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }


        }
        public bool OnScaleBegin(ScaleGestureDetector detector)
        {
            mode = ZOOM;
            RequestDisallowInterceptTouchEvent(true);
            return true;
        }

        public void OnScaleEnd(ScaleGestureDetector detector)
        {
            mode = NONE;
            if (saveScale.Equals(minScale)) ResetDefault();
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            try
            {
                mDoubleTapDetector.OnTouchEvent(e);

                mScaleDetector.OnTouchEvent(e);

                matrix.GetValues(m);
                float x = m[Matrix.MtransX];
                float y = m[Matrix.MtransY];
                PointF curr = new PointF(e.GetX(), e.GetY());

                switch (e.Action)
                {
                    //when one finger is touching
                    //set the mode to DRAG
                    case MotionEventActions.Down:

                        RequestDisallowInterceptTouchEvent(true);

                        last.Set(e.GetX(), e.GetY());
                        start.Set(last);
                        mode = DRAG;

                        break;
                    //when two fingers are touching
                    //set the mode to ZOOM
                    case MotionEventActions.PointerDown:

                        RequestDisallowInterceptTouchEvent(true);

                        last.Set(e.GetX(), e.GetY());
                        start.Set(last);
                        mode = ZOOM;



                        break;
                    //when a finger moves
                    //If mode is applicable move image
                    case MotionEventActions.Move:
                        //if the mode is ZOOM or
                        //if the mode is DRAG and already zoomed
                        if (mode == ZOOM || (mode == DRAG && saveScale > minScale))
                        {

                            float deltaX = curr.X - last.X;// x difference
                            float deltaY = curr.Y - last.Y;// y difference
                            float scaleWidth = (float)Math.Round(origWidth * saveScale);// width after applying current scale
                            float scaleHeight = (float)Math.Round(origHeight * saveScale);// height after applying current scale
                                                                                          //if scaleWidth is smaller than the views width
                                                                                          //in other words if the image width fits in the view
                                                                                          //limit left and right movement
                            if (scaleWidth < width)
                            {
                                deltaX = 0;
                                if (y + deltaY > 0)
                                    deltaY = -y;
                                else if (y + deltaY < -bottom)
                                    deltaY = -(y + bottom);
                            }
                            //if scaleHeight is smaller than the views height
                            //in other words if the image height fits in the view
                            //limit up and down movement
                            else if (scaleHeight < height)
                            {
                                deltaY = 0;
                                if (x + deltaX > 0)
                                    deltaX = -x;
                                else if (x + deltaX < -right)
                                    deltaX = -(x + right);
                            }
                            //if the image doesnt fit in the width or height
                            //limit both up and down and left and right
                            else
                            {
                                if (x + deltaX > 0)
                                    deltaX = -x;
                                else if (x + deltaX < -right)
                                    deltaX = -(x + right);

                                if (y + deltaY > 0)
                                    deltaY = -y;
                                else if (y + deltaY < -bottom)
                                    deltaY = -(y + bottom);
                            }
                            //move the image with the matrix
                            matrix.PostTranslate(deltaX, deltaY);
                            //set the last touch location to the current
                            last.Set(curr.X, curr.Y);
                        }
                        else RequestDisallowInterceptTouchEvent(false);
                        break;
                    //first finger is lifted
                    case MotionEventActions.Up:
                        mode = NONE;
                        int xDiff = (int)Math.Abs(curr.X - start.X);
                        int yDiff = (int)Math.Abs(curr.Y - start.Y);
                        if (xDiff < CLICK && yDiff < CLICK)
                            PerformClick();

                        RequestDisallowInterceptTouchEvent(false);
                        break;
                    // second finger is lifted
                    case MotionEventActions.PointerUp:
                        mode = NONE;

                        RequestDisallowInterceptTouchEvent(false);
                        break;
                }
                Control.ImageMatrix = (matrix);
                Control.Invalidate();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return false;
        }


        public void setImageBitmap(Bitmap bm)
        {
            Control.SetImageBitmap(bm);

            bmWidth = bm.Width;
            bmHeight = bm.Height;
        }
        public void SetImageUri(string uri)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                var mIcon_val = new DownloadImageTask().Execute(uri).GetResult();
                if (mIcon_val != null)
                {
                    setImageBitmap(mIcon_val);
                }


            }

        }


        public void setMaxZoom(float x)
        {
            maxScale = x;
        }


    }
    public class DownloadImageTask : AsyncTask<string, object, Bitmap>
    {

        protected override Bitmap RunInBackground(params string[] @params)
        {
            string urldisplay = @params[0];
            Bitmap mIcon11 = null;
            try
            {
                var ipn = new Java.Net.URL(urldisplay).OpenStream();
                mIcon11 = BitmapFactory.DecodeStream(ipn);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return mIcon11;
        }



    }

    public class DoubleTapListener : GestureDetector.SimpleOnGestureListener
    {
        Action<MotionEvent> action;
        public DoubleTapListener(Action<MotionEvent> action)
        {
            this.action = action;
        }
        public override bool OnDoubleTap(MotionEvent e)
        {
            action?.Invoke(e);
            return true;
        }
    }


}
