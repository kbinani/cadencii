/*
 * FileWaveReceiver.cs
 * Copyright © 2010-2011 kbinani
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
package com.github.cadencii;

import java.util.*;
import com.github.cadencii.*;
import com.github.cadencii.media.*;
#else
using System;
using com.github.cadencii;
using com.github.cadencii.java.util;
using com.github.cadencii.media;

namespace com.github.cadencii
{
#endif

#if JAVA
    public class FileWaveReceiver extends WaveUnit implements WaveReceiver {
#else
    public class FileWaveReceiver : WaveUnit, WaveReceiver
    {
#endif
        private const int BUFLEN = 1024;
        private WaveWriter mAdapter = null;
        private double[] mBufferL = new double[BUFLEN];
        private double[] mBufferR = new double[BUFLEN];
        private double[] mBuffer2L = new double[BUFLEN];
        private double[] mBuffer2R = new double[BUFLEN];
        private WaveReceiver mReceiver = null;
        private int mVersion = 0;
        private String mPath;
        private int mChannel;
        private int mBitPerSample;
        private Object mSyncRoot = new Object();

        public FileWaveReceiver( String path, int channel, int bit_per_sample, int sample_rate )
        {
            mPath = path;
            mChannel = channel;
            mBitPerSample = bit_per_sample;
            try{
                mAdapter = new WaveWriter( mPath, mChannel, mBitPerSample, sample_rate );
            }catch( Exception ex ){
#if JAVA
                ex.printStackTrace();
                mAdapter = null;
#endif
            }
        }

        public override void setGlobalConfig( EditorConfig config )
        {
            // do nothing
        }

        public override void setConfig( String parameter )
        {
            // do nothing
        }

        /// <summary>
        /// 初期化メソッド．
        /// </summary>
        /// <param name="parameter"></param>
        public void init( String parameter )
        {
        }

        public override int getVersion()
        {
            return mVersion;
        }

        public void end()
        {
            lock ( mSyncRoot ) {
#if DEBUG
                if ( mAdapter == null ) {
                    sout.println( "FileWaveReceiver#end; warning; 'end' when mAdapter is null" );
                }
#endif
                if ( mAdapter != null ) {
                    mAdapter.close();
                }
                if ( mReceiver != null ) {
                    mReceiver.end();
                }
            }
        }

        public void push( double[] l, double[] r, int length )
        {
            lock ( mSyncRoot ) {
                mAdapter.append( l, r, length );
                if ( mReceiver != null ) {
                    mReceiver.push( l, r, length );
                }
            }
        }

        public void setReceiver( WaveReceiver r )
        {
            if ( mReceiver != null ) {
                mReceiver.end();
            }
            mReceiver = r;
        }
    }

#if !JAVA
}
#endif
