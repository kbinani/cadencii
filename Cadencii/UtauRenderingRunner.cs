﻿/*
 * UtauRenderingRunner.cs
 * Copyright (c) 2009 kbinani
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
package org.kbinani.cadencii;

import java.io.*;
import java.util.*;
import org.kbinani.*;
import org.kbinani.media.*;
import org.kbinani.vsq.*;
#else
using System;
using System.Diagnostics;
using org.kbinani.media;
using org.kbinani.vsq;
using bocoree;
using bocoree.java.io;
using bocoree.java.util;

namespace org.kbinani.cadencii {
    using boolean = System.Boolean;
    using Integer = System.Int32;
#endif

#if JAVA
    public class UtauRenderingRunner extends RenderingRunner{
#else
    public class UtauRenderingRunner : RenderingRunner {
#endif
        public const String FILEBASE = "temp.wav";
        private const int MAX_CACHE = 512;
        private static TreeMap<String, ValuePair<String, Double>> s_cache = new TreeMap<String, ValuePair<String, Double>>();
        
        private Vector<RenderQueue> m_resampler_queue = new Vector<RenderQueue>();
        private double[] m_left;
        private double[] m_right;
        private double m_progress = 0.0;

        VsqFileEx m_vsq;
        Vector<SingerConfig> m_singer_config_sys;
        String m_resampler;
        String m_wavtool;
        String m_temp_dir;
        boolean m_invoke_with_wine;
        boolean m_mode_infinite;
        double m_started_date;
        double m_running_rate;

        public UtauRenderingRunner( VsqFileEx vsq,
                                    int track_,
                                    Vector<SingerConfig> singer_config_sys,
                                    String resampler,
                                    String wavtool,
                                    String temp_dir,
                                    boolean invoke_with_wine,
                                    int sample_rate,
                                    int trim_msec,
                                    long total_samples,
                                    boolean mode_infinite,
                                    WaveWriter wave_writer,
                                    double wave_read_offset_seconds,
                                    Vector<WaveReader> reader,
                                    boolean direct_play,
                                    boolean reflect_amp_to_wave )
#if JAVA
        {
#else
            :
#endif
            base( track_, reflect_amp_to_wave, wave_writer, wave_read_offset_seconds, reader, direct_play, trim_msec, total_samples, sample_rate )
#if JAVA
            ;
#else
        {
#endif
            m_vsq = vsq;
            m_singer_config_sys = singer_config_sys;
            m_resampler = resampler;
            m_wavtool = wavtool;
            m_temp_dir = temp_dir;
            m_invoke_with_wine = invoke_with_wine;
            m_mode_infinite = mode_infinite;
        }

        public static void clearCache() {
            for ( Iterator itr = s_cache.keySet().iterator(); itr.hasNext(); ){
                String key = (String)itr.next();
                ValuePair<String, Double> value = s_cache.get( key );
                String file = value.getKey();
                try {
                    PortUtil.deleteFile( file );
                } catch ( Exception ex ) {
                    PortUtil.stderr.println( "UtauRenderingRunner#clearCache; ex=" + ex );
                }
            }
            s_cache.clear();
        }

        public override double getProgress() {
            return m_progress;
        }

        public override double getElapsedSeconds() {
            return PortUtil.getCurrentTime() - m_started_date;
        }

        public override void run() {
#if DEBUG
            PortUtil.println( "UtauRenderingRunner#run" );
#endif
            m_started_date = PortUtil.getCurrentTime();
            m_rendering = true;
#if MAKEBAT_SP
            StreamWriter bat = null;
            StreamWriter log = null;
#endif
            try {
                double sample_length = m_vsq.getSecFromClock( m_vsq.TotalClocks ) * sampleRate;
                m_abort_required = false;
                m_progress = 0.0;
                if ( !PortUtil.isDirectoryExists( m_temp_dir ) ) {
                    PortUtil.createDirectory( m_temp_dir );
                }

#if MAKEBAT_SP
                log = new StreamWriter( Path.Combine( m_temp_dir, "UtauRenderingRunner.log" ), false, Encoding.GetEncoding( "Shift_JIS" ) );
#endif
                // 原音設定を読み込み
                TreeMap<Integer, UtauVoiceDB> config = new TreeMap<Integer, UtauVoiceDB>();
                Vector<SingerConfig> singers = m_singer_config_sys;
                VsqTrack target = m_vsq.Track.get( renderingTrack );
#if MAKEBAT_SP
                log.WriteLine( "reading voice db. configs..." );
#endif
                for ( int pc = 0; pc < singers.size(); pc++ ) {
                    String singer_name = singers.get( pc ).VOICENAME;
                    String singer_path = singers.get( pc ).VOICEIDSTR;

                    //TODO: mono on linuxにて、singer_pathが取得できていない？
                    String config_file = PortUtil.combinePath( singer_path, "oto.ini" );
                    UtauVoiceDB db = new UtauVoiceDB( config_file );
#if MAKEBAT_SP
                    log.Write( "    #" + pc + "; PortUtil.isFileExists( oto.ini )=" + PortUtil.isFileExists( config_file ) );
                    log.WriteLine( "; name=" + db.getName() );
#endif
                    config.put( pc, db );
                }
#if MAKEBAT_SP
                log.WriteLine( "...done" );
#endif

                String file = PortUtil.combinePath( m_temp_dir, FILEBASE );
                if ( PortUtil.isFileExists( file ) ) {
                    PortUtil.deleteFile( file );
                }
                String file_whd = PortUtil.combinePath( m_temp_dir, FILEBASE + ".whd" );
                if ( PortUtil.isFileExists( file_whd ) ) {
                    PortUtil.deleteFile( file_whd );
                }
                String file_dat = PortUtil.combinePath( m_temp_dir, FILEBASE + ".dat" );
                if ( PortUtil.isFileExists( file_dat ) ) {
                    PortUtil.deleteFile( file_dat );
                }
#if DEBUG
                AppManager.debugWriteLine( "temp_dir=" + m_temp_dir );
                AppManager.debugWriteLine( "file_whd=" + file_whd );
                AppManager.debugWriteLine( "file_dat=" + file_dat );
#endif

                int count = -1;
                double sec_end = 0;
                double sec_end_old = 0;
                int program_change = 0;
                m_resampler_queue.clear();

                // 前後の音符の先行発音やオーバーラップやらを取得したいので、一度リストに格納する
                Vector<VsqEvent> events = new Vector<VsqEvent>();
                for ( Iterator itr = target.getNoteEventIterator(); itr.hasNext(); ) {
                    events.add( (VsqEvent)itr.next() );
                }
                
#if MAKEBAT_SP
                log.WriteLine( "making resampler queue..." );
#endif
                int events_count = events.size();
                for ( int k = 0; k < events_count; k++ ) {
                    VsqEvent item = events.get( k );
#if MAKEBAT_SP
                    log.Write( "    #" + k + "; clock=" + item.Clock );
#endif
                    VsqEvent singer_event = target.getSingerEventAt( item.Clock );
                    if ( singer_event == null ) {
                        program_change = 0;
                    } else {
                        program_change = singer_event.ID.IconHandle.Program;
                    }
#if MAKEBAT_SP
                    log.Write( "; pc=" + program_change );
#endif
                    if ( m_abort_required ) {
                        m_rendering = false;
                        return;
                    }
                    count++;
                    double sec_start = m_vsq.getSecFromClock( item.Clock );
                    double sec_start_act = sec_start - item.UstEvent.PreUtterance / 1000.0;
                    sec_end_old = sec_end;
                    sec_end = m_vsq.getSecFromClock( item.Clock + item.ID.getLength() );
                    double sec_end_act = sec_end;
                    VsqEvent item_next = null;
                    if ( k + 1 < events_count ) {
                        item_next = events.get( k + 1 );
                    }
                    if ( item_next != null ) {
                        double sec_start_act_next = m_vsq.getSecFromClock( item_next.Clock ) - item_next.UstEvent.PreUtterance / 1000.0 + item_next.UstEvent.VoiceOverlap / 1000.0;
                        if ( sec_start_act_next < sec_end_act ) {
                            sec_end_act = sec_start_act_next;
                        }
                    }
                    float t_temp = (float)(item.ID.getLength() / (sec_end - sec_start) / 8.0);
                    if ( (count == 0 && sec_start > 0.0) || (sec_start > sec_end_old) ) {
                        double sec_start2 = sec_end_old;
                        double sec_end2 = sec_start;
                        float t_temp2 = (float)(item.Clock / (sec_end2 - sec_start2) / 8.0);
                        String singer = "";
                        if ( 0 <= program_change && program_change < singers.size() ) {
                            singer = singers.get( program_change ).VOICEIDSTR;
                        }
                        RenderQueue rq = new RenderQueue();
                        rq.ResamplerArg = "";
                        rq.WavtoolArgPrefix = "\"" + file + "\" \"" + PortUtil.combinePath( singer, "R.wav" ) + "\" 0 " + item.Clock + "@" + PortUtil.formatMessage( "{0:f2}", t_temp2 );
                        rq.WavtoolArgSuffix = " 0 0";
                        rq.Oto = new OtoArgs();
                        rq.FileName = "";
                        rq.secEnd = sec_end2;
                        rq.ResamplerFinished = true;
                        m_resampler_queue.add( rq );
                        count++;
                    }
                    String lyric = item.ID.LyricHandle.L0.Phrase;
                    String note = NoteStringFromNoteNumber( item.ID.Note );
                    int millisec = (int)((sec_end_act - sec_start_act) * 1000) + 50;

                    OtoArgs oa = new OtoArgs();
                    if ( config.containsKey( program_change ) ) {
                        UtauVoiceDB db = config.get( program_change );
                        oa = db.attachFileNameFromLyric( lyric );
                    }
#if MAKEBAT_SP
                    log.Write( "; lyric=" + lyric + "; fileName=" + oa.fileName );
#endif
                    String singer2 = "";
                    if ( 0 <= program_change && program_change < singers.size() ) {
                        singer2 = singers.get( program_change ).VOICEIDSTR;
                    }
                    oa.msPreUtterance = item.UstEvent.PreUtterance;
                    oa.msOverlap = item.UstEvent.VoiceOverlap;
                    RenderQueue rq2 = new RenderQueue();
                    String resampler_arg_prefix = "\"" + PortUtil.combinePath( singer2, lyric + ".wav" ) + "\"";
                    String resampler_arg_suffix = "\"" + note + "\" 100 " + item.UstEvent.Flags + "L" + " " + oa.msOffset + " " + millisec + " " + oa.msConsonant + " " + oa.msBlank + " 100 " + item.UstEvent.Moduration;

                    // ピッチを取得
                    String pitch = "";
                    boolean allzero = true;
                    int delta_clock = 5;  //ピッチを取得するクロック間隔
                    int tempo = 120;
                    double delta_sec = delta_clock / (8.0 * tempo); //ピッチを取得する時間間隔
                    if ( item.ID.VibratoHandle == null ) {
                        int pit_count = (int)((sec_end_act - sec_start_act) / delta_sec) + 1;
                        for ( int i = 0; i < pit_count; i++ ) {
                            double gtime = sec_start_act + delta_sec * i;
                            int clock = (int)m_vsq.getClockFromSec( gtime );
                            float pvalue = (float)target.getPitchAt( clock );
                            if ( pvalue != 0 ) {
                                allzero = false;
                            }
                            pitch += " " + PortUtil.formatDecimal( "0.00", pvalue );
                            if ( i == 0 ) {
                                pitch += "Q" + tempo;
                            }
                        }
                    } else {
                        // ビブラートが始まるまでのピッチを取得
                        double sec_vibstart = m_vsq.getSecFromClock( item.Clock + item.ID.VibratoDelay );
                        int pit_count = (int)((sec_vibstart - sec_start_act) / delta_sec);
                        int totalcount = 0;
                        for ( int i = 0; i < pit_count; i++ ) {
                            double gtime = sec_start_act + delta_sec * i;
                            int clock = (int)m_vsq.getClockFromSec( gtime );
                            float pvalue = (float)target.getPitchAt( clock );
                            pitch += " " + PortUtil.formatDecimal( "0.00", pvalue );
                            if ( totalcount == 0 ) {
                                pitch += "Q" + tempo;
                            }
                            totalcount++;
                        }
                        Vector<PointD> ret = FormMain.getVibratoPoints( m_vsq,
                                                                      item.ID.VibratoHandle.RateBP,
                                                                      item.ID.VibratoHandle.StartRate,
                                                                      item.ID.VibratoHandle.DepthBP,
                                                                      item.ID.VibratoHandle.StartDepth,
                                                                      item.Clock + item.ID.VibratoDelay,
                                                                      item.ID.getLength() - item.ID.VibratoDelay,
                                                                      (float)delta_sec );
                        for ( int i = 0; i < ret.size(); i++ ) {
                            float gtime = (float)ret.get( i ).getX();
                            int clock = (int)m_vsq.getClockFromSec( gtime );
                            float pvalue = (float)target.getPitchAt( clock );
                            pitch += " " + PortUtil.formatDecimal( "0.00", pvalue + ret.get( i ).getY() * 100.0f );
                            if ( totalcount == 0 ) {
                                pitch += "Q" + tempo;
                            }
                            totalcount++;
                        }
                        allzero = totalcount == 0;
                    }

                    //4_あ_C#4_550.wav
                    String filename = PortUtil.combinePath( m_temp_dir, PortUtil.getMD5FromString( s_cache.size() + resampler_arg_prefix + resampler_arg_suffix + pitch ) + ".wav" );

                    rq2.ResamplerArg = resampler_arg_prefix + " \"" + filename + "\" " + resampler_arg_suffix;
                    if ( !allzero ) {
                        rq2.ResamplerArg += pitch;
                    }

                    String search_key = resampler_arg_prefix + resampler_arg_suffix + pitch;
                    boolean exist_in_cache = s_cache.containsKey( search_key );
                    if ( !exist_in_cache ) {
                        if ( s_cache.size() + 1 >= MAX_CACHE ) {
                            double old = PortUtil.getCurrentTime();
                            String delfile = "";
                            String delkey = "";
                            for ( Iterator itr = s_cache.keySet().iterator(); itr.hasNext(); ){
                                String key = (String)itr.next();
                                ValuePair<String, Double> value = s_cache.get( key );
                                if ( old < value.getValue() ) {
                                    old = value.getValue();
                                    delfile = value.getKey();
                                    delkey = key;
                                }
                            }
                            try {
#if DEBUG
                                bocoree.debug.push_log( "deleting... \"" + delfile + "\"" );
#endif
                                PortUtil.deleteFile( delfile );
                            } catch ( Exception ex ) {
                                PortUtil.stderr.println( "UtauRenderingRunner#run; ex=" + ex );
                            }
                            s_cache.remove( delkey );
                        }
                        s_cache.put( search_key, new ValuePair<String, Double>( filename, PortUtil.getCurrentTime() ) );
                    } else {
                        filename = s_cache.get( search_key ).getKey();
                    }

                    rq2.WavtoolArgPrefix = "\"" + file + "\" \"" + filename + "\" 0 " + item.ID.getLength() + "@" + PortUtil.formatMessage( "{0:f2}", t_temp );
                    UstEnvelope env = item.UstEvent.Envelope;
                    if ( env == null ) {
                        env = new UstEnvelope();
                    }
                    rq2.WavtoolArgSuffix = " " + env.p1 + " " + env.p2 + " " + env.p3 + " " + env.v1 + " " + env.v2 + " " + env.v3 + " " + env.v4;
                    rq2.WavtoolArgSuffix += " " + oa.msOverlap + " " + env.p4 + " " + env.p5 + " " + env.v5;
                    rq2.Oto = oa;
                    rq2.FileName = filename;
                    rq2.secEnd = sec_end;
                    rq2.ResamplerFinished = exist_in_cache;
                    m_resampler_queue.add( rq2 );
#if MAKEBAT_SP
                    log.WriteLine();
#endif
                }
#if MAKEBAT_SP
                log.WriteLine( "...done" );
#endif

#if DEBUG
                bocoree.debug.push_log( "s_cache:" );
                for ( Iterator itr = s_cache.keySet().iterator(); itr.hasNext(); ){
                    String key = (String)itr.next();
                    ValuePair<String, Double> value = s_cache.get( key );
                    bocoree.debug.push_log( "    arg=" + key );
                    bocoree.debug.push_log( "    file=" + value.getKey() );
                }
#endif

                int num_queues = m_resampler_queue.size();
                int processed_sample = 0; //WaveIncomingで受け渡した波形の合計サンプル数
                int channel = 0; // .whdに記録されたチャンネル数
                int byte_per_sample = 0;
                // 引き続き、wavtoolを呼ぶ作業に移行
                boolean first = true;
                int trim_remain = (int)(trimMillisec / 1000.0 * sampleRate); //先頭から省かなければならないサンプル数の残り
#if DEBUG
                bocoree.debug.push_log( "trim_remain=" + trim_remain );
#endif
                VsqBPList dyn_curve = m_vsq.Track.get( renderingTrack ).getCurve( "dyn" );
#if MAKEBAT_SP
                bat = new StreamWriter( Path.Combine( m_temp_dir, "utau.bat" ), false, Encoding.GetEncoding( "Shift_JIS" ) );
#endif
                for ( int i = 0; i < num_queues; i++ ) {
                    RenderQueue rq = m_resampler_queue.get( i );
                    if ( !rq.ResamplerFinished ) {
                        String arg = rq.ResamplerArg;
#if DEBUG
                        bocoree.debug.push_log( "resampler arg=" + arg );
#endif
#if MAKEBAT_SP
                        bat.WriteLine( "\"" + m_resampler + "\" " + arg );
#endif

#if JAVA
                        String[] args = m_invoke_with_wine ? new String[]{ "wine \"" + m_resampler + "\"", arg }
                                                           : new String[]{ "\"" + m_resampler + "\"", arg };
                        ProcessBuilder pb = new ProcessBuilder( args );
                        Process process = pb.start();
                        process.waitFor();                        
#else
                        using ( Process process = new Process() ) {
                            process.StartInfo.FileName = (m_invoke_with_wine ? "wine \"" : "\"") + m_resampler + "\"";
                            process.StartInfo.Arguments = arg;
                            process.StartInfo.WorkingDirectory = m_temp_dir;
                            process.StartInfo.CreateNoWindow = true;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            
                            process.Start();
                            process.WaitForExit();
                        }
#endif
                    }
                    if ( m_abort_required ) {
                        break;
                    }

                    // wavtoolを起動
                    double sec_fin; // 今回のwavtool起動によってレンダリングが完了したサンプル長さ
                    RenderQueue p = m_resampler_queue.get( i ); //Phon p = s_resampler_queue[i];
                    OtoArgs oa_next;
                    if ( i + 1 < num_queues ) {
                        oa_next = m_resampler_queue.get( i + 1 ).Oto;
                    } else {
                        oa_next = new OtoArgs();
                    }
                    sec_fin = p.secEnd - oa_next.msOverlap / 1000.0;
#if DEBUG
                    AppManager.debugWriteLine( "sec_fin=" + sec_fin );
#endif
                    float mten = p.Oto.msPreUtterance + oa_next.msOverlap - oa_next.msPreUtterance;
                    String arg_wavtool = p.WavtoolArgPrefix + (mten >= 0 ? ("+" + mten) : ("-" + (-mten))) + p.WavtoolArgSuffix;
#if MAKEBAT_SP
                    bat.WriteLine( "\"" + m_wavtool + "\" " + arg_wavtool );
#endif
                    ProcessWavtool( arg_wavtool, file, m_temp_dir, m_wavtool, m_invoke_with_wine );

                    // できたwavを読み取ってWaveIncomingイベントを発生させる
                    int sample_end = (int)(sec_fin * sampleRate);
#if DEBUG
                    AppManager.debugWriteLine( "RenderUtau.StartRendering; sample_end=" + sample_end );
#endif
                    // whdを読みに行く
                    if ( first ) {
                        RandomAccessFile whd = null;
                        try {
                            whd = new RandomAccessFile( file_whd, "r" );
                            #region whdを読みに行く
                            whd.seek( 0 );
                            // RIFF
                            byte[] buf = new byte[4];
                            int gcount = whd.read( buf, 0, 4 );
                            if ( buf[0] != 'R' || buf[1] != 'I' || buf[2] != 'F' || buf[3] != 'F' ) {
#if DEBUG
                                AppManager.debugWriteLine( "RenderUtau.startRendering; whd header error" );
                                AppManager.debugWriteLine( ((char)buf[0]).ToString() + "" + ((char)buf[1]).ToString() + "" + ((char)buf[2]).ToString() + "" + ((char)buf[3]).ToString() + " must be RIFF" );
#endif
                                continue;
                            }
                            // ファイルサイズ
                            whd.read( buf, 0, 4 );
                            // WAVE
                            whd.read( buf, 0, 4 );
                            if ( buf[0] != 'W' || buf[1] != 'A' || buf[2] != 'V' || buf[3] != 'E' ) {
#if DEBUG
                                AppManager.debugWriteLine( "RenderUtau.startRendering; whd header error" );
                                AppManager.debugWriteLine( ((char)buf[0]).ToString() + "" + ((char)buf[1]).ToString() + "" + ((char)buf[2]).ToString() + "" + ((char)buf[3]).ToString() + " must be WAVE" );
#endif
                                continue;
                            }
                            // fmt 
                            whd.read( buf, 0, 4 );
                            if ( buf[0] != 'f' || buf[1] != 'm' || buf[2] != 't' || buf[3] != ' ' ) {
#if DEBUG
                                AppManager.debugWriteLine( "RenderUtau.startRendering; whd header error" );
                                AppManager.debugWriteLine( ((char)buf[0]).ToString() + "" + ((char)buf[1]).ToString() + "" + ((char)buf[2]).ToString() + "" + ((char)buf[3]).ToString() + " must be fmt " );
#endif
                                continue;
                            }
                            // fmt チャンクのサイズ
                            whd.read( buf, 0, 4 );
                            long loc_end_of_fmt = whd.getFilePointer(); //fmtチャンクの終了位置．ここは一定値でない可能性があるので読込み
                            loc_end_of_fmt += buf[0] | buf[1] << 8 | buf[2] << 16 | buf[3] << 24;
                            // format ID
                            whd.read( buf, 0, 2 );
                            int id = buf[0] | buf[1] << 8;
                            if ( id != 0x0001 ) { //0x0001はリニアPCM
                                continue;
                            }
                            // チャンネル数
                            whd.read( buf, 0, 2 );
                            channel = buf[1] << 8 | buf[0];
                            // サンプリングレート
                            whd.read( buf, 0, 4 );
                            int this_sample_rate = buf[0] | buf[1] << 8 | buf[2] << 16 | buf[3] << 24;
                            // データ速度
                            whd.read( buf, 0, 4 );
                            // ブロックサイズ
                            whd.read( buf, 0, 2 );
                            // 1チャンネル、1サンプルあたりのビット数
                            whd.read( buf, 0, 2 );
                            int bit_per_sample = buf[1] << 8 | buf[0];
                            byte_per_sample = bit_per_sample / 8;
                            whd.seek( loc_end_of_fmt );
                            // data
                            whd.read( buf, 0, 4 );
                            if ( buf[0] != 'd' || buf[1] != 'a' || buf[2] != 't' || buf[3] != 'a' ) {
#if DEBUG
                                AppManager.debugWriteLine( "RenderUtau.startRendering; whd header error" );
                                AppManager.debugWriteLine( ((char)buf[0]).ToString() + "" + ((char)buf[1]).ToString() + "" + ((char)buf[2]).ToString() + "" + ((char)buf[3]).ToString() + " must be data" );
#endif
                                continue;
                            }
                            // size of data chunk
                            whd.read( buf, 0, 4 );
                            int size = buf[3] << 24 | buf[2] << 16 | buf[1] << 8 | buf[0];
                            int total_samples = size / (channel * byte_per_sample);
                            #endregion
                        } catch ( Exception ex ) {
                            PortUtil.stderr.println( "UtauRenderingRunner#run; ex=" + ex );
                        } finally {
                            if ( whd != null ) {
                                try {
                                    whd.close();
                                } catch ( Exception ex2 ) {
                                    PortUtil.stderr.println( "UtauRenderingRunner#run; ex2=" + ex2 );
                                }
                            }
                        }
                        first = false;
                    }

                    // datを読みに行く
                    int sampleFrames = sample_end - processed_sample;
#if DEBUG
                    AppManager.debugWriteLine( "RenderUtau.StartRendering; sampleFrames=" + sampleFrames + "; channel=" + channel + "; byte_per_sample=" + byte_per_sample );
#endif
                    if ( channel > 0 && byte_per_sample > 0 && sampleFrames > 0 ) {
                        int length = (sampleFrames > sampleRate ? sampleRate : sampleFrames);
                        int remain = sampleFrames;
                        m_left = new double[length];
                        m_right = new double[length];
                        float k_inv64 = 1.0f / 64.0f;
                        float k_inv32768 = 1.0f / 32768.0f;
                        int buflen = 1024;
                        byte[] wavbuf = new byte[buflen];
                        int pos = 0;
                        RandomAccessFile dat = null;
                        try {
                            dat = new RandomAccessFile( file_dat, "r" );
                            dat.seek( processed_sample * channel * byte_per_sample );
                            double sec_start = processed_sample / (double)sampleRate;
                            double sec_per_sa = 1.0 / (double)sampleRate;
                            ByRef<Integer> index = new ByRef<Integer>( 0 );
                            #region チャンネル数／ビット深度ごとの読み取り操作
                            if ( byte_per_sample == 1 ) {
                                if ( channel == 1 ) {
                                    while ( remain > 0 ) {
                                        if ( m_abort_required ) {
                                            break;
                                        }
                                        int len = dat.read( wavbuf, 0, buflen );
                                        if ( len <= 0 ) {
                                            break;
                                        }
                                        int c = 0;
                                        while ( len > 0 && remain > 0 ) {
                                            if ( m_abort_required ) {
                                                break;
                                            }
                                            len -= 1;
                                            remain--;
                                            double gtime_dyn = sec_start + pos * sec_per_sa;
                                            int clock = (int)m_vsq.getClockFromSec( gtime_dyn );
                                            int dyn = dyn_curve.getValue( clock, index );
                                            float amp = (float)dyn * k_inv64;
                                            float v = (wavbuf[c] - 64.0f) * k_inv64 * amp;
                                            c++;
                                            m_left[pos] = v;
                                            m_right[pos] = v;
                                            pos++;
                                            if ( pos >= length ) {
                                                waveIncoming( m_left, m_right );
                                                pos = 0;
                                            }
                                        }
                                    }
                                } else {
                                    while ( remain > 0 ) {
                                        if ( m_abort_required ) {
                                            break;
                                        }
                                        int len = dat.read( wavbuf, 0, buflen );
                                        if ( len <= 0 ) {
                                            break;
                                        }
                                        int c = 0;
                                        while ( len > 0 && remain > 0 ) {
                                            if ( m_abort_required ) {
                                                break;
                                            }
                                            len -= 2;
                                            remain--;
                                            double gtime_dyn = sec_start + pos * sec_per_sa;
                                            int clock = (int)m_vsq.getClockFromSec( gtime_dyn );
                                            int dyn = dyn_curve.getValue( clock, index );
                                            float amp = (float)dyn * k_inv64;
                                            float vl = (wavbuf[c] - 64.0f) * k_inv64 * amp;
                                            float vr = (wavbuf[c + 1] - 64.0f) * k_inv64 * amp;
                                            m_left[pos] = vl;
                                            m_right[pos] = vr;
                                            c += 2;
                                            pos++;
                                            if ( pos >= length ) {
                                                waveIncoming( m_left, m_right );
                                                pos = 0;
                                            }
                                        }
                                    }
                                }
                            } else if ( byte_per_sample == 2 ) {
                                if ( channel == 1 ) {
                                    while ( remain > 0 ) {
                                        if ( m_abort_required ) {
                                            break;
                                        }
                                        int len = dat.read( wavbuf, 0, buflen );
                                        if ( len <= 0 ) {
                                            break;
                                        }
                                        int c = 0;
                                        while ( len > 0 && remain > 0 ) {
                                            if ( m_abort_required ) {
                                                break;
                                            }
                                            len -= 2;
                                            remain--;
                                            double gtime_dyn = sec_start + pos * sec_per_sa;
                                            int clock = (int)m_vsq.getClockFromSec( gtime_dyn );
                                            int dyn = dyn_curve.getValue( clock, index );
                                            float amp = (float)dyn * k_inv64;
                                            float v = ((short)(wavbuf[c] | wavbuf[c + 1] << 8)) * k_inv32768 * amp;
                                            m_left[pos] = v;
                                            m_right[pos] = v;
                                            c += 2;
                                            pos++;
                                            if ( pos >= length ) {
                                                waveIncoming( m_left, m_right );
                                                pos = 0;
                                            }
                                        }
                                    }
                                } else {
                                    while ( remain > 0 ) {
                                        if ( m_abort_required ) {
                                            break;
                                        }
                                        int len = dat.read( wavbuf, 0, buflen );
                                        if ( len <= 0 ) {
                                            break;
                                        }
                                        int c = 0;
                                        while ( len > 0 && remain > 0 ) {
                                            if ( m_abort_required ) {
                                                break;
                                            }
                                            len -= 4;
                                            remain--;
                                            double gtime_dyn = sec_start + pos * sec_per_sa;
                                            int clock = (int)m_vsq.getClockFromSec( gtime_dyn );
                                            int dyn = dyn_curve.getValue( clock, index );
                                            float amp = (float)dyn * k_inv64;
                                            float vl = ((short)(wavbuf[c] | wavbuf[c + 1] << 8)) * k_inv32768 * amp;
                                            float vr = ((short)(wavbuf[c + 2] | wavbuf[c + 3] << 8)) * k_inv32768 * amp;
                                            m_left[pos] = vl;
                                            m_right[pos] = vr;
                                            c += 4;
                                            pos++;
                                            if ( pos >= length ) {
                                                waveIncoming( m_left, m_right );
                                                pos = 0;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        } catch ( Exception ex ) {
                            PortUtil.stderr.println( "UtauRenderingRunner#run; ex=" + ex );
                        } finally {
                            if ( dat != null ) {
                                try {
                                    dat.close();
                                } catch ( Exception ex2 ) {
                                    PortUtil.stderr.println( "UtauRenderingRunner#run; ex2=" + ex2 );
                                }
                                dat = null;
                            }
                        }

                        if ( m_abort_required ) {
                            m_rendering = false;
                            m_abort_required = false;
                            return;
                        }
#if DEBUG
                        AppManager.debugWriteLine( "calling WaveIncoming..." );
#endif
                        if ( pos > 0 ) {
                            double[] bufl = new double[pos];
                            double[] bufr = new double[pos];
                            for ( int j = 0; j < pos; j++ ) {
                                bufl[j] = m_left[j];
                                bufr[j] = m_right[j];
                            }
                            waveIncoming( bufl, bufr );
                            bufl = null;
                            bufr = null;
                        }
                        m_left = null;
                        m_right = null;
#if JAVA
                        System.gc();
#else
                        GC.Collect();
#endif
#if DEBUG
                        AppManager.debugWriteLine( "...done(calling WaveIncoming)" );
#endif
                        processed_sample += (sampleFrames - remain);
                        m_progress = processed_sample / sample_length * 100.0;
                        double elapsed = getElapsedSeconds();
                        m_running_rate = m_progress / elapsed;
                    }
                }

#if MAKEBAT_SP
                bat.Close();
                bat = null;
#endif

                if ( m_mode_infinite ) {
                    double[] silence_l = new double[44100];
                    double[] silence_r = new double[44100];
                    while ( !m_abort_required ) {
                        waveIncoming( silence_l, silence_r );
                    }
                    silence_l = null;
                    silence_r = null;
                }
            } catch ( Exception ex ) {
                PortUtil.stderr.println( "UtauRenderingRunner#run; ex=" + ex );
            } finally {
#if MAKEBAT_SP
                if ( bat != null ) {
                    bat.WriteLine( "copy \"" + m_temp_dir + "\\temp.wav.whd\" /b + \"" + m_temp_dir + "\\temp.wav.dat\" /b \"" + m_temp_dir + "\\temp.wav\" /b" );
                    bat.Close();
                    bat = null;
                }
                if ( log != null ) {
                    log.Close();
                }
#endif
                m_rendering = false;
            }
        }

        public override double computeRemainingSeconds() {
            double elapsed = getElapsedSeconds();
            double estimated_total_time = 100.0 / m_running_rate;
            double draft = estimated_total_time - elapsed;
            if ( draft < 0 ) {
                draft = 0.0;
            }
            return draft;
        }

        private static void ProcessWavtool( String arg, String filebase, String temp_dir, String wavtool, boolean invoke_with_wine ) {
#if DEBUG
            bocoree.debug.push_log( "wavtool arg=" + arg );
#endif

#if JAVA
            String[] args = new String[]{ (invoke_with_wine ? "wine \"" : "\"") + wavtool + "\"", arg };
            ProcessBuilder pb = new ProcessBuilder( args );
            try{
                Process process = pb.start();
                process.waitFor();
            }catch( Exception ex ){
            }
#else
            using ( Process process = new Process() ) {
                process.StartInfo.FileName = (invoke_with_wine ? "wine \"" : "\"") + wavtool + "\"";
                process.StartInfo.Arguments = arg;
                process.StartInfo.WorkingDirectory = temp_dir;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
            }
#endif
        }

        private static String NoteStringFromNoteNumber( int note_number ) {
            int odd = note_number % 12;
            String head = (new String[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" })[odd];
            return head + (note_number / 12 - 1);
        }
    }

#if !JAVA
}
#endif