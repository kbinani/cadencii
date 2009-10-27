﻿/*
 * awt.cs
 * Copyright (c) 2009 kbinani
 *
 * This file is part of bocoree.
 *
 * bocoree is free software; you can redistribute it and/or
 * modify it under the terms of the BSD License.
 *
 * bocoree is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 */
#if !JAVA
namespace bocoree.awt {

    public class Graphics2D {
        public System.Drawing.Graphics nativeGraphics;
        public Color color = Color.black;
        private BasicStroke m_stroke = new BasicStroke();
        public System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush( System.Drawing.Color.Black );
        private System.Drawing.Font m_font = new System.Drawing.Font( "Arial", 10 );

        public Graphics2D( System.Drawing.Graphics g ) {
            nativeGraphics = g;
        }

        public void clearRect( int x, int y, int width, int height ) {
            nativeGraphics.FillRectangle( System.Drawing.Brushes.White, x, y, width, height );
        }

        public void drawLine( int x1, int y1, int x2, int y2 ) {
            nativeGraphics.DrawLine( m_stroke.pen, x1, y1, x2, y2 );
        }

        public void drawRect( int x, int y, int width, int height ) {
            nativeGraphics.DrawRectangle( m_stroke.pen, x, y, width, height );
        }

        public void fillRect( int x, int y, int width, int height ) {
            nativeGraphics.FillRectangle( brush, x, y, width, height );
        }

        public void drawOval( int x, int y, int width, int height ) {
            nativeGraphics.DrawEllipse( m_stroke.pen, x, y, width, height );
        }

        public void fillOval( int x, int y, int width, int height ) {
            nativeGraphics.FillRectangle( brush, x, y, width, height );
        }

        public void setColor( Color c ) {
            color = c;
            m_stroke.pen.Color = c.color;
            brush.Color = c.color;
        }

        public Color getColor() {
            return color;
        }

        public void setFont( Font font ) {
            m_font = font.font;
        }

        public void drawString( string str, float x, float y ) {
            nativeGraphics.DrawString( str, m_font, brush, x, y );
        }

        public void drawPolygon( Polygon p ) {
            drawPolygon( p.xpoints, p.ypoints, p.npoints );
        }

        public void drawPolygon( int[] xPoints, int[] yPoints, int nPoints ) {
            System.Drawing.Point[] points = new System.Drawing.Point[nPoints];
            for ( int i = 0; i < nPoints; i++ ) {
                points[i] = new System.Drawing.Point( xPoints[i], yPoints[i] );
            }
            nativeGraphics.DrawLines( m_stroke.pen, points );
        }

        public void fillPolygon( Polygon p ) {
            fillPolygon( p.xpoints, p.ypoints, p.npoints );
        }

        public void fillPolygon( int[] xPoints, int[] yPoints, int nPoints ) {
            System.Drawing.Point[] points = new System.Drawing.Point[nPoints];
            for ( int i = 0; i < nPoints; i++ ) {
                points[i] = new System.Drawing.Point( xPoints[i], yPoints[i] );
            }
            nativeGraphics.FillPolygon( brush, points );
        }

        public void setStroke( Stroke stroke ) {
            if ( stroke is BasicStroke ) {
                BasicStroke bstroke = (BasicStroke)stroke;
                m_stroke.pen = bstroke.pen;
                m_stroke.pen.Color = color.color;
            }
        }

        public Stroke getStroke() {
            return m_stroke;
        }

        public Shape getClip() {
            Shape ret = new Shape();
            ret.region = nativeGraphics.Clip;
            return ret;
        }

        public void setClip( Shape clip ) {
            if ( clip == null ) {
                nativeGraphics.Clip = new System.Drawing.Region();
            } else {
                nativeGraphics.Clip = clip.region;
            }
        }

        public void clipRect( int x, int y, int width, int height ) {
            nativeGraphics.Clip = new System.Drawing.Region( new System.Drawing.Rectangle( x, y, width, height ) );
        }

        public void drawImage( Image img, int x, int y, object obs ) {
            if ( img is bocoree.awt.image.BufferedImage ) {
                nativeGraphics.DrawImage( ((bocoree.awt.image.BufferedImage)img).m_image, new System.Drawing.Point( x, y ) );
            }
        }
    }

    public interface Image{
        int getHeight( object observer );
        int getWidth( object observer );
    }

