/*
 * WaveDrawContext.cs
 * Copyright © 2009-2011 kbinani
 *
 * This file is part of org.kbinani.cadencii.
 *
 * org.kbinani.cadencii is free software; you can redistribute it and/or
 * modify it under the terms of the GPLv3 License.
 *
 * org.kbinani.cadencii is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 */
#if JAVA
package org.kbinani.editotoini;

import java.awt.*;
import org.kbinani.*;
import org.kbinani.apputil.*;
import org.kbinani.media.*;
#else
using System;
using org.kbinani.apputil;
using org.kbinani.java.awt;
using org.kbinani.media;

namespace org.kbinani.editotoini {
    using boolean = System.Boolean;
#endif

    /// <summary>
    /// WAVEファイルのデータをグラフィクスに書き込む操作を行うクラス
    /// </summary>
#if JAVA
    public class WaveDrawContext{
#else
    public class WaveDrawContext : IDisposable {
#endif
        private byte[] m_wave;
        private int m_sample_rate = 44100;
        private String m_name;
        //public UtauFreq Freq;
        private float m_length;
        private PolylineDrawer drawer = null;

        /// <summary>
        /// 読み込むWAVEファイルを指定したコンストラクタ。初期化と同時にWAVEファイルの読込みを行います。
        /// </summary>
        /// <param name="file">読み込むWAVEファイルのパス</param>
        public WaveDrawContext( String file ) {
            load( file );
            drawer = new PolylineDrawer( null, 1024 );
        }

        /// <summary>
        /// デフォルトのコンストラクタ。
        /// </summary>
        public WaveDrawContext() {
            m_wave = new byte[0];
            m_length = 0.0f;
            drawer = new PolylineDrawer( null, 1024 );
        }

        /// <summary>
        /// 保持しているWAVEデータを破棄します。
        /// </summary>
        public void unload() {
            drawer.clear();
            m_wave = new byte[0];
            m_length = 0.0f;
        }

        public void reloadPartial( String file, double sec_from, double sec_to ) {
            if ( !PortUtil.isFileExists( file ) ) {
                return;
            }

            WaveRateConverter wr = null;
            try {
                wr = new WaveRateConverter( new WaveReader( file ), m_sample_rate );
                int saFrom = (int)(sec_from * m_sample_rate);
                int saTo = (int)(sec_to * m_sample_rate);
                int oldLength = m_wave.Length;
                if ( oldLength < saTo ) {
#if JAVA
                    m_wave = Arrays.copyOf( m_wave, saTo );
#else
                    Array.Resize( ref m_wave, saTo );
#endif
                    saFrom = oldLength;
                }
                int buflen = 1024;
                double[] left = new double[buflen];
                double[] right = new double[buflen];
                int remain = saTo - saFrom;
                int pos = saFrom;
                while ( remain > 0 ) {
                    int delta = remain > buflen ? buflen : remain;
                    wr.read( pos, delta, left, right );

                    for ( int i = 0; i < delta; i++ ) {
                        double d = (left[i] + right[i]) * 0.5;
                        byte b = (byte)((d + 1.0) * 0.5 * 127);
                        m_wave[pos + i] = b;
                    }

                    pos += delta;
                    remain -= delta;
                }
                left = null;
                right = null;
            } catch ( Exception ex ) {
                serr.println( "WaveDrawContext#reloadPartial; ex=" + ex );
            } finally {
                if ( wr != null ) {
                    try {
                        wr.close();
                    } catch ( Exception ex2 ) {
                        serr.println( "WaveDrawContext#reloadPartial; ex2=" + ex2 );
                    }
                }
            }
        }

