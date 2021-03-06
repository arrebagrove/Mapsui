using Android.Graphics;
using Java.Lang;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using System.Diagnostics;
using AndroidBitmap = Android.Graphics.Bitmap;
using AndroidColor = Android.Graphics.Color;
using AndroidGraphics = Android.Graphics;
using Exception = System.Exception;

namespace Mapsui.Rendering.Android
{
    static class RasterRenderer
    {
        public static void Draw(Canvas canvas, IViewport viewport, IStyle style, IFeature feature)
        {
            try
            {
                if (!feature.RenderedGeometry.ContainsKey(style)) feature.RenderedGeometry[style] = ToAndroidBitmap(feature.Geometry);
                var bitmap = (AndroidGraphics.Bitmap)feature.RenderedGeometry[style];
                
                var dest = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
                dest = new BoundingBox(
                    dest.MinX,
                    dest.MinY,
                    dest.MaxX,
                    dest.MaxY);

                var destination = RoundToPixel(dest);
                using (var paint = new Paint())
                {
                    canvas.DrawBitmap(bitmap, new Rect(0, 0, bitmap.Width, bitmap.Height), destination, paint);
                }

                DrawOutline(canvas, style, destination);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

        }

        private static void DrawOutline(Canvas canvas, IStyle style, RectF destination)
        {
            var vectorStyle = (style as VectorStyle);
            if (vectorStyle == null) return;
            if (vectorStyle.Outline == null) return;
            if (vectorStyle.Outline.Color == null) return;
            DrawRectangle(canvas, destination, vectorStyle.Outline.Color);
        }

        private static BoundingBox WorldToScreen(IViewport viewport, BoundingBox boundingBox)
        {
            var first = viewport.WorldToScreen(boundingBox.Min);
            var second = viewport.WorldToScreen(boundingBox.Max);
            return new BoundingBox
                (
                    Math.Min(first.X, second.X),
                    Math.Min(first.Y, second.Y),
                    Math.Max(first.X, second.X),
                    Math.Max(first.Y, second.Y)
                );
        }

        public static RectF RoundToPixel(BoundingBox dest)
        {
            return new RectF(
                Math.Round(dest.Left),
                Math.Round(Math.Min(dest.Top, dest.Bottom)),
                Math.Round(dest.Right),
                Math.Round(Math.Max(dest.Top, dest.Bottom)));
        }

        private static void DrawRectangle(Canvas canvas, RectF destination, Styles.Color outlineColor)
        {
            var paint = new Paint();
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = new AndroidColor(outlineColor.R, outlineColor.G, outlineColor.B, outlineColor.A);
            paint.StrokeWidth = 4;
            canvas.DrawRect(destination, paint);
        }

        private static AndroidBitmap ToAndroidBitmap(IGeometry geometry)
        {
            var raster = (IRaster)geometry;
            var rasterData = raster.Data.ToArray();
            var bitmap = BitmapFactory.DecodeByteArray(rasterData, 0, rasterData.Length);
            return bitmap;
        }
    }
}