    public struct Color {
        /// <summary>
        /// 黒を表します。
        /// </summary>
        public static Color black = new Color( System.Drawing.Color.Black );
        /// <summary>
        /// 黒を表します。
        /// </summary>
        public static Color BLACK = new Color( System.Drawing.Color.Black );
        /// <summary>
        /// 青を表します。
        /// </summary>
        public static Color blue = new Color( System.Drawing.Color.Blue );
        /// <summary>
        /// 青を表します。
        /// </summary>
        public static Color BLUE = new Color( System.Drawing.Color.Blue );
        /// <summary>
        /// シアンを表します。
        /// </summary>
        public static Color cyan = new Color( System.Drawing.Color.Cyan );
        /// <summary>
        /// シアンを表します。
        /// </summary>
        public static Color CYAN = new Color( System.Drawing.Color.Cyan );
        /// <summary>
        /// ダークグレイを表します。
        /// </summary>
        public static Color DARK_GRAY = new Color( System.Drawing.Color.DarkGray );
        /// <summary>
        /// ダークグレイを表します。
        /// </summary>
        public static Color darkGray = new Color( System.Drawing.Color.DarkGray );
        /// <summary>
        /// グレイを表します。
        /// </summary>
        public static Color gray = new Color( System.Drawing.Color.Gray );
        /// <summary>
        /// グレイを表します。
        /// </summary>
        public static Color GRAY = new Color( System.Drawing.Color.Gray );
        /// <summary>
        /// 緑を表します。
        /// </summary>
        public static Color green = new Color( System.Drawing.Color.Green );
        /// <summary>
        /// 緑を表します。
        /// </summary>
        public static Color GREEN = new Color( System.Drawing.Color.Green );
        /// <summary>
        /// ライトグレイを表します。
        /// </summary>
        public static Color LIGHT_GRAY = new Color( System.Drawing.Color.LightGray );
        /// <summary>
        /// ライトグレイを表します。
        /// </summary>
        public static Color lightGray = new Color( System.Drawing.Color.LightGray );
        /// <summary>
        /// マゼンタを表します。
        /// </summary>
        public static Color magenta = new Color( System.Drawing.Color.Magenta );
        /// <summary>
        /// マゼンタを表します。
        /// </summary>
        public static Color MAGENTA = new Color( System.Drawing.Color.Magenta );
        /// <summary>
        /// オレンジを表します。
        /// </summary>
        public static Color orange = new Color( System.Drawing.Color.Orange );
        /// <summary>
        /// オレンジを表します。
        /// </summary>
        public static Color ORANGE = new Color( System.Drawing.Color.Orange );
        /// <summary>
        /// ピンクを表します。
        /// </summary>
        public static Color pink = new Color( System.Drawing.Color.Pink );
        /// <summary>
        /// ピンクを表します。
        /// </summary>
        public static Color PINK = new Color( System.Drawing.Color.Pink );
        /// <summary>
        /// 赤を表します。
        /// </summary>
        public static Color red = new Color( System.Drawing.Color.Red );
        /// <summary>
        /// 赤を表します。
        /// </summary>
        public static Color RED = new Color( System.Drawing.Color.Red );
        /// <summary>
        /// 白を表します。
        /// </summary>
        public static Color white = new Color( System.Drawing.Color.White );
        /// <summary>
        /// 白を表します。
        /// </summary>
        public static Color WHITE = new Color( System.Drawing.Color.White );
        /// <summary>
        /// 黄を表します。
        /// </summary>
        public static Color yellow = new Color( System.Drawing.Color.Yellow );
        /// <summary>
        /// 黄を表します。 
        /// </summary>
        public static Color YELLOW = new Color( System.Drawing.Color.Yellow );

        public System.Drawing.Color color;

        public Color( System.Drawing.Color value ) {
            color = value;
        }

        public Color( int r, int g, int b ) {
            color = System.Drawing.Color.FromArgb( r, g, b );
        }

        public Color( int r, int g, int b, int a ) {
            color = System.Drawing.Color.FromArgb( a, r, g, b );
        }

        public int getRed() {
            return color.R;
        }

        public int getGreen() {
            return color.G;
        }

        public int getBlue() {
            return color.B;
        }
    }

    public struct Rectangle {
        public int height;
        public int width;
        public int x;
        public int y;

        public Rectangle( int width_, int height_ ) {
            x = 0;
            y = 0;
            width = width_;
            height = height_;
        }

        public Rectangle( int x_, int y_, int width_, int height_ ) {
            x = x_;
            y = y_;
            width = width_;
            height = height_;
        }