        /// <summary>
        /// WAVEファイルを読み込みます。
        /// </summary>
        /// <param name="file">読み込むWAVEファイルのパス</param>
        public void load( String file ) {
            if ( !PortUtil.isFileExists( file ) ) {
                m_wave = new byte[0];
                m_length = 0.0f;
                return;
            }

            Wave wr = null;
            try {
                wr = new Wave( file );
                m_wave = new byte[(int)wr.getTotalSamples()];
                m_sample_rate = (int)wr.getSampleRate();
                m_length = wr.getTotalSamples() / (float)wr.getSampleRate();
                int count = (int)wr.getTotalSamples();
                for ( int i = 0; i < count; i++ ) {
                    double b = wr.getDouble( (int)i );
                    m_wave[i] = (byte)((b + 1.0) * 0.5 * 127.0);
                }
            } catch ( Exception ex ) {
            } finally {
                if ( wr != null ) {
                    try {
                        wr.dispose();
                    } catch ( Exception ex2 ) {
                    }
                }
            }
            if ( m_wave == null ) {
                m_wave = new byte[0];
                m_sample_rate = 44100;
                m_length = 0.0f;
            }
        }

        /// <summary>
        /// このWAVE描画コンテキストの名前を取得します。
        /// </summary>
        /// <returns>この描画コンテキストの名前</returns>
        public String getName() {
            return m_name;
        }

        /// <summary>
        /// このWAVE描画コンテキストの名前を設定します。
        /// </summary>
        /// <param name="value">この描画コンテキストの名前</param>
        public void setName( String value ) {
            m_name = value;
        }

        /// <summary>
        /// このWAVE描画コンテキストが保持しているWAVEデータの、秒数を取得します。
        /// </summary>
        /// <returns>保持しているWAVEデータの長さ(秒)</returns>
        public float getLength() {
            return m_length;
        }

#if !JAVA
        /// <summary>
        /// デストラクタ。disposeメソッドを呼び出します。
        /// </summary>
        ~WaveDrawContext() {
            dispose();
        }
#endif

#if !JAVA
        /// <summary>
        /// このWAVE描画コンテキストが使用しているリソースを開放します。
        /// </summary>
        public void Dispose(){
            dispose();
        }
#endif

        /// <summary>
        /// このWAVE描画コンテキストが使用しているリソースを開放します。
        /// </summary>
        public void dispose() {
            m_wave = null;
#if JAVA
            System.gc();
#else
            GC.Collect();
#endif
        }

        /// <summary>
        /// このWAVE描画コンテキストが保持しているWAVEデータを、秒基準でグラフィクスに描画します。
        /// </summary>
        /// <param name="g">描画に使用するグラフィクスオブジェクト</param>
        /// <param name="pen">描画に使用するペン</param>
        /// <param name="rect">描画範囲</param>
        /// <param name="sec_start">描画開始位置の秒時</param>
        /// <param name="sec_end">描画終了位置の秒時</param>
        public void draw( Graphics2D g, Color pen, Rectangle rect, float sec_start, float sec_end ) {
            int start0 = (int)(sec_start * m_sample_rate) - 1;
            int end = (int)(sec_end * m_sample_rate) + 1;

            int width = rect.width;
            int height = rect.height;
            int ox = rect.x;
            int oy = rect.y + height;
            float order_y = rect.height / 127.0f;
            float order_x = rect.width / (float)(sec_end - sec_start) / (float)m_sample_rate;

            int start = start0;
            if ( start < 0 ) {
                start = 0;
            }
            if ( m_wave.Length < end ) {
                end = m_wave.Length - 1;
            }

            byte last = 0x0;
            if ( m_wave == null || (m_wave != null && m_wave.Length <= 0) ) {
                return;
            }
            last = m_wave[0];
            int lastx = ox;
            int lasty = oy - (int)(last * order_y);
            boolean drawn = false;
            g.setColor( pen );
            for ( int i = start + 1; i <= end; i++ ) {
                byte v = m_wave[i];
                if ( v != last ) {
                    drawn = true;
                    int x = ox + (int)((i - start0) * order_x);
                    int y = oy - (int)(v * order_y);
                    g.drawLine( lastx, lasty, x, lasty );
                    g.drawLine( x, lasty, x, y );
                    lastx = x;
                    lasty = y;
                    last = v;
                }
            }
            if ( !drawn ) {
                g.drawLine( rect.x, lasty, rect.x + rect.width, lasty );
            }
        }
    }

#if !JAVA
}
#endif