        public Rectangle( Rectangle r ) {
            x = r.x;
            y = r.y;
            width = r.width;
            height = r.height;
        }
    }

    public struct Point {
        public int x;
        public int y;

        public Point( int x_, int y_ ) {
            x = x_;
            y = y_;
        }

        public Point( Point p ) {
            x = p.x;
            y = p.y;
        }
    }

    public class Font {
        public const int PLAIN = 0;
        public const int ITALIC = 2;
        public const int BOLD = 1;
        public static readonly string DIALOG = "Dialog";
        public static readonly string DIALOG_INPUT = "DialogInput";
        public static readonly string MONOSPACED = "Monospaced";
        public static readonly string SANS_SERIF = "SansSerif";
        public static readonly string SERIF = "Serif";
        public System.Drawing.Font font;

        public Font( System.Drawing.Font value ) {
            font = value;
        }

        public Font( string name, int style, int size ) {
            System.Drawing.FontStyle fstyle = System.Drawing.FontStyle.Regular;
            if ( style >= Font.BOLD ) {
                fstyle = fstyle | System.Drawing.FontStyle.Bold;
            }
            if ( style >= Font.ITALIC ) {
                fstyle = fstyle | System.Drawing.FontStyle.Italic;
            }
            font = new System.Drawing.Font( name, size, fstyle );
        }

        public string getName() {
            return font.Name;
        }

        public int getSize() {
            return (int)font.SizeInPoints;
        }

        public float getSize2D() {
            return font.SizeInPoints;
        }
    }

    public interface Stroke {
    }

    public class BasicStroke : Stroke {
        public const int CAP_BUTT = 0;
        public const int CAP_ROUND = 1;
        public const int CAP_SQUARE = 2;
        public const int JOIN_BEVEL = 2;
        public const int JOIN_MITER = 0;
        public const int JOIN_ROUND = 1;
        public System.Drawing.Pen pen;

        public BasicStroke() {
            pen = new System.Drawing.Pen( System.Drawing.Color.Black );
        }

        public BasicStroke( float width )
            : this( width, 0, 0, 10.0f ) {
        }

        public BasicStroke( float width, int cap, int join )
            : this( width, cap, join, 10.0f ) {
        }

        public BasicStroke( float width, int cap, int join, float miterlimit ) {
            pen = new System.Drawing.Pen( System.Drawing.Color.Black, width );
            System.Drawing.Drawing2D.LineCap linecap = System.Drawing.Drawing2D.LineCap.Flat;
            if ( cap == 1 ) {
                linecap = System.Drawing.Drawing2D.LineCap.Round;
            } else if ( cap == 2 ) {
                linecap = System.Drawing.Drawing2D.LineCap.Square;
            }
            pen.StartCap = linecap;
            pen.EndCap = linecap;
            System.Drawing.Drawing2D.LineJoin linejoin = System.Drawing.Drawing2D.LineJoin.Miter;
            if ( join == 1 ) {
                linejoin = System.Drawing.Drawing2D.LineJoin.Round;
            } else if ( join == 2 ) {
                linejoin = System.Drawing.Drawing2D.LineJoin.Bevel;
            }
            pen.LineJoin = linejoin;
            pen.MiterLimit = miterlimit;
        }

        public BasicStroke( float width, int cap, int join, float miterlimit, float[] dash, float dash_phase )
            : this( width, cap, join, miterlimit ) {
            pen.DashPattern = dash;
            pen.DashOffset = dash_phase;
        }
    }

    public class Polygon {
        /// <summary>
        /// 点の総数です。
        /// </summary>
        public int npoints;
        /// <summary>
        /// X 座標の配列です。
        /// </summary>
        public int[] xpoints;
        /// <summary>
        /// Y 座標の配列です。
        /// </summary>
        public int[] ypoints;

        public Polygon() {
            npoints = 0;
            xpoints = new int[0];
            ypoints = new int[0];
        }

        public Polygon( int[] xpoints_, int[] ypoints_, int npoints_ ) {
            npoints = npoints_;
            xpoints = xpoints_;
            ypoints = ypoints_;
        }
    }

    public class Shape {
        public System.Drawing.Region region;
    }

    public struct Dimension {
        public int height;
        public int width;

        public Dimension( int width_, int height_ ) {
            width = width_;
            height = height_;
        }
    }

    public class Frame : System.Windows.Forms.Form {
    }

}
#endif