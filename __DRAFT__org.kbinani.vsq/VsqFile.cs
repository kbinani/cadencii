/*
 * VsqFile.cs
 * Copyright (C) 2008-2010 kbinani
 *
 * This file is part of org.kbinani.vsq.
 *
 * org.kbinani.vsq is free software; you can redistribute it and/or
 * modify it under the terms of the BSD License.
 *
 * org.kbinani.vsq is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 */
#define NEW_IMPL
#if JAVA
package org.kbinani.vsq;

import java.util.*;
import java.io.*;
import org.kbinani.*;
#else
using System;
using System.Collections.Generic;

namespace org.kbinani.vsq {
    using boolean = System.Boolean;
    using Integer = System.Int32;
    using Long = System.Int64;
#endif

    /// <summary>
    /// VSQファイルの内容を保持するクラス
    /// </summary>
#if JAVA
    public class VsqFile implements Cloneable, Serializable {
#else
    [Serializable]
    public class VsqFile : ICloneable {
#endif
        /// <summary>
        /// トラックのリスト．最初のトラックはMasterTrackであり，通常の音符が格納されるトラックはインデックス1以降となる
        /// </summary>
        public List<VsqTrack> Track;
        /// <summary>
        /// テンポ情報を保持したテーブル
        /// </summary>
        public TempoVector TempoTable;
        public List<TimeSigTableEntry> TimesigTable;
        protected const int m_tpq = 480;
        /// <summary>
        /// 曲の長さを取得します。(クロック(4分音符は480クロック))
        /// </summary>
        public int TotalClocks = 0;
        protected const int baseTempo = 500000; 
        public VsqMaster Master;  // VsqMaster, VsqMixerは通常，最初の非Master Trackに記述されるが，可搬性のため，
        public VsqMixer Mixer;    // ここではVsqFileに直属するものとして取り扱う．
        private BarLineIterator barLineIterator = null;
        public Object Tag;

        static readonly byte[] _MTRK = new byte[] { (byte)0x4d, (byte)0x54, (byte)0x72, (byte)0x6b };
        static readonly byte[] _MTHD = new byte[] { (byte)0x4d, (byte)0x54, (byte)0x68, (byte)0x64 };
        static readonly byte[] _MASTER_TRACK = new byte[] { (byte)0x4D, (byte)0x61, (byte)0x73, (byte)0x74, (byte)0x65, (byte)0x72, (byte)0x20, (byte)0x54, (byte)0x72, (byte)0x61, (byte)0x63, (byte)0x6B, };
        static readonly String[] _CURVES = new String[] { "VEL", "DYN", "BRE", "BRI", "CLE", "OPE", "GEN", "POR", "PIT", "PBS" };

#if JAVA
        public VsqFile( UstFile ust ){
            this( "Miku", 1, 4, 4, ust.getBaseTempo() );
#else
        public VsqFile( UstFile ust )
            : this( "Miku", 1, 4, 4, ust.getBaseTempo() ) {
#endif
            int clock_count = 480 * 4; //pre measure = 1、4分の4拍子としたので
            VsqBPList pitch = new VsqBPList( "", 0, -2400, 2400 ); // ノートナンバー×100
            for ( vecitr<UstEvent> itr = ust.getTrack( 0 ).getNoteEventIterator(); itr.hasNext(); ) {
                UstEvent ue = itr.next();
                if ( ue.Lyric != "R" ) {
                    VsqID id = new VsqID( 0 );
                    id.setLength( ue.getLength() );
                    String psymbol = "a";
                    SymbolTableEntry entry = SymbolTable.attatch( ue.Lyric );
                    if( entry != null ){
                        psymbol = entry.getSymbol();
                    }
                    id.LyricHandle = new LyricHandle( ue.Lyric, psymbol );
                    id.Note = ue.Note;
                    id.type = VsqIDType.Anote;
                    VsqEvent ve = new VsqEvent( clock_count, id );
                    ve.UstEvent = (UstEvent)ue.clone();
                    vec.get( Track, 1 ).addEvent( ve );

                    if ( ue.Pitches != null ) {
                        // PBTypeクロックごとにデータポイントがある
                        int clock = clock_count - ue.PBType;
                        for ( int i = 0; i < ue.Pitches.Length; i++ ) {
                            clock += ue.PBType;
                            pitch.add( clock, (int)ue.Pitches[i] );
                        }
                    }
                }
                if ( ue.Tempo > 0.0f ) {
                    vec.add( TempoTable, new TempoTableEntry( clock_count, (int)(60e6 / ue.Tempo), 0.0 ) );
                }
                clock_count += ue.getLength();
            }
            updateTempoInfo();
            updateTotalClocks();
            updateTimesigInfo();
            reflectPitch( this, 1, pitch );
        }

        /// <summary>
        /// このクラスの指定した名前のプロパティが総称型引数を用いる型である場合に，
        /// その型の限定名を返します．それ以外の場合は空文字を返します．
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String getGenericTypeName( String name ) {
            if ( name != null ) {
                if ( name.Equals( "Track" ) ) {
                    return "org.kbinani.vsq.VsqTrack";
                } else if ( name.Equals( "TimesigTable" ) ) {
                    return "org.kbinani.vsq.TimeSigTableEntry";
                } else if ( name.Equals( "TempoTable" ) ) {
                    return "org.kbinani.vsq.TempoTableEntry";
                }
            }
            return "";
        }

        /// <summary>
        /// このクラスの指定した名前のプロパティを，XMLシリアライズ時に無視するかどうかを表す
        /// ブール値を返します．デフォルトの実装では戻り値は全てfalseです．
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static boolean isXmlIgnored( String name ) {
            return false;
        }

        /// <summary>
        /// このクラスの指定した名前のプロパティをXMLシリアライズする際に使用する
        /// 要素名を取得します．
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String getXmlElementName( String name ) {
            return name;
        }

        public virtual void adjustClockToMatchWith( double tempo ) {
            int numTrack = Track.size();
            for ( int track = 1; track < numTrack; track++ ) {
                VsqTrack vsq_track = Track.get( track );
                // ノート・歌手イベントをシフト
                for ( Iterator<VsqEvent> itr = vsq_track.getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.ID.type == VsqIDType.Singer && item.Clock == 0 ) {
                        continue;
                    }
                    int clock = item.Clock;
                    double sec_start = getSecFromClock( clock );
                    double sec_end = getSecFromClock( clock + item.ID.getLength() );
                    int clock_start = (int)(sec_start * 8.0 * tempo);
                    int clock_end = (int)(sec_end * 8.0 * tempo);
                    item.Clock = clock_start;
                    item.ID.setLength( clock_end - clock_start );
                    if ( item.ID.VibratoHandle != null ) {
                        double sec_vib_start = getSecFromClock( clock + item.ID.VibratoDelay );
                        int clock_vib_start = (int)(sec_vib_start * 8.0 * tempo);
                        item.ID.VibratoDelay = clock_vib_start - clock_start;
                        item.ID.VibratoHandle.setLength( clock_end - clock_vib_start );
                    }
                }

                // コントロールカーブをシフト
                for ( int j = 0; j < _CURVES.Length; j++ ) {
                    String ct = _CURVES[j];
                    VsqBPList item = vsq_track.getCurve( ct );
                    if ( item == null ) {
                        continue;
                    }
                    VsqBPList repl = new VsqBPList( item.getName(), item.getDefault(), item.getMinimum(), item.getMaximum() );
                    int numPoints = item.size();
                    for ( int i = 0; i < numPoints; i++ ) {
                        int clock = item.getKeyClock( i );
                        int value = item.getElement( i );
                        double sec = getSecFromClock( clock );
                        if ( sec >= 0.0 ) {
                            int clock_new = (int)(sec * 8.0 * tempo);
                            repl.add( clock_new, value );
                        }
                    }
                    vsq_track.setCurve( ct, repl );
                }
            }

            // テンポテーブルを刷新
            TempoTable.clear();
            TempoTable.add( new TempoTableEntry( 0, (int)(60e6 / tempo), 0.0 ) );
            updateTempoInfo();
            updateTimesigInfo();
            updateTotalClocks();
        }

        /// <summary>
        /// VsqEvent, VsqBPListの全てのクロックを、tempoに格納されているテンポテーブルに
        /// 合致するようにシフトします
        /// </summary>
        /// <param name="tempo"></param>
        public virtual void adjustClockToMatchWith( TempoVector tempo ) {
            double premeasure_sec_tempo = 0;

            // テンポをリプレースする場合。
            // まずクロック値を、リプレース後のモノに置き換え
            int numTrack = Track.size();
            for ( int track = 1; track < numTrack; track++ ) {
                VsqTrack vsq_track = Track.get( track );
                // ノート・歌手イベントをシフト
                for ( Iterator<VsqEvent> itr = vsq_track.getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.ID.type == VsqIDType.Singer && item.Clock == 0 ) {
                        continue;
                    }
                    int clock = item.Clock;
                    double sec_start = this.getSecFromClock( clock );
                    double sec_end = this.getSecFromClock( clock + item.ID.getLength() );
                    int clock_start = (int)tempo.getClockFromSec( sec_start );
                    int clock_end = (int)tempo.getClockFromSec( sec_end );
                    item.Clock = clock_start;
                    item.ID.setLength( clock_end - clock_start );
                    if ( item.ID.VibratoHandle != null ) {
                        double sec_vib_start = this.getSecFromClock( clock + item.ID.VibratoDelay );
                        int clock_vib_start = (int)tempo.getClockFromSec( sec_vib_start );
                        item.ID.VibratoDelay = clock_vib_start - clock_start;
                        item.ID.VibratoHandle.setLength( clock_end - clock_vib_start );
                    }
                }

                // コントロールカーブをシフト
                for ( int j = 0; j < _CURVES.Length; j++ ) {
                    String ct = _CURVES[j];
                    VsqBPList item = vsq_track.getCurve( ct );
                    if ( item == null ) {
                        continue;
                    }
                    VsqBPList repl = new VsqBPList( item.getName(), item.getDefault(), item.getMinimum(), item.getMaximum() );
                    int c = item.size();
                    for ( int i = 0; i < c; i++ ) {
                        int clock = item.getKeyClock( i );
                        int value = item.getElement( i );
                        double sec = this.getSecFromClock( clock );
                        if ( sec >= premeasure_sec_tempo ) {
                            int clock_new = (int)tempo.getClockFromSec( sec );
                            repl.add( clock_new, value );
                        }
                    }
                    vsq_track.setCurve( ct, repl );
                }
            }
        }


        /// <summary>
        /// オーバーロードされます．このインスタンスの内容を，MusicXML形式のファイルに出力します
        /// </summary>
        /// <param name="file">出力するファイルのパス</param>
        /// <param name="encoding">MusicXMLのテキストエンコーディング</param>
        public void printAsMusicXml( String file, String encoding ) {
            printAsMusicXmlCore( file, encoding, "", (int)(60e6 / getTempoAt( 0 )), false );
        }

        /// <summary>
        /// オーバーロードされます．このインスタンスの内容を，MusicXML形式のファイルに出力します
        /// </summary>
        /// <param name="file">出力するファイルのパス</param>
        /// <param name="encoding">MusicXMLのテキストエンコーディング</param>
        /// <param name="tempo">このインスタンスの中身を，このテンポ値の場合の再生秒時に合致するように音符などを移動する</param>
        public void printAsMusicXml( String file, String encoding, int tempo ) {
            printAsMusicXmlCore( file, encoding, "", tempo, true );
        }

        /// <summary>
        /// オーバーロードされます．このインスタンスの内容を，MusicXML形式のファイルに出力します
        /// </summary>
        /// <param name="file">出力するファイルのパス</param>
        /// <param name="encoding">MusicXMLのテキストエンコーディング</param>
        /// <param name="software">出力を行ったソフトウェアの名称</param>
        public void printAsMusicXml( String file, String encoding, String software ) {
            printAsMusicXmlCore( file, encoding, software, (int)(60e6 / getTempoAt( 0 )), false );
        }

        /// <summary>
        /// オーバーロードされます．このインスタンスの内容を，MusicXML形式のファイルに出力します
        /// </summary>
        /// <param name="file">出力するファイルのパス</param>
        /// <param name="encoding">MusicXMLのテキストエンコーディング</param>
        /// <param name="software">出力を行ったソフトウェアの名称</param>
        /// <param name="tempo">このインスタンスの中身を，このテンポ値の場合の再生秒時に合致するように音符などを移動する</param>
        public void printAsMusicXml( String file, String encoding, String software, int tempo ) {
            printAsMusicXmlCore( file, encoding, software, tempo, true );
        }

        private static void printStyledNoteCor(
            BufferedWriter writer,
            int clock_length,
            int note,
            String lyric,
            Dictionary<String, Boolean> altered_context,
            boolean tie_start_required,
            boolean tie_stop_required,
            String type ) 
#if JAVA
            throws java.io.IOException
#endif
        {
            writer.write( "      <note>" ); writer.newLine();
            if ( note < 0 ) {
                writer.write( "        <rest/>" ); writer.newLine();
            } else {
                String noteStringBase = VsqNote.getNoteStringBase( note ); // "C"など
                int octave = VsqNote.getNoteOctave( note );
                writer.write( "        <pitch>" ); writer.newLine();
                writer.write( "          <step>" + noteStringBase + "</step>" ); writer.newLine();
                int alter = VsqNote.getNoteAlter( note );
                if ( alter != 0 ) {
                    writer.write( "          <alter>" + alter + "</alter>" ); writer.newLine();
                }
                writer.write( "          <octave>" + (octave + 1) + "</octave>" ); writer.newLine();
                writer.write( "        </pitch>" ); writer.newLine();
                String stem = note >= 70 ? "down" : "up";
                writer.write( "        <stem>" + stem + "</stem>" ); writer.newLine();
                String accidental = "";
                String checkAltered = noteStringBase;
                if ( !tie_stop_required && altered_context.containsKey( checkAltered ) ) {
                    if ( alter == 0 ) {
                        if ( altered_context.get( checkAltered ) ) {
                            accidental = "natural";
                            altered_context.put( checkAltered, false );
                        }
                    } else {
                        if ( !altered_context.get( checkAltered ) ) {
                            accidental = alter == 1 ? "sharp" : "flat";
                            altered_context.put( checkAltered, true );
                        }
                    }
                }

                if ( PortUtil.getStringLength( accidental ) > 0 ) {
                    writer.write( "        <accidental>" + accidental + "</accidental>" ); writer.newLine();
                }
                if ( tie_start_required ) {
                    writer.write( "        <tie type=\"start\"/>" ); writer.newLine();
                    writer.write( "        <notations>" ); writer.newLine();
                    writer.write( "          <tied type=\"start\"/>" ); writer.newLine();
                    writer.write( "        </notations>" ); writer.newLine();
                }
                if ( tie_stop_required ) {
                    writer.write( "        <tie type=\"stop\"/>" ); writer.newLine();
                    writer.write( "        <notations>" ); writer.newLine();
                    writer.write( "          <tied type=\"stop\"/>" ); writer.newLine();
                    writer.write( "        </notations>" ); writer.newLine();
                }
                if ( !tie_stop_required ) {
                    writer.write( "        <lyric>" ); writer.newLine();
                    writer.write( "          <text>" + lyric + "</text>" ); writer.newLine();
                    writer.write( "        </lyric>" ); writer.newLine();
                }
            }

            writer.write( "        <voice>1</voice>" ); writer.newLine();
            writer.write( "        <duration>" + clock_length + "</duration>" ); writer.newLine();
            if ( PortUtil.getStringLength( type ) > 0 ) {
                writer.write( "        <type>" + type + "</type>" ); writer.newLine();
            }
            writer.write( "      </note>" ); writer.newLine();
        }

        private static void printStyledNote( 
            BufferedWriter writer, 
            int clock_start,
            int clock_length,
            int note,
            List<TempoTableEntry> tempoInsert,
            String lyric,
            Dictionary<String, Boolean> altered_context,
            boolean tie_start_required, 
            boolean tie_stop_required )
#if JAVA
            throws java.io.IOException
#endif
        {
            int numInsert = tempoInsert.size();
            for ( int i = 0; i < numInsert; i++ ) {
                TempoTableEntry itemi = tempoInsert.get( i );
                int tempo = (int)(60e6 / (double)itemi.Tempo);
                writer.write( "      <direction placement=\"above\">" ); writer.newLine();
                writer.write( "        <direction-type>" ); writer.newLine();
                writer.write( "          <metronome>" ); writer.newLine();
                writer.write( "            <beat-unit>quarter</beat-unit>" ); writer.newLine();
                writer.write( "            <per-minute>" + tempo + "</per-minute>" ); writer.newLine();
                writer.write( "          </metronome>" ); writer.newLine();
                writer.write( "          <words>Tempo " + tempo + "</words>" ); writer.newLine();
                writer.write( "        </direction-type>" ); writer.newLine();
                writer.write( "        <sound tempo=\"" + tempo + "\"/>" ); writer.newLine();
                writer.write( "        <offset>" + (itemi.Clock - clock_start) + "</offset>" ); writer.newLine();
                writer.write( "      </direction>" ); writer.newLine();
            }
            int[] ret = new int[9];
            int[] len = new int[] { 1920, 960, 480, 240, 120, 60, 30, 15 };
            String[] name = new String[] { "whole", "half", "quarter", "eighth", "16th", "32nd", "64th", "128th", "" };
            int remain = clock_length;
            for ( int i = 0; i < 8; i++ ) {
                ret[i] = remain / len[i];
                if ( ret[i] > 0 ) {
                    remain -= len[i] * ret[i];
                }
            }
            ret[8] = remain;

            int firstContainedIndex = -1;
            int lastContainedIndex = -1;
            int numSeparated = 0;
            for ( int i = 0; i < 8; i++ ) {
                if ( ret[i] > 0 ) {
                    if ( firstContainedIndex < 0 ) {
                        firstContainedIndex = i;
                    }
                    lastContainedIndex = i;
                    numSeparated += ret[i];
                }
            }
            for ( int i = 0; i < 8; i++ ) {
                int count = ret[i];
                if ( count <= 0 ) {
                    continue;
                }
                for ( int j = 0; j < count; j++ ) {
                    boolean local_tie_start_required = numSeparated > 0 ? true : false;
                    boolean local_tie_stop_required = numSeparated > 0 ? true : false;
                    if ( i == firstContainedIndex && j == 0 ) {
                        local_tie_stop_required = tie_stop_required;
                    }
                    if ( i == lastContainedIndex && j == count - 1 ) {
                        local_tie_start_required = tie_start_required;
                    }
                    printStyledNoteCor( writer, len[i], note, lyric, altered_context, local_tie_start_required, local_tie_stop_required, name[i] );
                }
            }
        }

        private void printAsMusicXmlCore( String file, String encoding, String software, int tempo, boolean change_tempo ) {
            BufferedWriter sw = null;
            VsqFile vsq = (VsqFile)clone();
            int intTempo = (int)(60e6 / tempo);
            if ( !change_tempo ) {
                intTempo = vsq.getTempoAt( 0 );
            }
#if DEBUG
            PortUtil.println( "VsqFile#printAsMusicXmlCore; change_tempo=" + change_tempo );
            PortUtil.println( "VsqFile#printAsMusicXmlCore; tempo=" + tempo );
            PortUtil.println( "VsqFile#printAsMusicXmlCore; intTempo=" + intTempo + "; tempo=" + tempo );
#endif
            if ( change_tempo ) {
                //VsqFile tempoVsq = new VsqFile( "", getPreMeasure(), 4, 4, intTempo );
                //vsq.adjustClockToMatchWith( tempoVsq );
                vsq.adjustClockToMatchWith( tempo );
            }
            Timesig timesig = vsq.getTimesigAt( 0 );
            int removeClock = timesig.numerator * 480 * 4 / timesig.denominator;

            try {
                sw = new BufferedWriter( new OutputStreamWriter( new FileOutputStream( file ), encoding ) );

                // ヘッダ
                sw.write( "<?xml version=\"1.0\" encoding=\"" + encoding + "\"?>" ); sw.newLine();
                sw.write( "<!DOCTYPE score-partwise PUBLIC \"-//Recordare//DTD MusicXML 2.0 Partwise//EN\"" ); sw.newLine();
                sw.write( "                                \"http://www.musicxml.org/dtds/partwise.dtd\">" ); sw.newLine();
                sw.write( "<score-partwise version=\"2.0\">" ); sw.newLine();
                sw.write( "  <identification>" ); sw.newLine();
                sw.write( "    <encoding>" ); sw.newLine();
                if ( PortUtil.getStringLength( software ) > 0 ) {
                    sw.write( "      <software>" + software + "</software>" ); sw.newLine();
                }
                sw.write( "      <software>org.kbinani.vsq</software>" ); sw.newLine();
                sw.write( "    </encoding>" ); sw.newLine();
                sw.write( "  </identification>" ); sw.newLine();
                sw.write( "  <part-list>" ); sw.newLine();

                int track_count = vsq.Track.size();
                Timesig timesigStart = vsq.getTimesigAt( 0 );
                int clockPerMeasure = timesigStart.numerator * 480 * 4 / timesigStart.denominator;
                for ( int i = 1; i < track_count; i++ ) {
                    VsqTrack vsq_track = vsq.Track.get( i );
                    sw.write( "    <score-part id=\"P" + i + "\">" ); sw.newLine();
                    sw.write( "      <part-name>" + vsq_track.getName() + "</part-name>" ); sw.newLine();
                    sw.write( "    </score-part>" ); sw.newLine();
                }
                sw.write( "  </part-list>" ); sw.newLine();
                int measureStart = 0; // 出力開始する小節

                for ( int i = 1; i < track_count; i++ ) {
                    VsqTrack vsq_track = vsq.Track.get( i );
                    int numEvents = vsq_track.getEventCount();
                    sw.write( "  <part id=\"P" + i + "\">" ); sw.newLine();

                    // 拍子変更毎に出力していく
                    int countTimesig = vsq.TimesigTable.size();
                    int totalMeasure = measureStart; // 出力してきた小節の数
                    int clockLastBase = measureStart * clockPerMeasure; // 前回の拍子ブロックで出力し終わったクロック
                    int startIndex = 0;
                    for ( int n = 0; n < countTimesig; n++ ) {
                        TimeSigTableEntry timesigEntryThis = vsq.TimesigTable.get( n );
                        clockPerMeasure = timesigEntryThis.Numerator * 480 * 4 / timesigEntryThis.Denominator;

                        // この拍子が曲の終まで続くとしたら，あと何小節出力する必要があるのか？
                        int remainingMeasures = 0;
                        if ( n + 1 < countTimesig ) {
                            TimeSigTableEntry timesigEntryNext = vsq.TimesigTable.get( n + 1 );
                            remainingMeasures = timesigEntryNext.BarCount - timesigEntryThis.BarCount;
                        } else {
                            int remainingClocks = vsq.TotalClocks - clockLastBase;
                            remainingMeasures = remainingClocks / clockPerMeasure;
                            if ( remainingClocks % clockPerMeasure != 0 ) {
                                remainingMeasures++;
                            }
                        }

                        // remainingMeasures小節を順次出力
                        for ( int j = totalMeasure; j < totalMeasure + remainingMeasures; j++ ) {
                            int clockStart = clockLastBase + (j - totalMeasure) * clockPerMeasure;
                            int clockEnd = clockStart + clockPerMeasure;

                            // 今出力している小節内に、テンポ変更が挿入されているかどうか
                            int numTempo = TempoTable.size();
                            List<TempoTableEntry> tempoInsert = new List<TempoTableEntry>(); // 挿入するテンポ情報のリスト
                            for ( int k = 0; k < numTempo; k++ ) {
                                TempoTableEntry itemk = TempoTable.get( k );
                                if ( clockStart <= itemk.Clock && itemk.Clock < clockEnd ) {
                                    tempoInsert.add( (TempoTableEntry)itemk.clone() );
                                }
                            }

                            sw.write( "    <measure number=\"" + (j + 1 - measureStart) + "\">" ); sw.newLine();
                            if ( j == totalMeasure ) {
                                sw.write( "      <attributes>" ); sw.newLine();
                                sw.write( "        <divisions>480</divisions>" ); sw.newLine();
                                sw.write( "        <time symbol=\"common\">" ); sw.newLine();
                                sw.write( "          <beats>" + timesigEntryThis.Numerator + "</beats>" ); sw.newLine();
                                sw.write( "          <beat-type>" + timesigEntryThis.Denominator + "</beat-type>" ); sw.newLine();
                                sw.write( "        </time>" ); sw.newLine();
                                sw.write( "      </attributes>" ); sw.newLine();
                                sw.write( "      <direction placement=\"above\">" ); sw.newLine();
                                sw.write( "        <direction-type>" ); sw.newLine();
                                sw.write( "          <metronome>" ); sw.newLine();
                                sw.write( "            <beat-unit>quarter</beat-unit>" ); sw.newLine();
                                sw.write( "            <per-minute>" + tempo + "</per-minute>" ); sw.newLine();
                                sw.write( "          </metronome>" ); sw.newLine();
                                sw.write( "        </direction-type>" ); sw.newLine();
                                sw.write( "        <sound tempo=\"" + tempo + "\"/>" ); sw.newLine();
                                sw.write( "      </direction>" ); sw.newLine();
                            }

                            // 臨時記号のON/OFFを制御するために
                            Dictionary<String, Boolean> altered = new Dictionary<String, Boolean>();
                            String[] basic = new String[] { "C", "D", "E", "F", "G", "A", "B" };
                            for ( int m = 0; m < basic.Length; m++ ) {
                                altered.put( basic[m], false );
                            }

                            int clockLast = clockStart; // 出力済みのクロック
                            for ( int k = startIndex; k < numEvents; k++ ) {
                                VsqEvent itemk = vsq_track.getEvent( k );
                                if ( itemk.ID.type != VsqIDType.Anote ) {
                                    if ( clockEnd <= itemk.Clock ) {
                                        startIndex = k;
                                        break;
                                    }
                                    continue;
                                }
                                if ( (clockStart <= itemk.Clock && itemk.Clock < clockEnd) ||
                                     (clockStart <= itemk.Clock + itemk.ID.getLength() && itemk.Clock + itemk.ID.getLength() < clockEnd) ||
                                     (itemk.Clock <= clockStart && clockEnd <= itemk.Clock + itemk.ID.getLength()) ) {
                                    // 出力する必要がある
                                    if ( clockLast < itemk.Clock ) {
                                        // 音符の前に休符が必要
                                        // clockLast <= * < itemk.Clockの間にテンポ変更を挿入する必要があるかどうか
                                        List<TempoTableEntry> insert = new List<TempoTableEntry>();
                                        if ( !change_tempo ) {
                                            for ( int m = 0; m < tempoInsert.size(); m++ ) {
                                                TempoTableEntry itemm = tempoInsert.get( m );
                                                if ( clockLast <= itemm.Clock && itemm.Clock < itemk.Clock ) {
                                                    insert.add( itemm );
                                                }
                                            }
                                        }
                                        printStyledNote( sw, clockLast, itemk.Clock - clockLast, -1, insert, "", altered, false, false );
                                        clockLast = itemk.Clock;
                                    }

                                    boolean tieStopRequired = false;
                                    int start = itemk.Clock;
                                    if ( start < clockStart ) {
                                        // 前の小節からタイで接続されている場合
                                        start = clockStart;
                                        tieStopRequired = true;
                                    }
                                    int end = itemk.Clock + itemk.ID.getLength();
                                    boolean tieStartRequired = false;
                                    if ( clockEnd < end ) {
                                        // 次の小節にタイで接続しなければならない場合
                                        end = clockEnd;
                                        tieStartRequired = true;
                                    }
                                    int actualLength = end - start;
                                    List<TempoTableEntry> insert2 = new List<TempoTableEntry>();
                                    if ( !change_tempo ) {
                                        for ( int m = 0; m < tempoInsert.size(); m++ ) {
                                            TempoTableEntry itemm = tempoInsert.get( m );
                                            if ( start <= itemm.Clock && itemm.Clock < end ) {
                                                insert2.add( itemm );
                                            }
                                        }
                                    }
                                    printStyledNote( sw, start, actualLength, itemk.ID.Note, insert2, itemk.ID.LyricHandle.L0.Phrase, altered, tieStartRequired, tieStopRequired );
                                    clockLast = end;
                                    if ( tieStartRequired ) {
                                        startIndex = k;
                                    } else {
                                        startIndex = k + 1;
                                    }
                                }
                            }
                            if ( clockLast < clockEnd ) {
                                // 小節の最後に休符を入れる必要がある
                                List<TempoTableEntry> insert3 = new List<TempoTableEntry>();
                                if ( !change_tempo ) {
                                    for ( int m = 0; m < tempoInsert.size(); m++ ) {
                                        TempoTableEntry itemm = tempoInsert.get( m );
                                        if ( clockEnd <= itemm.Clock && itemm.Clock < clockLast ) {
                                            insert3.add( itemm );
                                        }
                                    }
                                }
                                printStyledNote( sw, clockLast, (clockEnd - clockLast), -1, insert3, "", altered, false, false );
                                clockLast = clockEnd;
                            }
                            sw.write( "    </measure>" ); sw.newLine();
                        }
                        clockLastBase += remainingMeasures * clockPerMeasure;
                        totalMeasure += remainingMeasures;
                    }
                    sw.write( "  </part>" ); sw.newLine();
                }
                sw.write( "</score-partwise>" ); sw.newLine();
            } catch ( Exception ex ) {
                PortUtil.stderr.println( "VsqFile#printAsMusicXml; ex=" + ex );
            } finally {
                if ( sw != null ) {
                    try {
                        sw.close();
                    } catch ( Exception ex2 ) {
                        PortUtil.stderr.println( "VsqFile#printAsMusicXml; ex2=" + ex2 );
                    }
                }
            }
        }

        /// <summary>
        /// このインスタンスの内容を，MusicXML形式のファイルに出力します
        /// </summary>
        /// <param name="file"></param>
        public void printAsMusicXml_OLD( String file ) {
            BufferedWriter sw = null;
            try {
                sw = new BufferedWriter( new OutputStreamWriter( new FileOutputStream( file ), "UTF-8" ) );

                // ヘッダ
                sw.write( "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" ); sw.newLine();
                sw.write( "<!DOCTYPE score-partwise PUBLIC \"-//Recordare//DTD MusicXML 2.0 Partwise//EN\"" ); sw.newLine();
                sw.write( "                                \"http://www.musicxml.org/dtds/partwise.dtd\">" ); sw.newLine();
                sw.write( "<score-partwise version=\"2.0\">" ); sw.newLine();
                sw.write( "  <part-list>" ); sw.newLine();
                int track_count = Track.size();
                for ( int i = 1; i < track_count; i++ ) {
                    VsqTrack vsq_track = Track.get( i );
                    sw.write( "    <score-part id=\"P" + i + "\">" ); sw.newLine();
                    sw.write( "      <part-name>" + vsq_track.getName() + "</part-name>" ); sw.newLine();
                    sw.write( "    </score-part>" ); sw.newLine();
                }
                sw.write( "  </part-list>" ); sw.newLine();
                int clockPerMeasure = 480 * 4;
                for ( int i = 1; i < track_count; i++ ) {
                    VsqTrack vsq_track = Track.get( i );
                    sw.write( "  <part id=\"P" + i + "\">" ); sw.newLine();
                    //TODO: この辺から
                    // とりあえず，全部4/4拍子設定で出力
                    int num_measure = TotalClocks / clockPerMeasure + 1;
                    int startIndex = 0;
                    int numEvents = vsq_track.getEventCount();
                    for ( int j = 0; j < num_measure; j++ ) {
                        sw.write( "    <measure number=\"" + (j + 1) + "\">" ); sw.newLine();
                        if ( j == 0 ) {
                            sw.write( "      <attributes>" ); sw.newLine();
                            sw.write( "        <divisions>480</divisions>" ); sw.newLine();
                            sw.write( "        <time symbol=\"common\">" ); sw.newLine();
                            sw.write( "          <beats>4</beats>" ); sw.newLine();
                            sw.write( "          <beat-type>4</beat-type>" ); sw.newLine();
                            sw.write( "        </time>" ); sw.newLine();
                            sw.write( "      </attributes>" ); sw.newLine();
                            sw.write( "      <sound tempo=\"" + getBaseTempo() + "\"/>" ); sw.newLine();
                        }
                        int clockStart = j * clockPerMeasure;
                        int clockEnd = clockStart + clockPerMeasure;
                        int clockLast = clockStart; // 出力済みのクロック
                        for ( int k = startIndex; k < numEvents; k++ ) {
                            VsqEvent itemk = vsq_track.getEvent( k );
                            if ( itemk.ID.type != VsqIDType.Anote ) {
                                if ( clockEnd <= itemk.Clock ) {
                                    startIndex = k;
                                    break;
                                }
                                continue;
                            }
                            if ( (clockStart <= itemk.Clock && itemk.Clock < clockEnd) ||
                                 (clockStart <= itemk.Clock + itemk.ID.getLength() && itemk.Clock + itemk.ID.getLength() < clockEnd) ||
                                 (itemk.Clock <= clockStart && clockEnd <= itemk.Clock + itemk.ID.getLength()) ) {
                                // 出力する必要がある
                                if ( clockLast < itemk.Clock ) {
                                    // 音符の前に休符が必要
                                    sw.write( "      <note>" ); sw.newLine();
                                    sw.write( "        <rest/>" ); sw.newLine();
                                    sw.write( "        <duration>" + (itemk.Clock - clockLast) + "</duration>" ); sw.newLine();
                                    sw.write( "        <voice>1</voice>" ); sw.newLine();
                                    sw.write( "      </note>" ); sw.newLine();
                                    clockLast = itemk.Clock;
                                }

                                boolean tieStopRequired = false;
                                int start = itemk.Clock;
                                if ( start < clockStart ) {
                                    // 前の小節からタイで接続されている場合
                                    start = clockStart;
                                    tieStopRequired = true;
                                }
                                int end = itemk.Clock + itemk.ID.getLength();
                                boolean tieStartRequired = false;
                                if ( clockEnd < end ) {
                                    // 次の小節にタイで接続しなければならない場合
                                    end = clockEnd;
                                    tieStartRequired = true;
                                }
                                int actualLength = end - start;
                                sw.write( "      <note>" ); sw.newLine();
                                int note = itemk.ID.Note;
                                sw.write( "        <pitch>" ); sw.newLine();
                                sw.write( "          <step>" + VsqNote.getNoteStringBase( note ) + "</step>" ); sw.newLine();
                                int alter = VsqNote.getNoteAlter( note );
                                if ( alter != 0 ) {
                                    sw.write( "          <alter>" + alter + "</alter>" ); sw.newLine();
                                }
                                sw.write( "          <octave>" + (VsqNote.getNoteOctave( note ) + 1) + "</octave>" ); sw.newLine();
                                sw.write( "        </pitch>" ); sw.newLine();
                                sw.write( "        <duration>" + actualLength + "</duration>" ); sw.newLine();
                                sw.write( "        <voice>1</voice>" ); sw.newLine();
                                //if ( !(tieStartRequired && tieStopRequired) ) {
                                    if ( tieStartRequired ) {
                                        sw.write( "        <tie type=\"start\"/>" ); sw.newLine();
                                        sw.write( "        <notations>" ); sw.newLine();
                                        sw.write( "          <tied type=\"start\"/>" ); sw.newLine();
                                        sw.write( "        </notations>" ); sw.newLine();
                                    }
                                    if ( tieStopRequired ) {
                                        sw.write( "        <tie type=\"stop\"/>" ); sw.newLine();
                                        sw.write( "        <notations>" ); sw.newLine();
                                        sw.write( "          <tied type=\"stop\"/>" ); sw.newLine();
                                        sw.write( "        </notations>" ); sw.newLine();
                                    }
                                //}
                                sw.write( "        <lyric>" ); sw.newLine();
                                sw.write( "          <text>" + itemk.ID.LyricHandle.L0.Phrase + "</text>" ); sw.newLine();
                                sw.write( "        </lyric>" ); sw.newLine();
                                sw.write( "      </note>" ); sw.newLine();
                                clockLast = end;
                                if ( tieStartRequired ) {
                                    startIndex = k;
                                } else {
                                    startIndex = k + 1;
                                }
                            }
                        }
                        if ( clockLast < clockEnd ) {
                            // 小節の最後に休符を入れる必要がある
                            sw.write( "      <note>" ); sw.newLine();
                            sw.write( "        <rest/>" ); sw.newLine();
                            sw.write( "        <duration>" + (clockEnd - clockLast) + "</duration>" ); sw.newLine();
                            sw.write( "        <voice>1</voice>" ); sw.newLine();
                            sw.write( "      </note>" ); sw.newLine();
                            clockLast = clockEnd;
                        }
                        sw.write( "    </measure>" ); sw.newLine();
                    }
                    sw.write( "  </part>" ); sw.newLine();
                }
                sw.write( "</score-partwise>" ); sw.newLine();
            } catch ( Exception ex ) {
                PortUtil.stderr.println( "VsqFile#printAsMusicXml; ex=" + ex );
            } finally {
                if ( sw != null ) {
                    try {
                        sw.close();
                    } catch ( Exception ex2 ) {
                        PortUtil.stderr.println( "VsqFile#printAsMusicXml; ex2=" + ex2 );
                    }
                }
            }
        }

        /// <summary>
        /// master==MasterPitchControl.Pitchの場合、m_pitchからPITとPBSを再構成。
        /// master==MasterPitchControl.PITandPBSの場合、PITとPBSからm_pitchを再構成
        /// </summary>
        private static void reflectPitch( VsqFile vsq, int track, VsqBPList pitch ) {
            //double offset = AttachedCurves[track - 1].MasterTuningInCent * 100;
            //List<Integer> keyclocks = new List<Integer>( pitch.getKeys() );
            int keyclock_size = pitch.size();
            VsqBPList pit = new VsqBPList( "pit", 0, -8192, 8191 );
            VsqBPList pbs = new VsqBPList( "pbs", 2, 0, 24 );
            int premeasure_clock = vsq.getPreMeasureClocks();
            int lastpit = pit.getDefault();
            int lastpbs = pbs.getDefault();
            int vpbs = 24;
            int vpit = 0;

            List<Integer> parts = new List<Integer>();   // 連続した音符ブロックの先頭音符のクロック位置。のリスト
            parts.add( premeasure_clock );
            int lastclock = premeasure_clock;
            for ( Iterator<VsqEvent> itr = vsq.Track.get( track ).getNoteEventIterator(); itr.hasNext(); ) {
                VsqEvent ve = itr.next();
                if ( ve.Clock <= lastclock ) {
                    lastclock = Math.Max( lastclock, ve.Clock + ve.ID.getLength() );
                } else {
                    parts.add( ve.Clock );
                    lastclock = ve.Clock + ve.ID.getLength();
                }
            }

            int parts_size = parts.size();
            for ( int i = 0; i < parts_size; i++ ) {
                int partstart = parts.get( i );
                int partend = int.MaxValue;
                if ( i + 1 < parts.size() ) {
                    partend = parts.get( i + 1 );
                }

                // まず、区間内の最大ピッチベンド幅を調べる
                double max = 0;
                for ( int j = 0; j < keyclock_size; j++ ) {
                    int clock = pitch.getKeyClock( j );
                    if ( clock < partstart ) {
                        continue;
                    }
                    if ( partend <= clock ) {
                        break;
                    }
                    max = Math.Max( max, Math.Abs( pitch.getValue( clock ) / 100.0 ) );
                }

                // 最大ピッチベンド幅を表現できる最小のPBSを計算
                vpbs = (int)(Math.Ceiling( max * 8192.0 / 8191.0 ) + 0.1);
                if ( vpbs <= 0 ) {
                    vpbs = 1;
                }
                double pitch2 = pitch.getValue( partstart ) / 100.0;
                if ( lastpbs != vpbs ) {
                    pbs.add( partstart, vpbs );
                    lastpbs = vpbs;
                }
                vpit = (int)(pitch2 * 8192 / (double)vpbs);
                if ( lastpit != vpit ) {
                    pit.add( partstart, vpit );
                    lastpit = vpit;
                }
                for ( int j = 0; j < keyclock_size; j++ ) {
                    int clock = pitch.getKeyClock( j );
                    if ( clock < partstart ) {
                        continue;
                    }
                    if ( partend <= clock ) {
                        break;
                    }
                    if ( clock != partstart ) {
                        pitch2 = pitch.getElement( j ) / 100.0;
                        vpit = (int)(pitch2 * 8192 / (double)vpbs);
                        if ( lastpit != vpit ) {
                            pit.add( clock, vpit );
                            lastpit = vpit;
                        }
                    }
                }
            }
            vsq.Track.get( track ).setCurve( "pit", pit );
            vsq.Track.get( track ).setCurve( "pbs", pbs );
        }

        /// <summary>
        /// プリセンドタイムの妥当性を判定します
        /// </summary>
        /// <param name="ms_pre_send_time"></param>
        /// <returns></returns>
        public boolean checkPreSendTimeValidity( int ms_pre_send_time ) {
            int track_count = Track.size();
            for ( int i = 1; i < track_count; i++ ) {
                VsqTrack track = Track.get( i );
                for ( Iterator<VsqEvent> itr = track.getNoteEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    int presend_clock = getPresendClockAt( item.Clock, ms_pre_send_time );
                    if ( item.Clock - presend_clock < 0 ) {
                        return false;
                    }
                    break;
                }
            }
            return true;
        }

        /// <summary>
        /// テンポ値を一律order倍します。
        /// </summary>
        /// <param name="order"></param>
        public void speedingUp( double order ) {
            lock ( TempoTable ) {
                int c = TempoTable.size();
                for ( int i = 0; i < c; i++ ) {
                    TempoTable.get( i ).Tempo = (int)(TempoTable.get( i ).Tempo / order);
                }
            }
            updateTempoInfo();
        }

        /// <summary>
        /// このインスタンスに編集を行うコマンドを実行します
        /// </summary>
        /// <param name="command">実行するコマンド</param>
        /// <returns>編集結果を元に戻すためのコマンドを返します</returns>
        public VsqCommand executeCommand( VsqCommand command ) {
#if DEBUG
            PortUtil.stdout.println( "VsqFile#executeCommand(VsqCommand); type=" + command.Type );
#endif
            VsqCommandType type = command.Type;
            if ( type == VsqCommandType.CHANGE_PRE_MEASURE ) {
                #region CHANGE_PRE_MEASURE
                VsqCommand ret = VsqCommand.generateCommandChangePreMeasure( Master.PreMeasure );
                int value = (Integer)command.Args[0];
                Master.PreMeasure = value;
                updateTimesigInfo();
                return ret;
                #endregion
            } else if ( type == VsqCommandType.TRACK_ADD ) {
                #region TRACK_ADD
                VsqTrack track = (VsqTrack)command.Args[0];
                VsqMixerEntry mixer = (VsqMixerEntry)command.Args[1];
                int position = (Integer)command.Args[2];
                VsqCommand ret = VsqCommand.generateCommandDeleteTrack( position );
                if ( Track.size() <= 17 ) {
                    Track.insertElementAt( (VsqTrack)track.clone(), position );
                    Mixer.Slave.add( (VsqMixerEntry)mixer.clone() );
                    return ret;
                } else {
                    return null;
                }
                #endregion
            } else if ( type == VsqCommandType.TRACK_DELETE ) {
                #region TRACK_DELETE
                int track = (Integer)command.Args[0];
                VsqCommand ret = VsqCommand.generateCommandAddTrack( Track.get( track ), Mixer.Slave.get( track - 1 ), track );
                Track.removeElementAt( track );
                Mixer.Slave.removeElementAt( track - 1 );
                updateTotalClocks();
                return ret;
                #endregion
            } else if ( type == VsqCommandType.UPDATE_TEMPO ) {
                #region UPDATE_TEMPO
                int clock = (Integer)command.Args[0];
                int tempo = (Integer)command.Args[1];
                int new_clock = (Integer)command.Args[2];

                int index = -1;
                int c = TempoTable.size();
                for ( int i = 0; i < c; i++ ) {
                    if ( TempoTable.get( i ).Clock == clock ) {
                        index = i;
                        break;
                    }
                }
                VsqCommand ret = null;
                if ( index >= 0 ) {
                    if ( tempo <= 0 ) {
                        ret = VsqCommand.generateCommandUpdateTempo( clock, clock, TempoTable.get( index ).Tempo );
                        TempoTable.removeElementAt( index );
                    } else {
                        ret = VsqCommand.generateCommandUpdateTempo( new_clock, clock, TempoTable.get( index ).Tempo );
                        TempoTable.get( index ).Tempo = tempo;
                        TempoTable.get( index ).Clock = new_clock;
                    }
                } else {
                    ret = VsqCommand.generateCommandUpdateTempo( clock, clock, -1 );
                    TempoTable.add( new TempoTableEntry( new_clock, tempo, 0.0 ) );
                }
                updateTempoInfo();
                updateTotalClocks();
                return ret;
                #endregion
            } else if ( type == VsqCommandType.UPDATE_TEMPO_RANGE ) {
                #region UPDATE_TEMPO_RANGE
                int[] clocks = (int[])command.Args[0];
                int[] tempos = (int[])command.Args[1];
                int[] new_clocks = (int[])command.Args[2];
                int[] new_tempos = new int[tempos.Length];
                int affected_clock = int.MaxValue;
                for ( int i = 0; i < clocks.Length; i++ ) {
                    int index = -1;
                    affected_clock = Math.Min( affected_clock, clocks[i] );
                    affected_clock = Math.Min( affected_clock, new_clocks[i] );
                    int tempo_table_count = TempoTable.size();
                    for ( int j = 0; j < tempo_table_count; j++ ) {
                        if ( TempoTable.get( j ).Clock == clocks[i] ) {
                            index = j;
                            break;
                        }
                    }
                    if ( index >= 0 ) {
                        new_tempos[i] = TempoTable.get( index ).Tempo;
                        if ( tempos[i] <= 0 ) {
                            TempoTable.removeElementAt( index );
                        } else {
                            TempoTable.get( index ).Tempo = tempos[i];
                            TempoTable.get( index ).Clock = new_clocks[i];
                        }
                    } else {
                        new_tempos[i] = -1;
                        TempoTable.add( new TempoTableEntry( new_clocks[i], tempos[i], 0.0 ) );
                    }
                }
                updateTempoInfo();
                updateTotalClocks();
                return VsqCommand.generateCommandUpdateTempoRange( new_clocks, clocks, new_tempos );
                #endregion
            } else if ( type == VsqCommandType.UPDATE_TIMESIG ) {
                #region UPDATE_TIMESIG
                int barcount = (Integer)command.Args[0];
                int numerator = (Integer)command.Args[1];
                int denominator = (Integer)command.Args[2];
                int new_barcount = (Integer)command.Args[3];
                int index = -1;
                int timesig_table_count = TimesigTable.size();
                for ( int i = 0; i < timesig_table_count; i++ ) {
                    if ( barcount == TimesigTable.get( i ).BarCount ) {
                        index = i;
                        break;
                    }
                }
                VsqCommand ret = null;
                if ( index >= 0 ) {
                    if ( numerator <= 0 ) {
                        ret = VsqCommand.generateCommandUpdateTimesig( barcount, barcount, TimesigTable.get( index ).Numerator, TimesigTable.get( index ).Denominator );
                        TimesigTable.removeElementAt( index );
                    } else {
                        ret = VsqCommand.generateCommandUpdateTimesig( new_barcount, barcount, TimesigTable.get( index ).Numerator, TimesigTable.get( index ).Denominator );
                        TimesigTable.get( index ).BarCount = new_barcount;
                        TimesigTable.get( index ).Numerator = numerator;
                        TimesigTable.get( index ).Denominator = denominator;
                    }
                } else {
                    ret = VsqCommand.generateCommandUpdateTimesig( new_barcount, new_barcount, -1, -1 );
                    TimesigTable.add( new TimeSigTableEntry( 0, numerator, denominator, new_barcount ) );
                }
                updateTimesigInfo();
                updateTotalClocks();
                return ret;
                #endregion
            } else if ( type == VsqCommandType.UPDATE_TIMESIG_RANGE ) {
                #region UPDATE_TIMESIG_RANGE
                int[] barcounts = (int[])command.Args[0];
                int[] numerators = (int[])command.Args[1];
                int[] denominators = (int[])command.Args[2];
                int[] new_barcounts = (int[])command.Args[3];
                int[] new_numerators = new int[numerators.Length];
                int[] new_denominators = new int[denominators.Length];
                for ( int i = 0; i < barcounts.Length; i++ ) {
                    int index = -1;
                    // すでに拍子が登録されているかどうかを検査
                    int timesig_table_count = TimesigTable.size();
                    for ( int j = 0; j < timesig_table_count; j++ ) {
                        if ( TimesigTable.get( j ).BarCount == barcounts[i] ) {
                            index = j;
                            break;
                        }
                    }
                    if ( index >= 0 ) {
                        // 登録されている場合
                        new_numerators[i] = TimesigTable.get( index ).Numerator;
                        new_denominators[i] = TimesigTable.get( index ).Denominator;
                        if ( numerators[i] <= 0 ) {
                            TimesigTable.removeElementAt( index );
                        } else {
                            TimesigTable.get( index ).BarCount = new_barcounts[i];
                            TimesigTable.get( index ).Numerator = numerators[i];
                            TimesigTable.get( index ).Denominator = denominators[i];
                        }
                    } else {
                        // 登録されていない場合
                        new_numerators[i] = -1;
                        new_denominators[i] = -1;
                        TimesigTable.add( new TimeSigTableEntry( 0, numerators[i], denominators[i], new_barcounts[i] ) );
                    }
                }
                updateTimesigInfo();
                updateTotalClocks();
                return VsqCommand.generateCommandUpdateTimesigRange( new_barcounts, barcounts, new_numerators, new_denominators );
                #endregion
            } else if ( type == VsqCommandType.REPLACE ) {
                #region REPLACE
                VsqFile vsq = (VsqFile)command.Args[0];
                VsqFile inv = (VsqFile)this.clone();
                Track.clear();
                int track_count = vsq.Track.size();
                for ( int i = 0; i < track_count; i++ ) {
                    Track.add( (VsqTrack)vsq.Track.get( i ).clone() );
                }

                TempoTable.clear();
                int tempo_table_count = vsq.TempoTable.size();
                for ( int i = 0; i < tempo_table_count; i++ ) {
                    TempoTable.add( (TempoTableEntry)vsq.TempoTable.get( i ).clone() );
                }

                TimesigTable.clear();
                int timesig_table_count = vsq.TimesigTable.size();
                for ( int i = 0; i < timesig_table_count; i++ ) {
                    TimesigTable.add( (TimeSigTableEntry)vsq.TimesigTable.get( i ).clone() );
                }
                //m_tpq = vsq.m_tpq;
                TotalClocks = vsq.TotalClocks;
                //m_base_tempo = vsq.m_base_tempo;
                Master = (VsqMaster)vsq.Master.clone();
                Mixer = (VsqMixer)vsq.Mixer.clone();
                updateTotalClocks();
                return VsqCommand.generateCommandReplace( inv );
                #endregion
            } else if ( type == VsqCommandType.EVENT_ADD ) {
                #region EVENT_ADD
                int track = (Integer)command.Args[0];
                VsqEvent item = (VsqEvent)command.Args[1];
                Track.get( track ).addEvent( item );
                VsqCommand ret = VsqCommand.generateCommandEventDelete( track, item.InternalID );
                updateTotalClocks();
                Track.get( track ).sortEvent();
                return ret;
                #endregion
            } else if ( type == VsqCommandType.EVENT_ADD_RANGE ) {
                #region EVENT_ADD_RANGE
#if DEBUG
                PortUtil.stdout.println( "    TrackAddNoteRange" );
#endif
                int track = (Integer)command.Args[0];
                VsqEvent[] items = (VsqEvent[])command.Args[1];
                List<Integer> inv_ids = new List<Integer>();
                int min_clock = (int)TotalClocks;
                int max_clock = 0;
                VsqTrack target = Track.get( track );
                for ( int i = 0; i < items.Length; i++ ) {
                    VsqEvent item = (VsqEvent)items[i].clone();
                    min_clock = Math.Min( min_clock, item.Clock );
                    max_clock = Math.Max( max_clock, item.Clock + item.ID.getLength() );
#if DEBUG
                    PortUtil.stdout.println( "        i=" + i + "; item.InternalID=" + item.InternalID );
#endif
                    target.addEvent( item );
                    inv_ids.add( item.InternalID );
#if DEBUG
                    PortUtil.stdout.println( " => " + item.InternalID );
#endif
                }
                updateTotalClocks();
                target.sortEvent();
                return VsqCommand.generateCommandEventDeleteRange( track, inv_ids );
                #endregion
            } else if ( type == VsqCommandType.EVENT_DELETE ) {
                #region EVENT_DELETE
                int track = (Integer)command.Args[0];
                int internal_id = (Integer)command.Args[1];
                VsqEvent[] original = new VsqEvent[1];
                VsqTrack target = Track.get( track );
                for ( Iterator<VsqEvent> itr = target.getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.InternalID == internal_id ) {
                        original[0] = (VsqEvent)item.clone();
                        break;
                    }
                }
                VsqCommand ret = VsqCommand.generateCommandEventAddRange( track, original );
                int count = target.getEventCount();
                for ( int i = 0; i < count; i++ ) {
                    if ( target.getEvent( i ).InternalID == internal_id ) {
                        target.removeEvent( i );
                        break;
                    }
                }
                updateTotalClocks();
                return ret;
                #endregion
            } else if ( type == VsqCommandType.EVENT_DELETE_RANGE ) {
                #region EVENT_DELETE_RANGE
                List<Integer> internal_ids = (List<Integer>)command.Args[1];
                int track = (Integer)command.Args[0];
                List<VsqEvent> inv = new List<VsqEvent>();
                int min_clock = int.MaxValue;
                int max_clock = int.MinValue;
                VsqTrack target = this.Track.get( track );
                int count = internal_ids.size();
                for ( int j = 0; j < count; j++ ) {
                    for ( int i = 0; i < target.getEventCount(); i++ ) {
                        VsqEvent item = target.getEvent( i );
                        if ( internal_ids.get( j ) == item.InternalID ) {
                            inv.add( (VsqEvent)item.clone() );
                            min_clock = Math.Min( min_clock, item.Clock );
                            max_clock = Math.Max( max_clock, item.Clock + item.ID.getLength() );
                            target.removeEvent( i );
                            break;
                        }
                    }
                }
                updateTotalClocks();
                return VsqCommand.generateCommandEventAddRange( track, inv.toArray( new VsqEvent[] { } ) );
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_CLOCK ) {
                #region EVENT_CHANGE_CLOCK
                int track = (Integer)command.Args[0];
                int internal_id = (Integer)command.Args[1];
                int value = (Integer)command.Args[2];
                VsqTrack target = this.Track.get( track );
                for ( Iterator<VsqEvent> itr = target.getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.InternalID == internal_id ) {
                        VsqCommand ret = VsqCommand.generateCommandEventChangeClock( track, internal_id, item.Clock );
                        int min = Math.Min( item.Clock, value );
                        int max = Math.Max( item.Clock + item.ID.getLength(), value + item.ID.getLength() );
                        item.Clock = value;
                        updateTotalClocks();
                        target.sortEvent();
                        return ret;
                    }
                }
                return null;
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_LYRIC ) {
                #region EVENT_CHANGE_LYRIC
                int track = (Integer)command.Args[0];
                int internal_id = (Integer)command.Args[1];
                String phrase = (String)command.Args[2];
                String phonetic_symbol = (String)command.Args[3];
                boolean protect_symbol = (Boolean)command.Args[4];
                VsqTrack target = this.Track.get( track );
                for ( Iterator<VsqEvent> itr = target.getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.InternalID == internal_id ) {
                        if ( item.ID.type == VsqIDType.Anote ) {
                            VsqCommand ret = VsqCommand.generateCommandEventChangeLyric( track, internal_id, item.ID.LyricHandle.L0.Phrase, item.ID.LyricHandle.L0.getPhoneticSymbol(), item.ID.LyricHandle.L0.PhoneticSymbolProtected );
                            item.ID.LyricHandle.L0.Phrase = phrase;
                            item.ID.LyricHandle.L0.setPhoneticSymbol( phonetic_symbol );
                            item.ID.LyricHandle.L0.PhoneticSymbolProtected = protect_symbol;
                            updateTotalClocks();
                            return ret;
                        }
                    }
                }
                return null;
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_NOTE ) {
                #region EVENT_CHANGE_NOTE
                int track = (Integer)command.Args[0];
                int internal_id = (Integer)command.Args[1];
                int note = (Integer)command.Args[2];
                VsqTrack target = this.Track.get( track );
                for ( Iterator<VsqEvent> itr = target.getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.InternalID == internal_id ) {
                        VsqCommand ret = VsqCommand.generateCommandEventChangeNote( track, internal_id, item.ID.Note );
                        item.ID.Note = note;
                        updateTotalClocks();
                        return ret;
                    }
                }
                return null;
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_CLOCK_AND_NOTE ) {
                #region EVENT_CHANGE_CLOCK_AND_NOTE
                int track = (Integer)command.Args[0];
                int internal_id = (Integer)command.Args[1];
                int clock = (Integer)command.Args[2];
                int note = (Integer)command.Args[3];
                VsqTrack target = this.Track.get( track );
                for ( Iterator<VsqEvent> itr = target.getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.InternalID == internal_id ) {
                        VsqCommand ret = VsqCommand.generateCommandEventChangeClockAndNote( track, internal_id, item.Clock, item.ID.Note );
                        int min = Math.Min( item.Clock, clock );
                        int max = Math.Max( item.Clock + item.ID.getLength(), clock + item.ID.getLength() );
                        item.Clock = clock;
                        item.ID.Note = note;
                        target.sortEvent();
                        updateTotalClocks();
                        return ret;
                    }
                }
                return null;
                #endregion
            } else if ( type == VsqCommandType.TRACK_CURVE_EDIT ) {
                #region TRACK_CURVE_EDIT
                int track = (Integer)command.Args[0];
                String curve = (String)command.Args[1];
                List<BPPair> com = (List<BPPair>)command.Args[2];
                VsqCommand inv = null;
                List<BPPair> edit = new List<BPPair>();
                VsqBPList target_list = Track.get( track ).getCurve( curve );
                if ( com != null ) {
                    if ( com.size() > 0 ) {
                        int start_clock = com.get( 0 ).Clock;
                        int end_clock = com.get( 0 ).Clock;
                        for ( Iterator<BPPair> itr = com.iterator(); itr.hasNext(); ) {
                            BPPair item = itr.next();
                            start_clock = Math.Min( start_clock, item.Clock );
                            end_clock = Math.Max( end_clock, item.Clock );
                        }
                        int start_value = target_list.getValue( start_clock );
                        int end_value = target_list.getValue( end_clock );
                        for ( Iterator<Integer> i = target_list.keyClockIterator(); i.hasNext(); ) {
                            int clock = i.next();
                            if ( start_clock <= clock && clock <= end_clock ) {
                                edit.add( new BPPair( clock, target_list.getValue( clock ) ) );
                            }
                        }
                        boolean start_found = false;
                        boolean end_found = false;
                        int count = edit.size();
                        for ( int i = 0; i < count; i++ ) {
                            if ( edit.get( i ).Clock == start_clock ) {
                                start_found = true;
                                edit.get( i ).Value = start_value;
                                if ( start_found && end_found ) {
                                    break;
                                }
                            }
                            if ( edit.get( i ).Clock == end_clock ) {
                                end_found = true;
                                edit.get( i ).Value = end_value;
                                if ( start_found && end_found ) {
                                    break;
                                }
                            }
                        }
                        if ( !start_found ) {
                            edit.add( new BPPair( start_clock, start_value ) );
                        }
                        if ( !end_found ) {
                            edit.add( new BPPair( end_clock, end_value ) );
                        }

                        // 並べ替え
                        Collections.sort( edit );
                        inv = VsqCommand.generateCommandTrackCurveEdit( track, curve, edit );
                    } else if ( com.size() == 0 ) {
                        inv = VsqCommand.generateCommandTrackCurveEdit( track, curve, new List<BPPair>() );
                    }
                }

                updateTotalClocks();
                if ( com.size() == 0 ) {
                    return inv;
                } else if ( com.size() == 1 ) {
                    boolean found = false;
                    for ( Iterator<Integer> itr = target_list.keyClockIterator(); itr.hasNext(); ) {
                        int clock = itr.next();
                        if ( clock == com.get( 0 ).Clock ) {
                            found = true;
                            target_list.add( clock, com.get( 0 ).Value );
                            break;
                        }
                    }
                    if ( !found ) {
                        target_list.add( com.get( 0 ).Clock, com.get( 0 ).Value );
                    }
                } else {
                    int start_clock = com.get( 0 ).Clock;
                    int end_clock = com.get( com.size() - 1 ).Clock;
                    boolean removed = true;
                    while ( removed ) {
                        removed = false;
                        for ( Iterator<Integer> itr = target_list.keyClockIterator(); itr.hasNext(); ) {
                            int clock = itr.next();
                            if ( start_clock <= clock && clock <= end_clock ) {
                                target_list.remove( clock );
                                removed = true;
                                break;
                            }
                        }
                    }
                    for ( Iterator<BPPair> itr = com.iterator(); itr.hasNext(); ) {
                        BPPair item = itr.next();
                        target_list.add( item.Clock, item.Value );
                    }
                }
                return inv;
                #endregion
            } else if ( type == VsqCommandType.TRACK_CURVE_EDIT2 ) {
                #region TRACK_CURVE_EDIT2
                int track = (Integer)command.Args[0];
                String curve = (String)command.Args[1];
                List<Long> delete = (List<Long>)command.Args[2];
                Dictionary<Integer, VsqBPPair> add = (Dictionary<Integer, VsqBPPair>)command.Args[3];

                List<Long> inv_delete = new List<Long>();
                Dictionary<Integer, VsqBPPair> inv_add = new Dictionary<Integer, VsqBPPair>();
                processTrackCurveEdit( track, curve, delete, add, inv_delete, inv_add );
                updateTotalClocks();

                return VsqCommand.generateCommandTrackCurveEdit2( track, curve, inv_delete, inv_add );
                #endregion
            } else if ( type == VsqCommandType.TRACK_CURVE_EDIT2_ALL ) {
                #region TRACK_CURVE_EDIT2_ALL
                int track = (Integer)command.Args[0];
                List<String> curve = (List<String>)command.Args[1];
                List<List<Long>> delete = (List<List<Long>>)command.Args[2];
                List<Dictionary<Integer, VsqBPPair>> add = (List<Dictionary<Integer, VsqBPPair>>)command.Args[3];

                int c = curve.size();
                List<List<Long>> inv_delete = new List<List<Long>>();
                List<Dictionary<Integer, VsqBPPair>> inv_add = new List<Dictionary<Integer, VsqBPPair>>();
                for ( int i = 0; i < c; i++ ) {
                    List<Long> part_inv_delete = new List<Long>();
                    Dictionary<Integer, VsqBPPair> part_inv_add = new Dictionary<Integer, VsqBPPair>();
                    processTrackCurveEdit( track, curve.get( i ), delete.get( i ), add.get( i ), part_inv_delete, part_inv_add );
                    inv_delete.add( part_inv_delete );
                    inv_add.add( part_inv_add );
                }
                updateTotalClocks();

                return VsqCommand.generateCommandTrackCurveEdit2All( track, curve, inv_delete, inv_add );
                #endregion
            } else if ( type == VsqCommandType.TRACK_CURVE_REPLACE ) {
                #region TRACK_CURVE_REPLACE
                int track = (Integer)command.Args[0];
                String target_curve = (String)command.Args[1];
                VsqBPList bplist = (VsqBPList)command.Args[2];
                VsqCommand inv = VsqCommand.generateCommandTrackCurveReplace( track, target_curve, Track.get( track ).getCurve( target_curve ) );
                Track.get( track ).setCurve( target_curve, bplist );
                return inv;
                #endregion
            } else if ( type == VsqCommandType.TRACK_CURVE_REPLACE_RANGE ) {
                #region TRACK_CURVE_REPLACE_RANGE
                int track = (Integer)command.Args[0];
                String[] target_curve = (String[])command.Args[1];
                VsqBPList[] bplist = (VsqBPList[])command.Args[2];
                VsqBPList[] inv_bplist = new VsqBPList[bplist.Length];
                VsqTrack work = Track.get( track );
                for ( int i = 0; i < target_curve.Length; i++ ) {
                    inv_bplist[i] = work.getCurve( target_curve[i] );
                }
                VsqCommand inv = VsqCommand.generateCommandTrackCurveReplaceRange( track, target_curve, inv_bplist );
                for ( int i = 0; i < target_curve.Length; i++ ) {
                    work.setCurve( target_curve[i], bplist[i] );
                }
                return inv;
                #endregion
            } else if ( type == VsqCommandType.TRACK_CURVE_EDIT_RANGE ) {
                #region TRACK_CURVE_EDIT_RANGE
                int track = (Integer)command.Args[0];
                List<String> curves = (List<String>)command.Args[1];
                List<List<BPPair>> coms = (List<List<BPPair>>)command.Args[2];
                List<List<BPPair>> inv_coms = new List<List<BPPair>>();
                VsqCommand inv = null;

                int count = curves.size();
                for ( int k = 0; k < count; k++ ) {
                    String curve = curves.get( k );
                    List<BPPair> com = coms.get( k );
                    //SortedList<int, int> list = Tracks[track][curve].List;
                    List<BPPair> edit = new List<BPPair>();
                    if ( com != null ) {
                        if ( com.size() > 0 ) {
                            int start_clock = com.get( 0 ).Clock;
                            int end_clock = com.get( 0 ).Clock;
                            for ( Iterator<BPPair> itr = com.iterator(); itr.hasNext(); ) {
                                BPPair item = itr.next();
                                start_clock = Math.Min( start_clock, item.Clock );
                                end_clock = Math.Max( end_clock, item.Clock );
                            }
                            int start_value = Track.get( track ).getCurve( curve ).getValue( start_clock );
                            int end_value = Track.get( track ).getCurve( curve ).getValue( end_clock );
                            for ( Iterator<Integer> itr = Track.get( track ).getCurve( curve ).keyClockIterator(); itr.hasNext(); ) {
                                int clock = itr.next();
                                if ( start_clock <= clock && clock <= end_clock ) {
                                    edit.add( new BPPair( clock, Track.get( track ).getCurve( curve ).getValue( clock ) ) );
                                }
                            }
                            boolean start_found = false;
                            boolean end_found = false;
                            for ( int i = 0; i < edit.size(); i++ ) {
                                if ( edit.get( i ).Clock == start_clock ) {
                                    start_found = true;
                                    edit.get( i ).Value = start_value;
                                    if ( start_found && end_found ) {
                                        break;
                                    }
                                }
                                if ( edit.get( i ).Clock == end_clock ) {
                                    end_found = true;
                                    edit.get( i ).Value = end_value;
                                    if ( start_found && end_found ) {
                                        break;
                                    }
                                }
                            }
                            if ( !start_found ) {
                                edit.add( new BPPair( start_clock, start_value ) );
                            }
                            if ( !end_found ) {
                                edit.add( new BPPair( end_clock, end_value ) );
                            }

                            // 並べ替え
                            Collections.sort( edit );
                            inv_coms.add( edit );
                            //inv = generateCommandTrackEditCurve( track, curve, edit );
                        } else if ( com.size() == 0 ) {
                            //inv = generateCommandTrackEditCurve( track, curve, new List<BPPair>() );
                            inv_coms.add( new List<BPPair>() );
                        }
                    }

                    updateTotalClocks();
                    if ( com.size() == 0 ) {
                        return inv;
                    } else if ( com.size() == 1 ) {
                        boolean found = false;
                        for ( Iterator<Integer> itr = Track.get( track ).getCurve( curve ).keyClockIterator(); itr.hasNext(); ) {
                            int clock = itr.next();
                            if ( clock == com.get( 0 ).Clock ) {
                                found = true;
                                Track.get( track ).getCurve( curve ).add( clock, com.get( 0 ).Value );
                                break;
                            }
                        }
                        if ( !found ) {
                            Track.get( track ).getCurve( curve ).add( com.get( 0 ).Clock, com.get( 0 ).Value );
                        }
                    } else {
                        int start_clock = com.get( 0 ).Clock;
                        int end_clock = com.get( com.size() - 1 ).Clock;
                        boolean removed = true;
                        while ( removed ) {
                            removed = false;
                            for ( Iterator<Integer> itr = Track.get( track ).getCurve( curve ).keyClockIterator(); itr.hasNext(); ) {
                                int clock = itr.next();
                                if ( start_clock <= clock && clock <= end_clock ) {
                                    Track.get( track ).getCurve( curve ).remove( clock );
                                    removed = true;
                                    break;
                                }
                            }
                        }
                        for ( Iterator<BPPair> itr = com.iterator(); itr.hasNext(); ) {
                            BPPair item = itr.next();
                            Track.get( track ).getCurve( curve ).add( item.Clock, item.Value );
                        }
                    }
                }
                return VsqCommand.generateCommandTrackCurveEditRange( track, curves, inv_coms );
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_VELOCITY ) {
                #region EVENT_CHANGE_VELOCITY
                int track = (Integer)command.Args[0];
                List<ValuePair<Integer, Integer>> veloc = (List<ValuePair<Integer, Integer>>)command.Args[1];
                List<ValuePair<Integer, Integer>> inv = new List<ValuePair<Integer, Integer>>();
                for ( Iterator<VsqEvent> itr = Track.get( track ).getEventIterator(); itr.hasNext(); ) {
                    VsqEvent ev = itr.next();
                    for ( Iterator<ValuePair<Integer, Integer>> itr2 = veloc.iterator(); itr2.hasNext(); ) {
                        ValuePair<Integer, Integer> add = itr2.next();
                        if ( ev.InternalID == add.getKey() ) {
                            inv.add( new ValuePair<Integer, Integer>( ev.InternalID, ev.ID.Dynamics ) );
                            ev.ID.Dynamics = add.getValue();
                            break;
                        }
                    }
                }
                return VsqCommand.generateCommandEventChangeVelocity( track, inv );
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_ACCENT ) {
                #region EVENT_CHANGE_ACCENT
                int track = (Integer)command.Args[0];
                List<ValuePair<Integer, Integer>> veloc = (List<ValuePair<Integer, Integer>>)command.Args[1];
                List<ValuePair<Integer, Integer>> inv = new List<ValuePair<Integer, Integer>>();
                for ( Iterator<VsqEvent> itr = Track.get( track ).getEventIterator(); itr.hasNext(); ) {
                    VsqEvent ev = itr.next();
                    for ( Iterator<ValuePair<Integer, Integer>> itr2 = veloc.iterator(); itr2.hasNext(); ) {
                        ValuePair<Integer, Integer> add = itr2.next();
                        if ( ev.InternalID == add.getKey() ) {
                            inv.add( new ValuePair<Integer, Integer>( ev.InternalID, ev.ID.DEMaccent ) );
                            ev.ID.DEMaccent = add.getValue();
                            break;
                        }
                    }
                }
                return VsqCommand.generateCommandEventChangeAccent( track, inv );
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_DECAY ) {
                #region EVENT_CHANGE_DECAY
                int track = (Integer)command.Args[0];
                List<ValuePair<Integer, Integer>> veloc = (List<ValuePair<Integer, Integer>>)command.Args[1];
                List<ValuePair<Integer, Integer>> inv = new List<ValuePair<Integer, Integer>>();
                for ( Iterator<VsqEvent> itr = Track.get( track ).getEventIterator(); itr.hasNext(); ) {
                    VsqEvent ev = itr.next();
                    for ( Iterator<ValuePair<Integer, Integer>> itr2 = veloc.iterator(); itr2.hasNext(); ) {
                        ValuePair<Integer, Integer> add = itr2.next();
                        if ( ev.InternalID == add.getKey() ) {
                            inv.add( new ValuePair<Integer, Integer>( ev.InternalID, ev.ID.DEMdecGainRate ) );
                            ev.ID.DEMdecGainRate = add.getValue();
                            break;
                        }
                    }
                }
                return VsqCommand.generateCommandEventChangeDecay( track, inv );
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_LENGTH ) {
                #region EVENT_CHANGE_LENGTH
                int track = (Integer)command.Args[0];
                int internal_id = (Integer)command.Args[1];
                int new_length = (Integer)command.Args[2];
                for ( Iterator<VsqEvent> itr = Track.get( track ).getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.InternalID == internal_id ) {
                        VsqCommand ret = VsqCommand.generateCommandEventChangeLength( track, internal_id, item.ID.getLength() );
                        item.ID.setLength( new_length );
                        updateTotalClocks();
                        return ret;
                    }
                }
                return null;
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_CLOCK_AND_LENGTH ) {
                #region EVENT_CHANGE_CLOCK_AND_LENGTH
                int track = (Integer)command.Args[0];
                int internal_id = (Integer)command.Args[1];
                int new_clock = (Integer)command.Args[2];
                int new_length = (Integer)command.Args[3];
                for ( Iterator<VsqEvent> itr = Track.get( track ).getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.InternalID == internal_id ) {
                        VsqCommand ret = VsqCommand.generateCommandEventChangeClockAndLength( track, internal_id, item.Clock, item.ID.getLength() );
                        int min = Math.Min( item.Clock, new_clock );
                        int max_length = Math.Max( item.ID.getLength(), new_length );
                        int max = Math.Max( item.Clock + max_length, new_clock + max_length );
                        item.ID.setLength( new_length );
                        item.Clock = new_clock;
                        Track.get( track ).sortEvent();
                        updateTotalClocks();
                        return ret;
                    }
                }
                return null;
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_ID_CONTAINTS ) {
                #region EVENT_CHANGE_ID_CONTAINTS
                int track = (Integer)command.Args[0];
                int internal_id = (Integer)command.Args[1];
                VsqID value = (VsqID)command.Args[2];
                for ( Iterator<VsqEvent> itr = Track.get( track ).getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.InternalID == internal_id ) {
                        VsqCommand ret = VsqCommand.generateCommandEventChangeIDContaints( track, internal_id, item.ID );
                        int max_length = Math.Max( item.ID.getLength(), value.getLength() );
                        item.ID = (VsqID)value.clone();
                        updateTotalClocks();
                        return ret;
                    }
                }
                return null;
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_ID_CONTAINTS_RANGE ) {
                #region EVENT_CHANGE_ID_CONTAINTS_RANGE
                int track = (Integer)command.Args[0];
                int[] internal_ids = (int[])command.Args[1];
                VsqID[] values = (VsqID[])command.Args[2];
                VsqID[] inv_values = new VsqID[values.Length];
                for ( int i = 0; i < internal_ids.Length; i++ ) {
                    for ( Iterator<VsqEvent> itr = Track.get( track ).getEventIterator(); itr.hasNext(); ) {
                        VsqEvent item = itr.next();
                        if ( item.InternalID == internal_ids[i] ) {
                            inv_values[i] = (VsqID)item.ID.clone();
                            int max_length = Math.Max( item.ID.getLength(), values[i].getLength() );
                            item.ID = (VsqID)values[i].clone();
                            break;
                        }
                    }
                }
                updateTotalClocks();
                return VsqCommand.generateCommandEventChangeIDContaintsRange( track, internal_ids, inv_values );
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_CLOCK_AND_ID_CONTAINTS ) {
                #region EVENT_CHANGE_CLOCK_AND_ID_CONTAINTS
                int track = (Integer)command.Args[0];
                int internal_id = (Integer)command.Args[1];
                int new_clock = (Integer)command.Args[2];
                VsqID value = (VsqID)command.Args[3];
                VsqTrack target = Track.get( track );
                for ( Iterator<VsqEvent> itr = target.getEventIterator(); itr.hasNext(); ) {
                    VsqEvent item = itr.next();
                    if ( item.InternalID == internal_id ) {
                        VsqCommand ret = VsqCommand.generateCommandEventChangeClockAndIDContaints( track, internal_id, item.Clock, item.ID );
                        int max_length = Math.Max( item.ID.getLength(), value.getLength() );
                        int min = Math.Min( item.Clock, new_clock );
                        int max = Math.Max( item.Clock + max_length, new_clock + max_length );
                        item.ID = (VsqID)value.clone();
                        item.Clock = new_clock;
                        target.sortEvent();
                        updateTotalClocks();
                        return ret;
                    }
                }
                return null;
                #endregion
            } else if ( type == VsqCommandType.EVENT_CHANGE_CLOCK_AND_ID_CONTAINTS_RANGE ) {
                #region EVENT_CHANGE_CLOCK_AND_ID_CONTAINTS_RANGE
                int track = (Integer)command.Args[0];
                int[] internal_ids = (int[])command.Args[1];
                int[] clocks = (int[])command.Args[2];
                VsqID[] values = (VsqID[])command.Args[3];
                List<VsqID> inv_id = new List<VsqID>();
                List<Integer> inv_clock = new List<Integer>();
                for ( int i = 0; i < internal_ids.Length; i++ ) {
                    for ( Iterator<VsqEvent> itr = Track.get( track ).getEventIterator(); itr.hasNext(); ) {
                        VsqEvent item = itr.next();
                        if ( item.InternalID == internal_ids[i] ) {
                            inv_id.add( (VsqID)item.ID.clone() );
                            inv_clock.add( item.Clock );
                            int max_length = Math.Max( item.ID.getLength(), values[i].getLength() );
                            int min = Math.Min( item.Clock, clocks[i] );
                            int max = Math.Max( item.Clock + max_length, clocks[i] + max_length );
                            item.ID = (VsqID)values[i].clone();
                            item.Clock = clocks[i];
                            break;
                        }
                    }
                }
                Track.get( track ).sortEvent();
                updateTotalClocks();
                return VsqCommand.generateCommandEventChangeClockAndIDContaintsRange(
                    track,
                    internal_ids,
                    PortUtil.convertIntArray( inv_clock.toArray( new Integer[] { } ) ),
                    inv_id.toArray( new VsqID[] { } ) );
                #endregion
            } else if ( type == VsqCommandType.TRACK_CHANGE_NAME ) {
                #region TRACK_CHANGE_NAME
                int track = (Integer)command.Args[0];
                String new_name = (String)command.Args[1];
                VsqCommand ret = VsqCommand.generateCommandTrackChangeName( track, Track.get( track ).getName() );
                Track.get( track ).setName( new_name );
                return ret;
                #endregion
            } else if ( type == VsqCommandType.TRACK_REPLACE ) {
                #region TRACK_REPLACE
                int track = (Integer)command.Args[0];
                VsqTrack item = (VsqTrack)command.Args[1];
                VsqCommand ret = VsqCommand.generateCommandTrackReplace( track, Track.get( track ) );
                Track.set( track, item );
                updateTotalClocks();
                return ret;
                #endregion
            } else if ( type == VsqCommandType.TRACK_CHANGE_PLAY_MODE ) {
                #region TRACK_CHANGE_PLAY_MODE
                int track = (Integer)command.Args[0];
                int play_mode = (Integer)command.Args[1];
                int last_play_mode = (Integer)command.Args[2];
                VsqTrack vsqTrack = Track.get( track );
                VsqCommand ret = VsqCommand.generateCommandTrackChangePlayMode( track, vsqTrack.getCommon().PlayMode, vsqTrack.getCommon().LastPlayMode );
                vsqTrack.getCommon().PlayMode = play_mode;
                vsqTrack.getCommon().LastPlayMode = last_play_mode;
                return ret;
                #endregion
            } else if ( type == VsqCommandType.EVENT_REPLACE ) {
                #region EVENT_REPLACE
                int track = (Integer)command.Args[0];
                VsqEvent item = (VsqEvent)command.Args[1];
                VsqCommand ret = null;
                for ( int i = 0; i < Track.get( track ).getEventCount(); i++ ) {
                    VsqEvent ve = Track.get( track ).getEvent( i );
                    if ( ve.InternalID == item.InternalID ) {
                        ret = VsqCommand.generateCommandEventReplace( track, ve );
                        Track.get( track ).setEvent( i, item );
                        break;
                    }
                }
                Track.get( track ).sortEvent();
                updateTotalClocks();
                return ret;
                #endregion
            } else if ( type == VsqCommandType.EVENT_REPLACE_RANGE ) {
                #region EVENT_REPLACE_RANGE
                int track = (Integer)command.Args[0];
                VsqEvent[] items = (VsqEvent[])command.Args[1];
                VsqCommand ret = null;
                VsqEvent[] reverse = new VsqEvent[items.Length];
                for ( int i = 0; i < items.Length; i++ ) {
                    VsqEvent ve = items[i];
                    for ( int j = 0; j < Track.get( track ).getEventCount(); j++ ) {
                        VsqEvent ve2 = (VsqEvent)Track.get( track ).getEvent( j );
                        if ( ve2.InternalID == ve.InternalID ) {
                            reverse[i] = (VsqEvent)ve2.clone();
                            Track.get( track ).setEvent( j, items[i] );
                            break;
                        }
                    }
                }
                Track.get( track ).sortEvent();
                updateTotalClocks();
                ret = VsqCommand.generateCommandEventReplaceRange( track, reverse );
                return ret;
                #endregion
            }

            return null;
        }

        private void processTrackCurveEdit( int track, 
                                            String curve, 
                                            List<Long> delete,
                                            Dictionary<Integer, VsqBPPair> add,
                                            List<Long> inv_delete,
                                            Dictionary<Integer, VsqBPPair> inv_add ){
            VsqBPList list = Track.get( track ).getCurve( curve );

            // 逆コマンド発行用
            inv_delete.clear();
            inv_add.clear();

            // 最初に削除コマンドを実行
            for ( Iterator<Long> itr = delete.iterator(); itr.hasNext(); ) {
                long id = itr.next();
                VsqBPPairSearchContext item = list.findElement( id );
                if ( item.index >= 0 ) {
                    int clock = item.clock;
                    list.removeElementAt( item.index );
                    inv_add.put( clock, new VsqBPPair( item.point.value, item.point.id ) );
                }
            }

            // 追加コマンドを実行
            for ( Iterator<Integer> itr = add.keySet().iterator(); itr.hasNext(); ) {
                int clock = itr.next();
                VsqBPPair item = add.get( clock );
                list.addWithID( clock, item.value, item.id );
                inv_delete.add( item.id );
            }
        }

        /// <summary>
        /// VSQファイルの指定されたクロック範囲のイベント等を削除します
        /// </summary>
        /// <param name="vsq">編集対象のVsqFileインスタンス</param>
        /// <param name="clock_start">削除を行う範囲の開始クロック</param>
        /// <param name="clock_end">削除を行う範囲の終了クロック</param>
        public void removePart( int clock_start, int clock_end ) {
#if DEBUG
            PortUtil.println( "VsqFile#removePart; before:" );
            for ( int i = 0; i < TempoTable.size(); i++ ) {
                PortUtil.println( "    c" + TempoTable.get( i ).Clock + ", s" + TempoTable.get( i ).Time + ", t" + TempoTable.get( i ).Tempo );
            }
#endif
            int dclock = clock_end - clock_start;

            // テンポ情報の削除、シフト
            int tempoAtClockEnd = getTempoAt( clock_end );
            boolean changed = true;
            for( int i = 0; i < TempoTable.size() ; ){
                TempoTableEntry itemi = TempoTable.get( i );
                if ( clock_start <= itemi.Clock && itemi.Clock < clock_end ) {
                    TempoTable.removeElementAt( i );
                } else {
                    if ( clock_end < itemi.Clock ) {
                        itemi.Clock -= dclock;
                    }
                    i++;
                }
            }
            // clock_end => clock_startに変わるので，この位置におけるテンポ変更が欠けてないかどうかを検査
            int count = TempoTable.size();
            boolean contains_clock_start_tempo = false;
            for ( int i = 0; i < count; i++ ) {
                TempoTableEntry itemi = TempoTable.get( i );
                if ( itemi.Clock == clock_start ) {
                    itemi.Tempo = tempoAtClockEnd;
                    contains_clock_start_tempo = true;
                    break;
                }
            }
            if ( !contains_clock_start_tempo ) {
                TempoTable.add( new TempoTableEntry( clock_start, tempoAtClockEnd, 0.0 ) );
            }
            updateTempoInfo();

            int numTrack = Track.size();
            for ( int track = 1; track < numTrack; track++ ) {
                VsqTrack vsqTrack = Track.get( track );
                // 削除する範囲に歌手変更イベントが存在するかどうかを検査。
                VsqEvent t_last_singer = null;
                for ( Iterator<VsqEvent> itr = vsqTrack.getSingerEventIterator(); itr.hasNext(); ) {
                    VsqEvent ve = itr.next();
                    if ( clock_start <= ve.Clock && ve.Clock < clock_end ) {
                        t_last_singer = ve;
                    }
                    if ( ve.Clock == clock_end ) {
                        t_last_singer = null; // 後でclock_endの位置に補うが、そこにに既に歌手変更イベントがあるとまずいので。
                    }
                }
                VsqEvent last_singer = null;
                if ( t_last_singer != null ) {
                    last_singer = (VsqEvent)t_last_singer.clone();
                    last_singer.Clock = clock_end;
                }

                changed = true;
                // イベントの削除
                while ( changed ) {
                    changed = false;
                    int numEvents = vsqTrack.getEventCount();
                    for ( int i = 0; i < numEvents; i++ ) {
                        VsqEvent itemi = vsqTrack.getEvent( i );
                        if ( clock_start <= itemi.Clock && itemi.Clock < clock_end ) {
                            vsqTrack.removeEvent( i );
                            changed = true;
                            break;
                        }
                    }
                }

                // クロックのシフト
                if ( last_singer != null ) {
                    vsqTrack.addEvent( last_singer ); //歌手変更イベントを補う
                }
                int num_events = vsqTrack.getEventCount();
                for ( int i = 0; i < num_events; i++ ) {
                    VsqEvent itemi = vsqTrack.getEvent( i );
                    if ( clock_end <= itemi.Clock ) {
                        itemi.Clock -= dclock;
                    }
                }

                for ( int i = 0; i < _CURVES.Length; i++ ) {
                    String curve = _CURVES[i];
                    VsqBPList bplist = vsqTrack.getCurve( curve );
                    if ( bplist == null ){
                        continue;
                    }
                    VsqBPList buf_bplist = (VsqBPList)bplist.clone();
                    bplist.clear();
                    int value_at_end = buf_bplist.getValue( clock_end );
                    boolean at_end_added = false;
                    for ( Iterator<Integer> itr = buf_bplist.keyClockIterator(); itr.hasNext(); ) {
                        int key = itr.next();
                        if ( key < clock_start ) {
                            bplist.add( key, buf_bplist.getValue( key ) );
                        } else if ( clock_end <= key ) {
                            if ( key == clock_end ) {
                                at_end_added = true;
                            }
                            bplist.add( key - dclock, buf_bplist.getValue( key ) );
                        }
                    }
                    if ( !at_end_added ) {
                        bplist.add( clock_end - dclock, value_at_end );
                    }
                }
            }
#if DEBUG
            PortUtil.println( "VsqFile#removePart; after:" );
            for ( int i = 0; i < TempoTable.size(); i++ ) {
                PortUtil.println( "    c" + TempoTable.get( i ).Clock + ", s" + TempoTable.get( i ).Time + ", t" + TempoTable.get( i ).Tempo );
            }
#endif
        }

        /// <summary>
        /// vsqファイル全体のイベントを，指定したクロックだけ遅らせます．
        /// ただし，曲頭のテンポ変更イベントと歌手変更イベントはクロック0から移動しません．
        /// この操作を行うことで，TimesigTableの情報は破綻します（仕様です）．
        /// </summary>
        /// <param name="delta_clock"></param>
        public static void shift( VsqFile vsq, int delta_clock ) {
            if ( delta_clock == 0 ) {
                return;
            }
            int dclock = (int)delta_clock;
            for ( int i = 0; i < vsq.TempoTable.size(); i++ ) {
                if ( vsq.TempoTable.get( i ).Clock > 0 ) {
                    vsq.TempoTable.get( i ).Clock = vsq.TempoTable.get( i ).Clock + dclock;
                }
            }
            vsq.updateTempoInfo();
            int numTrack = vsq.Track.size();
            for ( int track = 1; track < numTrack; track++ ) {
                VsqTrack vsqTrack = vsq.Track.get( track );
                int numEvents = vsqTrack.getEventCount();
                for ( int i = 0; i < numEvents; i++ ) {
                    VsqEvent itemi = vsqTrack.getEvent( i );
                    if ( itemi.Clock > 0 ) {
                        itemi.Clock += dclock;
                    }
                }
                for ( int i = 0; i < _CURVES.Length; i++ ) {
                    String curve = _CURVES[i];
                    VsqBPList edit = vsqTrack.getCurve( curve );
                    if ( edit == null ) {
                        continue;
                    }
                    // 順番に+=dclockしていくとVsqBPList内部のSortedListの値がかぶる可能性がある．
                    VsqBPList new_one = new VsqBPList( edit.getName(), edit.getDefault(), edit.getMinimum(), edit.getMaximum() );
                    for ( Iterator<Integer> itr2 = edit.keyClockIterator(); itr2.hasNext(); ) {
                        int key = itr2.next();
                        new_one.add( key + dclock, edit.getValue( key ) );
                    }
                    vsqTrack.setCurve( curve, new_one );
                }
            }
            vsq.updateTotalClocks();
        }

        /// <summary>
        /// このインスタンスのコピーを作成します
        /// </summary>
        /// <returns>このインスタンスのコピー</returns>
        public Object clone() {
            VsqFile ret = new VsqFile();
            ret.Track = new List<VsqTrack>();
            for ( int i = 0; i < Track.size(); i++ ) {
                ret.Track.add( (VsqTrack)Track.get( i ).clone() );
            }

            ret.TempoTable = new TempoVector();
            for ( int i = 0; i < TempoTable.size(); i++ ) {
                ret.TempoTable.add( (TempoTableEntry)TempoTable.get( i ).clone() );
            }

            ret.TimesigTable = new List<TimeSigTableEntry>();
            for ( int i = 0; i < TimesigTable.size(); i++ ) {
                ret.TimesigTable.add( (TimeSigTableEntry)TimesigTable.get( i ).clone() );
            }
            //ret.m_tpq = m_tpq;
            ret.TotalClocks = TotalClocks;
            //ret.m_base_tempo = m_base_tempo;
            ret.Master = (VsqMaster)Master.clone();
            ret.Mixer = (VsqMixer)Mixer.clone();
            //ret.m_premeasure_clocks = m_premeasure_clocks;
            return ret;
        }

#if !JAVA
        public object Clone() {
            return clone();
        }
#endif

        private VsqFile() {
        }

#if JAVA
        private class BarLineIterator implements Iterator{
#else
        private class BarLineIterator : Iterator<VsqBarLineType> {
#endif
            private List<TimeSigTableEntry> m_list;
            private int m_end_clock;
            private int i;
            private int clock;
            int local_denominator;
            int local_numerator;
            int clock_step;
            int t_end;
            int local_clock;
            int bar_counter;

            public BarLineIterator( List<TimeSigTableEntry> list, int end_clock ) {
                m_list = list;
                m_end_clock = end_clock;
                i = 0;
                t_end = -1;
                clock = 0;
            }

            public void reset( int end_clock ) {
                this.m_end_clock = end_clock;
                this.i = 0;
                this.t_end = -1;
                this.clock = 0;
                this.local_denominator = 0;
                this.local_numerator = 0;
                this.clock_step = 0;
                this.local_clock = 0;
                this.bar_counter = 0;
            }

            public VsqBarLineType next() {
                int mod = clock_step * local_numerator;
                if ( clock < t_end ) {
                    if ( (clock - local_clock) % mod == 0 ) {
                        bar_counter++;
                        VsqBarLineType ret = new VsqBarLineType( clock, true, local_denominator, local_numerator, bar_counter );
                        clock += clock_step;
                        return ret;
                    } else {
                        VsqBarLineType ret = new VsqBarLineType( clock, false, local_denominator, local_numerator, bar_counter );
                        clock += clock_step;
                        return ret;
                    }
                }

                if ( i < m_list.size() ) {
                    local_denominator = m_list.get( i ).Denominator;
                    local_numerator = m_list.get( i ).Numerator;
                    local_clock = m_list.get( i ).Clock;
                    int local_bar_count = m_list.get( i ).BarCount;
                    clock_step = 480 * 4 / local_denominator;
                    mod = clock_step * local_numerator;
                    bar_counter = local_bar_count - 1;
                    t_end = m_end_clock;
                    if ( i + 1 < m_list.size() ) {
                        t_end = m_list.get( i + 1 ).Clock;
                    }
                    i++;
                    clock = local_clock;
                    if ( clock < t_end ) {
                        if ( (clock - local_clock) % mod == 0 ) {
                            bar_counter++;
                            VsqBarLineType ret = new VsqBarLineType( clock, true, local_denominator, local_numerator, bar_counter );
                            clock += clock_step;
                            return ret;
                        } else {
                            VsqBarLineType ret = new VsqBarLineType( clock, false, local_denominator, local_numerator, bar_counter );
                            clock += clock_step;
                            return ret;
                        }
                    }
                }
                return new VsqBarLineType();
            }

            public void remove() {
                //throw new Exception( "com.boare.vsq.VsqFile.BarLineIterator#remove; not implemented" );
            }

            public boolean hasNext() {
                if ( clock < m_end_clock ) {
                    return true;
                } else {
                    return false;
                }
            }
        }

        /// <summary>
        /// 小節の区切りを順次返すIterator。
        /// </summary>
        /// <returns></returns>
        public Iterator<VsqBarLineType> getBarLineIterator( int end_clock ) {
            if ( this.barLineIterator == null ) {
                this.barLineIterator = new BarLineIterator( this.TimesigTable, end_clock );
            } else {
                this.barLineIterator.reset( end_clock );
            }
            return this.barLineIterator;
        }

        /// <summary>
        /// 基本テンポ値を取得します
        /// </summary>
        public int getBaseTempo() {
            return baseTempo;
        }

        /// <summary>
        /// プリメジャー値を取得します
        /// </summary>
        public int getPreMeasure() {
            return Master.PreMeasure;
        }

        /// <summary>
        /// プリメジャー部分の長さをクロックに変換した値を取得します．
        /// </summary>
        public int getPreMeasureClocks() {
            return calculatePreMeasureInClock();
        }

        /// <summary>
        /// プリメジャーの長さ(クロック)を計算します。
        /// </summary>
        private int calculatePreMeasureInClock() {
            int pre_measure = Master.PreMeasure;
            TimeSigTableEntry item0 = this.TimesigTable.get( 0 );
            int last_bar_count = item0.BarCount;
            int last_clock = item0.Clock;
            int last_denominator = item0.Denominator;
            int last_numerator = item0.Numerator;
            int c = this.TimesigTable.size();
            for ( int i = 1; i < c; i++ ) {
                TimeSigTableEntry itemi = this.TimesigTable.get( i );
                if ( itemi.BarCount >= pre_measure ) {
                    break;
                } else {
                    last_bar_count = itemi.BarCount;
                    last_clock = itemi.Clock;
                    last_denominator = itemi.Denominator;
                    last_numerator = itemi.Numerator;
                }
            }

            int remained = pre_measure - last_bar_count;//プリメジャーの終わりまでの残り小節数
            return last_clock + remained * last_numerator * 480 * 4 / last_denominator;
        }

        /// <summary>
        /// 指定したクロックにおける、clock=0からの演奏経過時間(sec)を取得します
        /// </summary>
        /// <param name="clock"></param>
        /// <returns></returns>
        public double getSecFromClock( double clock ) {
            return TempoTable.getSecFromClock( clock );
        }

        /// <summary>
        /// 指定した時刻における、クロックを取得します
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public double getClockFromSec( double time ) {
            return TempoTable.getClockFromSec( time );
        }

        /// <summary>
        /// 指定したクロックにおける拍子を取得します
        /// </summary>
        /// <param name="clock"></param>
        public Timesig getTimesigAt( int clock ) {
            Timesig ret = new Timesig();
            ret.numerator = 4;
            ret.denominator = 4;
            int index = 0;
            int c = TimesigTable.size();
            for ( int i = c - 1; i >= 0; i-- ) {
                index = i;
                if ( TimesigTable.get( i ).Clock <= clock ) {
                    break;
                }
            }
            ret.numerator = TimesigTable.get( index ).Numerator;
            ret.denominator = TimesigTable.get( index ).Denominator;
            return ret;
        }

        public Timesig getTimesigAt( int clock, ByRef<Integer> bar_count ) {
            int index = 0;
            int c = TimesigTable.size();
            for ( int i = c - 1; i >= 0; i-- ) {
                index = i;
                if ( TimesigTable.get( i ).Clock <= clock ) {
                    break;
                }
            }
            TimeSigTableEntry item = TimesigTable.get( index );
            Timesig ret = new Timesig();
            ret.numerator = item.Numerator;
            ret.denominator = item.Denominator;
            int diff = clock - item.Clock;
            int clock_per_bar = 480 * 4 / ret.denominator * ret.numerator;
            bar_count.value = item.BarCount + diff / clock_per_bar;
            return ret;
        }

        /// <summary>
        /// 指定したクロックにおけるテンポを取得します。
        /// </summary>
        /// <param name="clock"></param>
        /// <returns></returns>
        public int getTempoAt( int clock ) {
            int index = 0;
            int c = TempoTable.size();
            for ( int i = c - 1; i >= 0; i-- ) {
                index = i;
                if ( TempoTable.get( i ).Clock <= clock ) {
                    break;
                }
            }
            return TempoTable.get( index ).Tempo;
        }

        /// <summary>
        /// 指定した小節の開始クロックを調べます。ここで使用する小節数は、プリメジャーを考慮しない。即ち、曲頭の小節が0である。
        /// </summary>
        /// <param name="bar_count"></param>
        /// <returns></returns>
        public int getClockFromBarCount( int bar_count ) {
            int index = 0;
            int c = TimesigTable.size();
            for ( int i = c - 1; i >= 0; i-- ) {
                index = i;
                if ( TimesigTable.get( i ).BarCount <= bar_count ) {
                    break;
                }
            }
            TimeSigTableEntry item = TimesigTable.get( index );
            int numerator = item.Numerator;
            int denominator = item.Denominator;
            int init_clock = item.Clock;
            int init_bar_count = item.BarCount;
            int clock_per_bar = numerator * 480 * 4 / denominator;
            return init_clock + (bar_count - init_bar_count) * clock_per_bar;
        }

        /// <summary>
        /// 指定したクロックが、曲頭から何小節目に属しているかを調べます。ここで使用する小節数は、プリメジャーを考慮しない。即ち、曲頭の小節が0である。
        /// </summary>
        /// <param name="clock"></param>
        /// <returns></returns>
        public int getBarCountFromClock( int clock ) {
            int index = 0;
            int c = TimesigTable.size();
            for ( int i = c - 1; i >= 0; i-- ) {
                index = i;
                if ( TimesigTable.get( i ).Clock <= clock ) {
                    break;
                }
            }
            int bar_count = 0;
            if ( index >= 0 ) {
                int last_clock = TimesigTable.get( index ).Clock;
                int t_bar_count = TimesigTable.get( index ).BarCount;
                int numerator = TimesigTable.get( index ).Numerator;
                int denominator = TimesigTable.get( index ).Denominator;
                int clock_per_bar = numerator * 480 * 4 / denominator;
                bar_count = t_bar_count + (clock - last_clock) / clock_per_bar;
            }
            return bar_count;
        }

        /// <summary>
        /// 4分の1拍子1音あたりのクロック数を取得します
        /// </summary>
        public int getTickPerQuarter() {
            return m_tpq;
        }

        /// <summary>
        /// 空のvsqファイルを構築します
        /// </summary>
        /// <param name="pre_measure"></param>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        /// <param name="tempo"></param>
        public VsqFile( String singer, int pre_measure, int numerator, int denominator, int tempo ) {
            TotalClocks = pre_measure * 480 * 4 / denominator * numerator;
            //m_tpq = 480;

            Track = new List<VsqTrack>();
            Track.add( new VsqTrack( tempo, numerator, denominator ) );
            Track.add( new VsqTrack( "Voice1", singer ) );
            Master = new VsqMaster( pre_measure );
#if DEBUG
            PortUtil.println( "VsqFile.ctor()" );
#endif
            Mixer = new VsqMixer( 0, 0, 0, 0 );
            Mixer.Slave.add( new VsqMixerEntry( 0, 0, 0, 0 ) );
            TimesigTable = new List<TimeSigTableEntry>();
            TimesigTable.add( new TimeSigTableEntry( 0, numerator, denominator, 0 ) );
            TempoTable = new TempoVector();
            TempoTable.add( new TempoTableEntry( 0, tempo, 0.0 ) );
            //m_base_tempo = tempo;
            //m_premeasure_clocks = calculatePreMeasureInClock();
        }

        /// <summary>
        /// vsqファイルからのコンストラクタ
        /// </summary>
        /// <param name="_fpath"></param>
        public VsqFile( String _fpath, String encoding )
#if JAVA
            throws FileNotFoundException
#endif
        {
            TempoTable = new TempoVector();
            TimesigTable = new List<TimeSigTableEntry>();

            // SMFをコンバートしたテキストファイルを作成
            MidiFile mf = new MidiFile( _fpath );
            Track = new List<VsqTrack>();
            int num_track = mf.getTrackCount();
#if DEBUG
            PortUtil.println( "VsqFile#.ctor; num_track=" + num_track );
#endif
            for ( int i = 0; i < num_track; i++ ) {
                Track.add( new VsqTrack( mf.getMidiEventList( i ), encoding ) );
            }

            VsqMaster master = Track.get( 1 ).getMaster();
            if ( master == null ) {
                Master = new VsqMaster( 4 );
            } else {
                Master = (VsqMaster)master.clone();
            }
            VsqMixer mixer = Track.get( 1 ).getMixer();
            if ( mixer == null ) {
                Mixer = new VsqMixer( 0, 0, 0, 0 );
                Mixer.Slave = new List<VsqMixerEntry>();
                for ( int i = 1; i < Track.size(); i++ ) {
                    Mixer.Slave.add( new VsqMixerEntry() );
                }
            } else {
                Mixer = (VsqMixer)mixer.clone();
            }
            Track.get( 1 ).setMaster( null );
            Track.get( 1 ).setMixer( null );

#if DEBUG
            PortUtil.stdout.println( "VsqFile#ctor(String,String)" );
#endif
            int master_track = -1;
            for ( int i = 0; i < Track.size(); i++ ) {
#if DEBUG
                PortUtil.stdout.println( "    m_tracks[i].Name=" + Track.get( i ).getName() );
#endif
                if ( Track.get( i ).getName().Equals( "Master Track" ) ) {
                    master_track = i;
                    break;
                }
            }

            int prev_tempo;
            int prev_index;
            double prev_time;
            if ( master_track >= 0 ) {
                #region TempoListの作成
                // MIDI event リストの取得
                List<MidiEvent> midi_event = mf.getMidiEventList( master_track );
                // とりあえずtempo_tableに格納
                prev_tempo = 500000;
                prev_index = 0;
                double thistime;
                prev_time = 0.0;
                int count = -1;
                int midi_event_size = midi_event.size();
                for ( int j = 0; j < midi_event_size; j++ ) {
                    MidiEvent itemj = midi_event.get( j );
                    if ( itemj.firstByte == 0xff && itemj.data.Length >= 4 && itemj.data[0] == 0x51 ) {
                        count++;
                        if ( count == 0 && itemj.clock != 0 ) {
                            TempoTable.add( new TempoTableEntry( 0, 500000, 0.0 ) );
                            prev_tempo = 500000;
                        }
                        int current_tempo = itemj.data[1] << 16 | itemj.data[2] << 8 | itemj.data[3];
                        int current_index = (int)midi_event.get( j ).clock;
                        thistime = prev_time + (double)(prev_tempo) * (double)(current_index - prev_index) / (m_tpq * 1000000.0);
                        TempoTable.add( new TempoTableEntry( current_index, current_tempo, thistime ) );
                        prev_tempo = current_tempo;
                        prev_index = current_index;
                        prev_time = thistime;
                    }
                }
                Collections.sort( TempoTable );
                #endregion

                #region TimeSigTableの作成
                int dnomi = 4;
                int numer = 4;
                count = -1;
                for ( int j = 0; j < midi_event.size(); j++ ) {
                    if ( midi_event.get( j ).firstByte == 0xff && midi_event.get( j ).data.Length >= 5 && midi_event.get( j ).data[0] == 0x58 ) {
                        count++;
                        numer = midi_event.get( j ).data[1];
                        dnomi = 1;
                        for ( int i = 0; i < midi_event.get( j ).data[2]; i++ ) {
                            dnomi = dnomi * 2;
                        }
                        if ( count == 0 ) {
                            int numerator = 4;
                            int denominator = 4;
                            int clock = 0;
                            int bar_count = 0;
                            if ( midi_event.get( j ).clock == 0 ) {
                                TimesigTable.add( new TimeSigTableEntry( 0, numer, dnomi, 0 ) );
                            } else {
                                TimesigTable.add( new TimeSigTableEntry( 0, 4, 4, 0 ) );
                                TimesigTable.add( new TimeSigTableEntry( 0, numer, dnomi, (int)midi_event.get( j ).clock / (480 * 4) ) );
                                count++;
                            }
                        } else {
                            int numerator = TimesigTable.get( count - 1 ).Numerator;
                            int denominator = TimesigTable.get( count - 1 ).Denominator;
                            int clock = TimesigTable.get( count - 1 ).Clock;
                            int bar_count = TimesigTable.get( count - 1 ).BarCount;
                            int dif = 480 * 4 / denominator * numerator;//1小節が何クロックか？
                            bar_count += ((int)midi_event.get( j ).clock - clock) / dif;
                            TimesigTable.add( new TimeSigTableEntry( (int)midi_event.get( j ).clock, numer, dnomi, bar_count ) );
                        }
                    }
                }
                #endregion
            }

            // 曲の長さを計算
            TempoTable.updateTempoInfo();
            updateTimesigInfo();
            updateTotalClocks();
#if DEBUG
            PortUtil.stdout.println( "    m_total_clocks=" + TotalClocks );
#endif
        }

        /// <summary>
        /// TimeSigTableの[*].Clockの部分を更新します
        /// </summary>
        public void updateTimesigInfo() {
#if DEBUG
            PortUtil.println( "VsqFile#updateTimesigInfo; before:" );
            for ( int i = 0; i < TimesigTable.size(); i++ ) {
                PortUtil.println( "    " + TimesigTable.get( i ).Clock + " " + TimesigTable.get( i ).Numerator + "/" + TimesigTable.get( i ).Denominator );
            }
#endif
            if ( TimesigTable.get( 0 ).Clock != 0 ) {
                return;
            }
            TimesigTable.get( 0 ).Clock = 0;
            Collections.sort( TimesigTable );
            int count = TimesigTable.size();
            for ( int j = 1; j < count; j++ ) {
                TimeSigTableEntry item = TimesigTable.get( j - 1 );
                int numerator = item.Numerator;
                int denominator = item.Denominator;
                int clock = item.Clock;
                int bar_count = item.BarCount;
                int dif = 480 * 4 / denominator * numerator;//1小節が何クロックか？
                clock += (TimesigTable.get( j ).BarCount - bar_count) * dif;
                TimesigTable.get( j ).Clock = clock;
            }
#if DEBUG
            PortUtil.println( "VsqFile#updateTimesigInfo; after:" );
            for ( int i = 0; i < TimesigTable.size(); i++ ) {
                PortUtil.println( "    " + TimesigTable.get( i ).Clock + " " + TimesigTable.get( i ).Numerator + "/" + TimesigTable.get( i ).Denominator );
            }
#endif
        }

        /// <summary>
        /// TempoTableの[*].Timeの部分を更新します
        /// </summary>
        public void updateTempoInfo() {
            TempoTable.updateTempoInfo();
        }

        /// <summary>
        /// VsqFile.Executeの実行直後などに、m_total_clocksの値を更新する
        /// </summary>
        public void updateTotalClocks() {
            int max = getPreMeasureClocks();
            for ( int i = 1; i < Track.size(); i++ ) {
                VsqTrack track = Track.get( i );
                int numEvents = track.getEventCount();
                if ( numEvents > 0 ) {
                    VsqEvent lastItem = track.getEvent( numEvents - 1 );
                    max = Math.Max( max, lastItem.Clock + lastItem.ID.getLength() );
                }
                for ( int j = 0; j < _CURVES.Length; j++ ) {
                    String vct = _CURVES[j];
                    VsqBPList list = track.getCurve( vct );
                    if ( list == null ) {
                        continue;
                    }
                    int keys = list.size();
                    if ( keys > 0 ) {
                        int last_key = list.getKeyClock( keys - 1 );
                        max = Math.Max( max, last_key );
                    }
                }
            }
            TotalClocks = max;
        }

        /// <summary>
        /// 曲の長さを取得する。(sec)
        /// </summary>
        public double getTotalSec() {
            return getSecFromClock( (int)TotalClocks );
        }

        /// <summary>
        /// 指定された番号のトラックに含まれる歌詞を指定されたファイルに出力します
        /// </summary>
        /// <param name="track"></param>
        /// <param name="fpath"></param>
        public void printLyricTable( int track, String fpath ) {
            BufferedWriter sw = null;
            try {
                sw = new BufferedWriter( new FileWriter( fpath ) );
                for ( int i = 0; i < Track.get( track ).getEventCount(); i++ ) {
                    int Length;
                    // timesignal
                    int time_signal = Track.get( track ).getEvent( i ).Clock;
                    // イベントで指定されたIDがLyricであった場合
                    if ( Track.get( track ).getEvent( i ).ID.type == VsqIDType.Anote ) {
                        // 発音長を取得
                        Length = Track.get( track ).getEvent( i ).ID.getLength();

                        // tempo_tableから、発音開始時のtempoを取得
                        int last = TempoTable.size() - 1;
                        int tempo = TempoTable.get( last ).Tempo;
                        int prev_index = TempoTable.get( last ).Clock;
                        double prev_time = TempoTable.get( last ).Time;
                        for ( int j = 1; j < TempoTable.size(); j++ ) {
                            if ( TempoTable.get( j ).Clock > time_signal ) {
                                tempo = TempoTable.get( j - 1 ).Tempo;
                                prev_index = TempoTable.get( j - 1 ).Clock;
                                prev_time = TempoTable.get( j - 1 ).Time;
                                break;
                            }
                        }
                        int current_index = Track.get( track ).getEvent( i ).Clock;
                        double start_time = prev_time + (double)(current_index - prev_index) * (double)tempo / (m_tpq * 1000000.0);
                        // TODO: 単純に + Lengthしただけではまずいはず。要検討
                        double end_time = start_time + ((double)Length) * ((double)tempo) / (m_tpq * 1000000.0);
                        sw.write( Track.get( track ).getEvent( i ).Clock + "," +
                                  PortUtil.formatDecimal( "0.000000", start_time ) + "," +
                                  PortUtil.formatDecimal( "0.000000", end_time ) + "," +
                                  Track.get( track ).getEvent( i ).ID.LyricHandle.L0.Phrase + "," +
                                  Track.get( track ).getEvent( i ).ID.LyricHandle.L0.getPhoneticSymbol() );
                        sw.newLine();
                    }

                }
            } catch ( Exception ex ) {
            } finally {
                if ( sw != null ) {
                    try {
                        sw.close();
                    } catch ( Exception ex2 ) {
                    }
                }
            }
        }

        public List<MidiEvent> generateMetaTextEvent( int track, String encoding ) {
            return generateMetaTextEvent( track, encoding, calculatePreMeasureInClock() );
        }

        public List<MidiEvent> generateMetaTextEvent( int track, String encoding, int start_clock ) {
            String _NL = "" + (char)(byte)0x0a;
            List<MidiEvent> ret = new List<MidiEvent>();
            TextStream sr = null;
            try {
                sr = new TextStream();
                Track.get( track ).printMetaText( sr, TotalClocks + 120, start_clock );
                sr.setPointer( -1 );
                int line_count = -1;
                String tmp = "";
                if ( sr.ready() ) {
#if NEW_IMPL
                    List<Byte> buffer = new List<Byte>();
                    while ( sr.ready() ) {
                        tmp = sr.readLine() + _NL;
                        Byte[] linebytes = PortUtil.convertByteArray( PortUtil.getEncodedByte( encoding, tmp ) );
                        buffer.addAll( Arrays.asList( linebytes ) );
                        while ( getLinePrefixBytes( line_count + 1 ).Length + buffer.size() >= 127 ) {
                            line_count++;
                            byte[] prefix = getLinePrefixBytes( line_count );
                            MidiEvent add = new MidiEvent();
                            add.clock = 0;
                            add.firstByte = 0xff;
                            add.data = new int[128];
                            add.data[0] = 0x01;
                            int remain = 127;
                            for ( int i = 0; i < prefix.Length; i++ ) {
                                add.data[i + 1] = prefix[i];
                            }
                            for ( int i = prefix.Length; i < remain; i++ ) {
                                byte d = buffer.get( 0 );
                                add.data[i + 1] = d;
                                buffer.removeElementAt( 0 );
                            }
                            ret.add( add );
                        }
                    }
                    if ( buffer.size() > 0 ) {
                        while ( getLinePrefixBytes( line_count + 1 ).Length + buffer.size() >= 127 ) {
                            line_count++;
                            byte[] prefix = getLinePrefixBytes( line_count );
                            MidiEvent add = new MidiEvent();
                            add.clock = 0;
                            add.firstByte = 0xff;
                            add.data = new int[128];
                            add.data[0] = 0x01;
                            int remain = 127;
                            for ( int i = 0; i < prefix.Length; i++ ) {
                                add.data[i + 1] = prefix[i];
                            }
                            for ( int i = prefix.Length; i < remain; i++ ) {
                                add.data[i + 1] = buffer.get( 0 );
                                buffer.removeElementAt( 0 );
                            }
                            ret.add( add );
                        }
                        if ( buffer.size() > 0 ) {
                            line_count++;
                            byte[] prefix = getLinePrefixBytes( line_count );
                            MidiEvent add = new MidiEvent();
                            add.clock = 0;
                            add.firstByte = 0xff;
                            int remain = prefix.Length + buffer.size();
                            add.data = new int[remain + 1];
                            add.data[0] = 0x01;
                            for ( int i = 0; i < prefix.Length; i++ ) {
                                add.data[i + 1] = prefix[i];
                            }
                            for ( int i = prefix.Length; i < remain; i++ ) {
                                add.data[i + 1] = buffer.get( 0 );
                                buffer.removeElementAt( 0 );
                            }
                            ret.add( add );
                        }
                    }
#else
                    tmp = sr.readLine();
                    byte[] line_bytes;
                    while ( sr.peek() >= 0 ) {
                        tmp += _NL + sr.readLine();
                        while ( PortUtil.getEncodedByteCount( encoding, tmp + getLinePrefix( line_count + 1 ) ) >= 127 ) {
                            line_count++;
                            tmp = getLinePrefix( line_count ) + tmp;
                            String work = substring127Bytes( tmp, encoding );// tmp.Substring( 0, 127 );
                            tmp = tmp.Substring( PortUtil.getStringLength( work ) );
                            line_bytes = PortUtil.getEecodedByte( encoding, work );
                            MidiEvent add = new MidiEvent();
                            add.clock = 0;
                            add.firstByte = (byte)0xff; //ステータス メタ＊
                            add.data = new byte[line_bytes.Length + 1];
                            add.data[0] = (byte)0x01; //メタテキスト
                            for ( int i = 0; i < line_bytes.Length; i++ ) {
                                add.data[i + 1] = line_bytes[i];
                            }
                            ret.add( add );
                        }
                    }
                    // 残りを出力
                    line_count++;
                    tmp = getLinePrefix( line_count ) + tmp + _NL;
                    while ( PortUtil.getEncodedByteCount( encoding, tmp ) > 127 ) {
                        String work = substring127Bytes( tmp, encoding );
                        tmp = tmp.Substring( PortUtil.getStringLength( work ) );
                        line_bytes = PortUtil.getEecodedByte( encoding, work );
                        MidiEvent add = new MidiEvent();
                        add.clock = 0;
                        add.firstByte = (byte)0xff;
                        add.data = new byte[line_bytes.Length + 1];
                        add.data[0] = (byte)0x01;
                        for ( int i = 0; i < line_bytes.Length; i++ ) {
                            add.data[i + 1] = line_bytes[i];
                        }
                        ret.add( add );
                        line_count++;
                        tmp = getLinePrefix( line_count );
                    }
                    line_bytes = PortUtil.getEecodedByte( encoding, tmp );
                    MidiEvent add0 = new MidiEvent();
                    add0.firstByte = (byte)0xff;
                    add0.data = new byte[line_bytes.Length + 1];
                    add0.data[0] = (byte)0x01;
                    for ( int i = 0; i < line_bytes.Length; i++ ) {
                        add0.data[i + 1] = line_bytes[i];
                    }
                    ret.add( add0 );
#endif
                }
            } catch ( Exception ex ) {
                PortUtil.stderr.println( "VsqFile#generateMetaTextEvent; ex=" + ex );
            } finally {
                if ( sr != null ) {
                    try {
                        sr.close();
                    } catch ( Exception ex2 ) {
                        PortUtil.stderr.println( "VsqFile#generateMetaTextEvent; ex2=" + ex2 );
                    }
                }
            }
#if DEBUG
            PortUtil.stdout.println( "VsqFile#generateMetaTextEvent; ret.size()=" + ret.size() );
#endif
            return ret;
        }

        /// <summary>
        /// 文字列sの先頭から文字列を切り取るとき，切り取った文字列をencodingによりエンコードした結果が127Byte以下になるように切り取ります．
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        private static String substring127Bytes( String s, String encoding ) {
            int count = Math.Min( 127, PortUtil.getStringLength( s ) );
            int c = PortUtil.getEncodedByteCount( encoding, s.Substring( 0, count ) );
            if ( c == 127 ) {
                return s.Substring( 0, count );
            }
            int delta = c > 127 ? -1 : 1;
            while ( (delta == -1 && c > 127) || (delta == 1 && c < 127) ) {
                count += delta;
                if ( delta == -1 && count == 0 ) {
                    break;
                } else if ( delta == 1 && count == PortUtil.getStringLength( s ) ) {
                    break;
                }
                c = PortUtil.getEncodedByteCount( encoding, s.Substring( 0, count ) );
            }
            return s.Substring( 0, count );
        }

        private static void printTrack( VsqFile vsq, int track, RandomAccessFile fs, int msPreSend, String encoding )
#if JAVA
            throws IOException
#endif
        {
            //VsqTrack item = Tracks[track];
            String _NL = "" + (char)(byte)0x0a;
            //ヘッダ
            fs.write( _MTRK, 0, 4 );
            //データ長。とりあえず0
            fs.writeByte( 0 );
            fs.writeByte( 0 );
            fs.writeByte( 0 );
            fs.writeByte( 0 );
            long first_position = fs.getFilePointer();
            //トラック名
            writeFlexibleLengthUnsignedLong( fs, (byte)0x00 );//デルタタイム
            fs.writeByte( 0xff );//ステータスタイプ
            fs.writeByte( 0x03 );//イベントタイプSequence/Track Name
            byte[] seq_name = PortUtil.getEncodedByte( encoding, vsq.Track.get( track ).getName() );
            writeFlexibleLengthUnsignedLong( fs, (long)seq_name.Length );//seq_nameの文字数
            fs.write( seq_name, 0, seq_name.Length );

            //Meta Textを準備
            List<MidiEvent> meta = vsq.generateMetaTextEvent( track, encoding );
            long lastclock = 0;
            for ( int i = 0; i < meta.size(); i++ ) {
                writeFlexibleLengthUnsignedLong( fs, (long)(meta.get( i ).clock - lastclock) );
                meta.get( i ).writeData( fs );
                lastclock = meta.get( i ).clock;
            }

            int last = 0;
            VsqNrpn[] data = generateNRPN( vsq, track, msPreSend );
            NrpnData[] nrpns = VsqNrpn.convert( data );
            for ( int i = 0; i < nrpns.Length; i++ ) {
                writeFlexibleLengthUnsignedLong( fs, (long)(nrpns[i].getClock() - last) );
                fs.write( (byte)0xb0 );
                fs.write( nrpns[i].getParameter() );
                fs.write( nrpns[i].Value );
                last = nrpns[i].getClock();
            }

            //トラックエンド
            VsqEvent last_event = vsq.Track.get( track ).getEvent( vsq.Track.get( track ).getEventCount() - 1 );
            int last_clock = last_event.Clock + last_event.ID.getLength();
            writeFlexibleLengthUnsignedLong( fs, (long)last_clock );
            fs.write( (byte)0xff );
            fs.write( (byte)0x2f );
            fs.write( (byte)0x00 );
            long pos = fs.getFilePointer();
            fs.seek( first_position - 4 );
            writeUnsignedInt( fs, (long)(pos - first_position) );
            fs.seek( pos );
        }

        /// <summary>
        /// 指定したクロックにおけるプリセンド・クロックを取得します
        /// </summary>
        /// <param name="clock"></param>
        /// <param name="msPreSend"></param>
        /// <returns></returns>
        public int getPresendClockAt( int clock, int msPreSend ) {
            double clock_msec = getSecFromClock( clock ) * 1000.0;
            float draft_clock_sec = (float)(clock_msec - msPreSend) / 1000.0f;
            int draft_clock = (int)Math.Floor( getClockFromSec( draft_clock_sec ) );
            return clock - draft_clock;
        }

        /// <summary>
        /// 指定したクロックにおける、音符長さ(ゲートタイム単位)の最大値を調べます
        /// </summary>
        /// <param name="clock"></param>
        /// <returns></returns>
        public int getMaximumNoteLengthAt( int clock ) {
            double secAtStart = getSecFromClock( clock );
            double secAtEnd = secAtStart + VsqID.MAX_NOTE_MILLISEC_LENGTH / 1000.0;
            int clockAtEnd = (int)getClockFromSec( secAtEnd ) - 1;
            secAtEnd = getSecFromClock( clockAtEnd );
            while ( (int)(secAtEnd * 1000.0) - (int)(secAtStart * 1000.0) <= VsqID.MAX_NOTE_MILLISEC_LENGTH ) {
                clockAtEnd++;
                secAtEnd = getSecFromClock( clockAtEnd );
            }
            clockAtEnd--;
            return clockAtEnd - clock;
        }

        /// <summary>
        /// 指定したトラックから、Expression(DYN)のNRPNリストを作成します
        /// </summary>
        /// <param name="track"></param>
        /// <param name="msPreSend"></param>
        /// <returns></returns>
        public static VsqNrpn[] generateExpressionNRPN( VsqFile vsq, int track, int msPreSend ) {
            List<VsqNrpn> ret = new List<VsqNrpn>();
            VsqBPList dyn = vsq.Track.get( track ).getCurve( "DYN" );
            int count = dyn.size();
            for ( int i = 0; i < count; i++ ) {
                int clock = dyn.getKeyClock( i );
                int c = clock - vsq.getPresendClockAt( clock, msPreSend );
                if ( c >= 0 ) {
                    VsqNrpn add = new VsqNrpn( c,
                                               NRPN.CC_E_EXPRESSION,
                                               (byte)dyn.getElement( i ) );
                    ret.add( add );
                }
            }
            return ret.toArray( new VsqNrpn[] { } );
        }

        public static VsqNrpn[] generateFx2DepthNRPN( VsqFile vsq, int track, int msPreSend ) {
            List<VsqNrpn> ret = new List<VsqNrpn>();
            VsqBPList fx2depth = vsq.Track.get( track ).getCurve( "fx2depth" );
            int count = fx2depth.size();
            for ( int i = 0; i < count; i++ ) {
                int clock = fx2depth.getKeyClock( i );
                int c = clock - vsq.getPresendClockAt( clock, msPreSend );
                if ( c >= 0 ) {
                    VsqNrpn add = new VsqNrpn( c,
                                               NRPN.CC_FX2_EFFECT2_DEPTH,
                                               (byte)fx2depth.getElement( i ) );
                    ret.add( add );
                }
            }
            return ret.toArray( new VsqNrpn[] { } );
        }

        /// <summary>
        /// 先頭に記録されるNRPNを作成します
        /// </summary>
        /// <returns></returns>
        public static VsqNrpn generateHeaderNRPN() {
            VsqNrpn ret = new VsqNrpn( 0, NRPN.CC_BS_VERSION_AND_DEVICE, (byte)0x00, (byte)0x00 );
            ret.append( NRPN.CC_BS_DELAY, (byte)0x00, (byte)0x00 );
            ret.append( NRPN.CC_BS_LANGUAGE_TYPE, (byte)0x00 );
            return ret;
        }

        /// <summary>
        /// 歌手変更イベントから，NRPNを作成します
        /// </summary>
        /// <param name="vsq"></param>
        /// <param name="ve"></param>
        /// <param name="msPreSend"></param>
        /// <returns></returns>
        public static VsqNrpn[] generateSingerNRPN( VsqFile vsq, VsqEvent ve, int msPreSend ) {
            int clock = ve.Clock;
            IconHandle singer_handle = null;
            if ( ve.ID.IconHandle != null && ve.ID.IconHandle is IconHandle ) {
                singer_handle = (IconHandle)ve.ID.IconHandle;
            }
            if ( singer_handle == null ) {
                return new VsqNrpn[] { };
            }

            double clock_msec = vsq.getSecFromClock( clock ) * 1000.0;

            int ttempo = vsq.getTempoAt( clock );
            double tempo = 6e7 / ttempo;
            //double sStart = SecFromClock( ve.Clock );
            double msEnd = vsq.getSecFromClock( ve.Clock + ve.ID.getLength() ) * 1000.0;
            int duration = (int)Math.Ceiling( msEnd - clock_msec );
#if DEBUG
            PortUtil.println( "GenerateNoteNRPN" );
            PortUtil.println( "    duration=" + duration );
#endif
            ValuePair<Byte, Byte> d = getMsbAndLsb( duration );
            byte duration0 = d.getKey();
            byte duration1 = d.getValue();
            ValuePair<Byte, Byte> d2 = getMsbAndLsb( msPreSend );
            byte delay0 = d2.getKey();
            byte delay1 = d2.getValue();
            List<VsqNrpn> ret = new List<VsqNrpn>();

            int i = clock - vsq.getPresendClockAt( clock, msPreSend );
            VsqNrpn add = new VsqNrpn( i, NRPN.CC_BS_VERSION_AND_DEVICE, (byte)0x00, (byte)0x00 );
            add.append( NRPN.CC_BS_DELAY, delay0, delay1, true );
            add.append( NRPN.CC_BS_LANGUAGE_TYPE, (byte)singer_handle.Language, true );
            add.append( NRPN.PC_VOICE_TYPE, (byte)singer_handle.Program );
            return new VsqNrpn[] { add };
        }

        /// <summary>
        /// 音符イベントから，NRPNを作成します
        /// </summary>
        /// <param name="vsq"></param>
        /// <param name="track"></param>
        /// <param name="ve"></param>
        /// <param name="msPreSend"></param>
        /// <param name="note_loc"></param>
        /// <param name="add_delay_sign"></param>
        /// <returns></returns>
        public static VsqNrpn generateNoteNRPN( VsqFile vsq, int track, VsqEvent ve, int msPreSend, byte note_loc, boolean add_delay_sign ) {
            int clock = ve.Clock;
            String renderer = vsq.Track.get( track ).getCommon().Version;

            double clock_msec = vsq.getSecFromClock( clock ) * 1000.0;

            int ttempo = vsq.getTempoAt( clock );
            double tempo = 6e7 / ttempo;
            double msEnd = vsq.getSecFromClock( ve.Clock + ve.ID.getLength() ) * 1000.0;
            int duration = (int)(msEnd - clock_msec);
            ValuePair<Byte, Byte> dur = getMsbAndLsb( duration );
            byte duration0 = dur.getKey();
            byte duration1 = dur.getValue();

            VsqNrpn add;
            if ( add_delay_sign ) {
                ValuePair<Byte, Byte> msp = getMsbAndLsb( msPreSend );
                byte delay0 = msp.getKey();
                byte delay1 = msp.getValue();
                add = new VsqNrpn( clock - vsq.getPresendClockAt( clock, msPreSend ), NRPN.CVM_NM_VERSION_AND_DEVICE, (byte)0x00, (byte)0x00 );
                add.append( NRPN.CVM_NM_DELAY, delay0, delay1, true );
                add.append( NRPN.CVM_NM_NOTE_NUMBER, (byte)ve.ID.Note, true ); // Note number
            } else {
                add = new VsqNrpn( clock - vsq.getPresendClockAt( clock, msPreSend ), NRPN.CVM_NM_NOTE_NUMBER, (byte)ve.ID.Note ); // Note number
            }
            add.append( NRPN.CVM_NM_VELOCITY, (byte)ve.ID.Dynamics, true ); // Velocity
            add.append( NRPN.CVM_NM_NOTE_DURATION, duration0, duration1, true ); // Note duration
            add.append( NRPN.CVM_NM_NOTE_LOCATION, note_loc, true ); // Note Location

            // CVM_NMの直後にビブラートのCCを入れるかどうか。ビブラート長さが100%のときのみtrue
            bool add_vib_cc_immediately = false;
            if ( ve.ID.VibratoHandle != null ) {
                add.append( NRPN.CVM_NM_INDEX_OF_VIBRATO_DB, (byte)0x00, (byte)0x00, true );
                String icon_id = ve.ID.VibratoHandle.IconID;
                String num = icon_id.Substring( PortUtil.getStringLength( icon_id ) - 4 );
                int vibrato_type = (int)PortUtil.fromHexString( num );
                int note_length = ve.ID.getLength();
                int vibrato_delay = ve.ID.VibratoDelay;
                byte bVibratoDuration = (byte)((float)(note_length - vibrato_delay) / (float)note_length * 127);
                byte bVibratoDelay = (byte)((byte)0x7f - bVibratoDuration);
                add.append( NRPN.CVM_NM_VIBRATO_CONFIG, (byte)vibrato_type, bVibratoDuration, true );
                add.append( NRPN.CVM_NM_VIBRATO_DELAY, bVibratoDelay, true );

                add_vib_cc_immediately = (bVibratoDelay == 0);
            }

            List<String> spl = ve.ID.LyricHandle.L0.getPhoneticSymbolList();
            String s = "";
            for ( int j = 0; j < spl.Count; j++ ) {
                s += spl[j];
            }
            char[] symbols = s.ToCharArray();
            if ( renderer.StartsWith( "DSB2" ) ) {
                add.append( 0x5011, (byte)0x01, true );//TODO: (byte)0x5011の意味は解析中
            }
            add.append( NRPN.CVM_NM_PHONETIC_SYMBOL_BYTES, (byte)symbols.Length, true );// (byte)0x12(Number of phonetic symbols in bytes)
            int count = -1;
            List<int> consonantAdjustment = ve.ID.LyricHandle.L0.getConsonantAdjustmentList();
            for ( int j = 0; j < spl.Count; j++ ) {
                char[] chars = spl[j].ToCharArray();
                for ( int k = 0; k < chars.Length; k++ ) {
                    count++;
                    if ( k == 0 ) {
                        add.append( (0x50 << 8) | (0x13 + count), (byte)chars[k], (byte)consonantAdjustment[j], true ); // Phonetic symbol j
                    } else {
                        add.append( (0x50 << 8) | (0x13 + count), (byte)chars[k], true ); // Phonetic symbol j
                    }
                }
            }
            if ( !renderer.StartsWith( "DSB2" ) ) {
                add.append( NRPN.CVM_NM_PHONETIC_SYMBOL_CONTINUATION, (byte)0x7f, true ); // End of phonetic symbols
            }
            if ( renderer.StartsWith( "DSB3" ) ) {
                int v1mean = ve.ID.PMBendDepth * 60 / 100;
                if ( v1mean < 0 ) {
                    v1mean = 0;
                }
                if ( 60 < v1mean ) {
                    v1mean = 60;
                }
                int d1mean = (int)(0.3196 * ve.ID.PMBendLength + 8.0);
                int d2mean = (int)(0.92 * ve.ID.PMBendLength + 28.0);
                add.append( NRPN.CVM_NM_V1MEAN, (byte)v1mean, true );// (byte)0x50(v1mean)
                add.append( NRPN.CVM_NM_D1MEAN, (byte)d1mean, true );// (byte)0x51(d1mean)
                add.append( NRPN.CVM_NM_D1MEAN_FIRST_NOTE, (byte)0x14, true );// (byte)0x52(d1meanFirstNote)
                add.append( NRPN.CVM_NM_D2MEAN, (byte)d2mean, true );// (byte)0x53(d2mean)
                add.append( NRPN.CVM_NM_D4MEAN, (byte)ve.ID.d4mean, true );// (byte)0x54(d4mean)
                add.append( NRPN.CVM_NM_PMEAN_ONSET_FIRST_NOTE, (byte)ve.ID.pMeanOnsetFirstNote, true ); // 055(pMeanOnsetFirstNote)
                add.append( NRPN.CVM_NM_VMEAN_NOTE_TRNSITION, (byte)ve.ID.vMeanNoteTransition, true ); // (byte)0x56(vMeanNoteTransition)
                add.append( NRPN.CVM_NM_PMEAN_ENDING_NOTE, (byte)ve.ID.pMeanEndingNote, true );// (byte)0x57(pMeanEndingNote)
                add.append( NRPN.CVM_NM_ADD_PORTAMENTO, (byte)ve.ID.PMbPortamentoUse, true );// (byte)0x58(AddScoopToUpInternals&AddPortamentoToDownIntervals)
                byte decay = (byte)(ve.ID.DEMdecGainRate / 100.0 * (byte)0x64);
                add.append( NRPN.CVM_NM_CHANGE_AFTER_PEAK, decay, true );// (byte)0x59(changeAfterPeak)
                byte accent = (byte)((byte)0x64 * ve.ID.DEMaccent / 100.0);
                add.append( NRPN.CVM_NM_ACCENT, accent, true );// (byte)0x5a(Accent)
            }
            if ( renderer.StartsWith( "UTU0" ) ) {
                // エンベロープ
                if ( ve.UstEvent != null ) {
                    UstEnvelope env = null;
                    if ( ve.UstEvent.Envelope != null ) {
                        env = ve.UstEvent.Envelope;
                    } else {
                        env = new UstEnvelope();
                    }
                    int[] vals = null;
                    vals = new int[10];
                    vals[0] = env.p1;
                    vals[1] = env.p2;
                    vals[2] = env.p3;
                    vals[3] = env.v1;
                    vals[4] = env.v2;
                    vals[5] = env.v3;
                    vals[6] = env.v4;
                    vals[7] = env.p4;
                    vals[8] = env.p5;
                    vals[9] = env.v5;
                    for ( int i = 0; i < vals.Length; i++ ) {
                        //(value3.msb & (byte)0xf) << 28 | (value2.msb & (byte)0x7f) << 21 | (value2.lsb & (byte)0x7f) << 14 | (value1.msb & (byte)0x7f) << 7 | (value1.lsb & (byte)0x7f)
                        byte msb, lsb;
                        int v = vals[i];
                        lsb = (byte)(v & (byte)0x7f);
                        v = v >> 7;
                        msb = (byte)(v & (byte)0x7f);
                        v = v >> 7;
                        add.append( NRPN.CVM_EXNM_ENV_DATA1, msb, lsb );
                        lsb = (byte)(v & (byte)0x7f);
                        v = v >> 7;
                        msb = (byte)(v & (byte)0x7f);
                        v = v >> 7;
                        add.append( NRPN.CVM_EXNM_ENV_DATA2, msb, lsb );
                        msb = (byte)(v & (byte)0xf);
                        add.append( NRPN.CVM_EXNM_ENV_DATA3, msb );
                        add.append( NRPN.CVM_EXNM_ENV_DATA_CONTINUATION, (byte)0x00 );
                    }
                    add.append( NRPN.CVM_EXNM_ENV_DATA_CONTINUATION, (byte)0x7f );

                    // モジュレーション
                    ValuePair<Byte, Byte> m;
                    if ( -100 <= ve.UstEvent.Moduration && ve.UstEvent.Moduration <= 100 ) {
                        m = getMsbAndLsb( ve.UstEvent.Moduration + 100 );
                        add.append( NRPN.CVM_EXNM_MODURATION, m.getKey(), m.getValue() );
                    }

                    // 先行発声
                    if ( ve.UstEvent.PreUtterance != 0 ) {
                        m = getMsbAndLsb( (int)(ve.UstEvent.PreUtterance + 8192) );
                        add.append( NRPN.CVM_EXNM_PRE_UTTERANCE, m.getKey(), m.getValue() );
                    }

                    // Flags
                    if ( ve.UstEvent.Flags != "" ) {
                        char[] arr = ve.UstEvent.Flags.ToCharArray();
                        add.append( NRPN.CVM_EXNM_FLAGS_BYTES, (byte)arr.Length );
                        for ( int i = 0; i < arr.Length; i++ ) {
                            byte b = (byte)arr[i];
                            add.append( NRPN.CVM_EXNM_FLAGS, b );
                        }
                        add.append( NRPN.CVM_EXNM_FLAGS_CONINUATION, (byte)0x7f );
                    }

                    // オーバーラップ
                    if ( ve.UstEvent.VoiceOverlap != 0 ) {
                        m = getMsbAndLsb( (int)(ve.UstEvent.VoiceOverlap + 8192) );
                        add.append( NRPN.CVM_EXNM_VOICE_OVERLAP, m.getKey(), m.getValue() );
                    }
                }
            }
            add.append( NRPN.CVM_NM_NOTE_MESSAGE_CONTINUATION, (byte)0x7f, true );// (byte)0x7f(Note message continuation)

            // ビブラートのコントロールチェンジを追加
            if ( ve.ID.VibratoHandle != null && add_vib_cc_immediately ) {
                add.append( NRPN.CC_VD_VIBRATO_DEPTH, (byte)ve.ID.VibratoHandle.getStartDepth(), false );
                add.append( NRPN.CC_VR_VIBRATO_RATE, (byte)ve.ID.VibratoHandle.getStartRate(), false );
            }
            
            return add;
        }

        /// <summary>
        /// 指定したシーケンスのデータから、指定したゲートタイム区間のNRPNのリストを作成します
        /// </summary>
        /// <param name="vsq">作成元のシーケンス</param>
        /// <param name="track">トラック番号</param>
        /// <param name="msPreSend">プリセンド値(ミリ秒)</param>
        /// <param name="clock_start">リストの作成区間の開始ゲートタイム</param>
        /// <param name="clock_end">リストの作成区間の終了ゲートタイム</param>
        /// <returns>NRPNのリスト</returns>
        public static VsqNrpn[] generateNRPN( VsqFile vsq, int track, int msPreSend, int clock_start, int clock_end ) {
            VsqFile temp = (VsqFile)vsq.clone();
            temp.removePart( clock_end, vsq.TotalClocks );
            if ( 0 < clock_start ) {
                temp.removePart( 0, clock_start );
            }
            temp.Master.PreMeasure = 1;
            //temp.m_premeasure_clocks = temp.getClockFromBarCount( 1 );
            VsqNrpn[] ret = generateNRPN( temp, track, msPreSend );
            temp = null;
            return ret;
        }

        /// <summary>
        /// 指定したシーケンスのデータから、NRPNのリストを作成します
        /// </summary>
        /// <param name="vsq">作成元のシーケンス</param>
        /// <param name="track">トラック番号</param>
        /// <param name="msPreSend">プリセンド値(ミリ秒)</param>
        /// <returns>NRPNのリスト</returns>
        public static VsqNrpn[] generateNRPN( VsqFile vsq, int track, int msPreSend ) {
#if DEBUG
            PortUtil.println( "GenerateNRPN(VsqTrack,int,int,int,int)" );
#endif
            List<VsqNrpn> list = new List<VsqNrpn>();

            VsqTrack target = vsq.Track.get( track );
            String version = target.getCommon().Version;

            int count = target.getEventCount();
            int note_start = 0;
            int note_end = target.getEventCount() - 1;
            for ( int i = 0; i < count; i++ ) {
                if ( 0 <= target.getEvent( i ).Clock ) {
                    note_start = i;
                    break;
                }
                note_start = i;
            }
            for ( int i = target.getEventCount() - 1; i >= 0; i-- ) {
                if ( target.getEvent( i ).Clock <= vsq.TotalClocks ) {
                    note_end = i;
                    break;
                }
            }

            // 最初の歌手を決める
            int singer_event = -1;
            for ( int i = note_start; i >= 0; i-- ) {
                if ( target.getEvent( i ).ID.type == VsqIDType.Singer ) {
                    singer_event = i;
                    break;
                }
            }
            if ( singer_event >= 0 ) { //見つかった場合
                list.addAll( Arrays.asList( generateSingerNRPN( vsq, target.getEvent( singer_event ), 0 ) ) );
            } else {                   //多分ありえないと思うが、歌手が不明の場合。
                //throw new Exception( "first singer was not specified" );
                list.add( new VsqNrpn( 0, NRPN.CC_BS_LANGUAGE_TYPE, (byte)0x0 ) );
                list.add( new VsqNrpn( 0, NRPN.PC_VOICE_TYPE, (byte)0x0 ) );
            }

            list.addAll( Arrays.asList( generateVoiceChangeParameterNRPN( vsq, track, msPreSend ) ) );
            if ( version.StartsWith( "DSB2" ) ) {
                list.addAll( Arrays.asList( generateFx2DepthNRPN( vsq, track, msPreSend ) ) );
            }

            int ms_presend = msPreSend;
            if ( version.StartsWith( "UTU0" ) ) {
                double sec_maxlen = 0.0;
                for ( Iterator<VsqEvent> itr = target.getNoteEventIterator(); itr.hasNext(); ) {
                    VsqEvent ve = itr.next();
                    double len = vsq.getSecFromClock( ve.Clock + ve.ID.getLength() ) - vsq.getSecFromClock( ve.Clock );
                    sec_maxlen = Math.Max( sec_maxlen, len );
                }
                ms_presend += (int)(sec_maxlen * 1000.0);
            }
            VsqBPList dyn = target.getCurve( "dyn" );
            if ( dyn.size() > 0 ) {
                List<VsqNrpn> listdyn = new List<VsqNrpn>( Arrays.asList( generateExpressionNRPN( vsq, track, ms_presend ) ) );
                if ( listdyn.size() > 0 ) {
                    list.addAll( listdyn );
                }
            }
            VsqBPList pbs = target.getCurve( "pbs" );
            if ( pbs.size() > 0 ) {
                List<VsqNrpn> listpbs = new List<VsqNrpn>( Arrays.asList( generatePitchBendSensitivityNRPN( vsq, track, ms_presend ) ) );
                if ( listpbs.size() > 0 ) {
                    list.addAll( listpbs );
                }
            }
            VsqBPList pit = target.getCurve( "pit" );
            if ( pit.size() > 0 ) {
                List<VsqNrpn> listpit = new List<VsqNrpn>( Arrays.asList( generatePitchBendNRPN( vsq, track, ms_presend ) ) );
                if ( listpit.size() > 0 ) {
                    list.addAll( listpit );
                }
            }

            boolean first = true;
            int last_note_end = 0;
            for ( int i = note_start; i <= note_end; i++ ) {
                VsqEvent item = target.getEvent( i );
                if ( item.ID.type == VsqIDType.Anote ) {
                    byte note_loc = (byte)0x03;
                    if ( item.Clock == last_note_end ) {
                        note_loc -= (byte)0x02;
                    }

                    // 次に現れる音符イベントを探す
                    int nextclock = item.Clock + item.ID.getLength() + 1;
                    int event_count = target.getEventCount();
                    for ( int j = i + 1; j < event_count; j++ ) {
                        VsqEvent itemj = target.getEvent( j );
                        if ( itemj.ID.type == VsqIDType.Anote ) {
                            nextclock = itemj.Clock;
                            break;
                        }
                    }
                    if ( item.Clock + item.ID.getLength() == nextclock ) {
                        note_loc -= (byte)0x01;
                    }

                    list.add( generateNoteNRPN( vsq,
                                                track,
                                                item,
                                                msPreSend,
                                                note_loc,
                                                first ) );
                    first = false;
                    list.addAll( Arrays.asList( generateVibratoNRPN( vsq,
                                                                     item,
                                                                     msPreSend ) ) );
                    last_note_end = item.Clock + item.ID.getLength();
                } else if ( item.ID.type == VsqIDType.Singer ) {
                    if ( i > note_start && i != singer_event ) {
                        list.addAll( Arrays.asList( generateSingerNRPN( vsq, item, msPreSend ) ) );
                    }
                }
            }

            list = VsqNrpn.sort( list );
            List<VsqNrpn> merged = new List<VsqNrpn>();
            for ( int i = 0; i < list.size(); i++ ) {
                merged.addAll( Arrays.asList( list.get( i ).expand() ) );
            }
            return merged.toArray( new VsqNrpn[] { } );
        }

        /// <summary>
        /// 指定したトラックから、PitchBendのNRPNを作成します
        /// </summary>
        /// <param name="vsq">作成元のシーケンス</param>
        /// <param name="track">トラック番号</param>
        /// <param name="msPreSend">プリセンド値(ミリ秒)</param>
        /// <returns>NRPNのリスト</returns>
        public static VsqNrpn[] generatePitchBendNRPN( VsqFile vsq, int track, int msPreSend ) {
            List<VsqNrpn> ret = new List<VsqNrpn>();
            VsqBPList pit = vsq.Track.get( track ).getCurve( "PIT" );
            int count = pit.size();
            for ( int i = 0; i < count; i++ ) {
                int clock = pit.getKeyClock( i );
                int value = pit.getElement( i ) + 0x2000;

                ValuePair<Byte, Byte> val = getMsbAndLsb( value );
                int c = clock - vsq.getPresendClockAt( clock, msPreSend );
                if ( c >= 0 ) {
                    VsqNrpn add = new VsqNrpn( c,
                                               NRPN.PB_PITCH_BEND,
                                               val.getKey(),
                                               val.getValue() );
                    ret.add( add );
                }
            }
            return ret.toArray( new VsqNrpn[] { } );
        }

        /// <summary>
        /// 指定したトラックから、PitchBendSensitivityのNRPNを作成します
        /// </summary>
        /// <param name="vsq">作成元のシーケンス</param>
        /// <param name="track">トラック番号</param>
        /// <param name="msPreSend">プリセンド値(ミリ秒)</param>
        /// <returns>NRPNのリスト</returns>
        public static VsqNrpn[] generatePitchBendSensitivityNRPN( VsqFile vsq, int track, int msPreSend ) {
            List<VsqNrpn> ret = new List<VsqNrpn>();
            VsqBPList pbs = vsq.Track.get( track ).getCurve( "PBS" );
            int count = pbs.size();
            for ( int i = 0; i < count; i++ ) {
                int clock = pbs.getKeyClock( i );
                int c = clock - vsq.getPresendClockAt( clock, msPreSend );
                if ( c >= 0 ) {
                    VsqNrpn add = new VsqNrpn( c,
                                               NRPN.CC_PBS_PITCH_BEND_SENSITIVITY,
                                               (byte)pbs.getElement( i ),
                                               (byte)0x00 );
                    ret.add( add );
                }
            }
            return ret.toArray( new VsqNrpn[] { } );
        }

        /// <summary>
        /// 指定した音符イベントから、ビブラート出力用のNRPNのリストを作成します
        /// </summary>
        /// <param name="vsq">作成元のシーケンス</param>
        /// <param name="ve">作成元の音符イベント</param>
        /// <param name="msPreSend">プリセンド値(ミリ秒)</param>
        /// <returns>NRPNのリスト</returns>
        public static VsqNrpn[] generateVibratoNRPN( VsqFile vsq, VsqEvent ve, int msPreSend ) {
            List<VsqNrpn> ret = new List<VsqNrpn>();
            if ( ve.ID.VibratoHandle != null ) {
                int vclock = ve.Clock + ve.ID.VibratoDelay;
                if ( ve.ID.VibratoDelay > 0 ) {
                    // ビブラート長さが100%未満のときのみ、Version&Device,Delay,DepthとRateを追加する。
                    // 100%のときの処理は、generateNoteNRPNで行われているので。(めんどくさいお・・・)
                    ValuePair<Byte, Byte> del = getMsbAndLsb( msPreSend );
                    VsqNrpn add2 = new VsqNrpn( vclock - vsq.getPresendClockAt( vclock, msPreSend ),
                                                NRPN.CC_VD_VERSION_AND_DEVICE,
                                                (byte)0x00,
                                                (byte)0x00 );
                    add2.append( NRPN.CC_VD_DELAY, del.getKey(), del.getValue(), true );
                    add2.append( NRPN.CC_VD_VIBRATO_DEPTH, (byte)ve.ID.VibratoHandle.getStartDepth(), true );
                    add2.append( NRPN.CC_VR_VIBRATO_RATE, (byte)ve.ID.VibratoHandle.getStartRate() );
                    ret.add( add2 );
                }

                int vlength = ve.ID.getLength() - ve.ID.VibratoDelay;
                VibratoBPList rateBP = ve.ID.VibratoHandle.getRateBP();
                int count = rateBP.getCount();
                if ( count > 0 ) {
                    for ( int i = 0; i < count; i++ ) {
                        VibratoBPPair itemi = rateBP.getElement( i );
                        float percent = itemi.X;
                        int cl = vclock + (int)(percent * vlength);
                        ret.add( new VsqNrpn( cl - vsq.getPresendClockAt( cl, msPreSend ),
                                              NRPN.CC_VR_VIBRATO_RATE,
                                              (byte)itemi.Y ) );
                    }
                }
                VibratoBPList depthBP = ve.ID.VibratoHandle.getDepthBP();
                count = depthBP.getCount();
                if ( count > 0 ) {
                    for ( int i = 0; i < count; i++ ) {
                        VibratoBPPair itemi = depthBP.getElement( i );
                        float percent = itemi.X;
                        int cl = vclock + (int)(percent * vlength);
                        ret.add( new VsqNrpn( cl - vsq.getPresendClockAt( cl, msPreSend ),
                                              NRPN.CC_VD_VIBRATO_DEPTH,
                                              (byte)itemi.Y ) );
                    }
                }
            }
            Collections.sort( ret );
            return ret.toArray( new VsqNrpn[] { } );
        }

        /// <summary>
        /// 指定したシーケンスから、VoiceChangeParameterのNRPNのリストを作成します
        /// </summary>
        /// <param name="vsq">作成元のシーケンス</param>
        /// <param name="track">トラック番号</param>
        /// <param name="msPreSend">プリセンド値(ミリ秒)</param>
        /// <returns>NRPNのリスト</returns>
        public static VsqNrpn[] generateVoiceChangeParameterNRPN( VsqFile vsq, int track, int msPreSend ) {
            int premeasure_clock = vsq.getPreMeasureClocks();
            String renderer = vsq.Track.get( track ).getCommon().Version;
            List<VsqNrpn> res = new List<VsqNrpn>();

            String[] curves;
            if ( renderer.StartsWith( "DSB3" ) ) {
                curves = new String[] { "BRE", "BRI", "CLE", "POR", "OPE", "GEN" };
            } else if ( renderer.StartsWith( "DSB2" ) ) {
                curves = new String[] { "BRE", "BRI", "CLE", "POR", "GEN", "harmonics",
                                        "reso1amp", "reso1bw", "reso1freq", 
                                        "reso2amp", "reso2bw", "reso2freq",
                                        "reso3amp", "reso3bw", "reso3freq",
                                        "reso4amp", "reso4bw", "reso4freq" };
            } else {
                curves = new String[] { "BRE", "BRI", "CLE", "POR", "GEN" };
            }

            for ( int i = 0; i < curves.Length; i++ ) {
                VsqBPList vbpl = vsq.Track.get( track ).getCurve( curves[i] );
                if ( vbpl.size() > 0 ) {
                    byte lsb = NRPN.getVoiceChangeParameterID( curves[i] );
                    int count = vbpl.size();
                    for ( int j = 0; j < count; j++ ) {
                        int clock = vbpl.getKeyClock( j );
                        int c = clock - vsq.getPresendClockAt( clock, msPreSend );
                        if ( c >= 0 ) {
                            VsqNrpn add = new VsqNrpn( c,
                                                       NRPN.VCP_VOICE_CHANGE_PARAMETER_ID,
                                                       lsb );
                            add.append( NRPN.VCP_VOICE_CHANGE_PARAMETER, (byte)vbpl.getElement( j ), true );
                            res.add( add );
                        }
                    }
                }
            }
            Collections.sort( res );
            return res.toArray( new VsqNrpn[] { } );
        }

        /// <summary>
        /// 指定した整数のMSBとLSBを計算します
        /// </summary>
        /// <param name="value">整数値</param>
        /// <returns>キーがMSB、値がLSBとなるペア値</returns>
        public static ValuePair<Byte, Byte> getMsbAndLsb( int value ) {
            ValuePair<Byte, Byte> ret = new ValuePair<Byte, Byte>();
            if ( 0x3fff < value ) {
                ret.setKey( (byte)0x7f );
                ret.setValue( (byte)0x7f );
            } else {
                byte msb = (byte)(value >> 7);
                ret.setKey( msb );
                ret.setValue( (byte)(value - (msb << 7)) );
            }
            return ret;
        }

        /// <summary>
        /// このシーケンスが保持している拍子変更を元に、MIDIイベントリストを作成します
        /// </summary>
        /// <returns>MIDIイベントのリスト</returns>
        public List<MidiEvent> generateTimeSig() {
            List<MidiEvent> events = new List<MidiEvent>();
            for ( Iterator<TimeSigTableEntry> itr = TimesigTable.iterator(); itr.hasNext(); ) {
                TimeSigTableEntry entry = itr.next();
                events.add( MidiEvent.generateTimeSigEvent( entry.Clock, entry.Numerator, entry.Denominator ) );
            }
            return events;
        }

        /// <summary>
        /// このシーケンスが保持しているテンポ変更を元に、MIDIイベントリストを作成します
        /// </summary>
        /// <returns>MIDIイベントのリスト</returns>
        public List<MidiEvent> generateTempoChange() {
            List<MidiEvent> events = new List<MidiEvent>();
            for ( Iterator<TempoTableEntry> itr = TempoTable.iterator(); itr.hasNext(); ) {
                TempoTableEntry entry = itr.next();
                events.add( MidiEvent.generateTempoChangeEvent( entry.Clock, entry.Tempo ) );
                //last_clock = Math.Max( last_clock, entry.Clock );
            }
            return events;
        }

        /// <summary>
        /// このインスタンスをファイルに出力します
        /// </summary>
        /// <param name="file"></param>
        public void write( String file ) {
            write( file, 500, "Shift_JIS" );
        }

        /// <summary>
        /// このインスタンスをファイルに出力します
        /// </summary>
        /// <param name="file"></param>
        /// <param name="msPreSend">プリセンドタイム(msec)</param>
        public void write( String file, int msPreSend, String encoding ) {
#if DEBUG
            PortUtil.println( "VsqFile.Write(String)" );
#endif
            int last_clock = 0;
            int track_size = Track.size();
            for ( int track = 1; track < track_size; track++ ) {
                if ( Track.get( track ).getEventCount() > 0 ) {
                    int index = Track.get( track ).getEventCount() - 1;
                    VsqEvent last = Track.get( track ).getEvent( index );
                    last_clock = Math.Max( last_clock, last.Clock + last.ID.getLength() );
                }
            }

            if ( PortUtil.isFileExists( file ) ) {
                try {
                    PortUtil.deleteFile( file );
                } catch ( Exception ex ) {
                }
            }

            RandomAccessFile fs = null;
            try {
                fs = new RandomAccessFile( file, "rw" );
                long first_position;//チャンクの先頭のファイル位置

                #region  ヘッダ
                //チャンクタイプ
                fs.write( _MTHD, 0, 4 );
                //データ長
                fs.write( (byte)0x00 );
                fs.write( (byte)0x00 );
                fs.write( (byte)0x00 );
                fs.write( (byte)0x06 );
                //フォーマット
                fs.write( (byte)0x00 );
                fs.write( (byte)0x01 );
                //トラック数
                writeUnsignedShort( fs, Track.size() );
                //時間単位
                fs.write( (byte)0x01 );
                fs.write( (byte)0xe0 );
                #endregion

                #region Master Track
                //チャンクタイプ
                fs.write( _MTRK, 0, 4 );
                //データ長。とりあえず0を入れておく
                fs.write( new byte[] { (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00 }, 0, 4 );
                first_position = fs.getFilePointer();
                //トラック名
                writeFlexibleLengthUnsignedLong( fs, 0 );//デルタタイム
                fs.write( (byte)0xff );//ステータスタイプ
                fs.write( (byte)0x03 );//イベントタイプSequence/Track Name
                fs.write( (byte)_MASTER_TRACK.Length );//トラック名の文字数。これは固定
                fs.write( _MASTER_TRACK, 0, _MASTER_TRACK.Length );

                List<MidiEvent> events = new List<MidiEvent>();
                for ( Iterator<TimeSigTableEntry> itr = TimesigTable.iterator(); itr.hasNext(); ) {
                    TimeSigTableEntry entry = itr.next();
                    events.add( MidiEvent.generateTimeSigEvent( entry.Clock, entry.Numerator, entry.Denominator ) );
                    last_clock = Math.Max( last_clock, entry.Clock );
                }
                for ( Iterator<TempoTableEntry> itr = TempoTable.iterator(); itr.hasNext(); ) {
                    TempoTableEntry entry = itr.next();
                    events.add( MidiEvent.generateTempoChangeEvent( entry.Clock, entry.Tempo ) );
                    last_clock = Math.Max( last_clock, entry.Clock );
                }
#if DEBUG
                PortUtil.println( "    events.Count=" + events.size() );
#endif
                Collections.sort( events );
                long last = 0;
                for ( Iterator<MidiEvent> itr = events.iterator(); itr.hasNext(); ) {
                    MidiEvent me = itr.next();
#if DEBUG
                    PortUtil.println( "me.Clock=" + me.clock );
#endif
                    writeFlexibleLengthUnsignedLong( fs, (long)(me.clock - last) );
                    me.writeData( fs );
                    last = me.clock;
                }

                //WriteFlexibleLengthUnsignedLong( fs, (ulong)(last_clock + 120 - last) );
                writeFlexibleLengthUnsignedLong( fs, 0 );
                fs.write( (byte)0xff );
                fs.write( (byte)0x2f );//イベントタイプEnd of Track
                fs.write( (byte)0x00 );
                long pos = fs.getFilePointer();
                fs.seek( first_position - 4 );
                writeUnsignedInt( fs, pos - first_position );
                fs.seek( pos );
                #endregion

                #region トラック
                VsqFile temp = (VsqFile)this.clone();
                temp.Track.get( 1 ).setMaster( (VsqMaster)Master.clone() );
                temp.Track.get( 1 ).setMixer( (VsqMixer)Mixer.clone() );
                printTrack( temp, 1, fs, msPreSend, encoding );
                int count = Track.size();
                for ( int track = 2; track < count; track++ ) {
                    printTrack( this, track, fs, msPreSend, encoding );
                }
                #endregion
            } catch ( Exception ex ) {
            } finally {
                if ( fs != null ) {
                    try {
                        fs.close();
                    } catch ( Exception ex2 ) {
                    }
                }
            }
        }

        /// <summary>
        /// メタテキストの行番号から、各行先頭のプレフィクス文字列("DM:0123:"等)を作成します
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static String getLinePrefix( int count ) {
            int digits = getHowManyDigits( count );
            int c = (digits - 1) / 4 + 1;
            String format = "";
            for ( int i = 0; i < c; i++ ) {
                format += "0000";
            }
            return "DM:" + PortUtil.formatDecimal( format, count ) + ":";
        }

        public static byte[] getLinePrefixBytes( int count ) {
            int digits = getHowManyDigits( count );
            int c = (digits - 1) / 4 + 1;
            String format = "";
            for ( int i = 0; i < c; i++ ) {
                format += "0000";
            }
            String str = "DM:" + PortUtil.formatDecimal( format, count ) + ":";
            byte[] ret = PortUtil.getEncodedByte( "ASCII", str );
            return ret;
        }

        /// <summary>
        /// 数numberの桁数を調べます。（10進数のみ）
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static int getHowManyDigits( int number ) {
            int val;
            if ( number > 0 ) {
                val = number;
            } else {
                val = -number;
            }
            int i = 1;
            int digits = 1;
            while ( true ) {
                i++;
                digits *= 10;
                if ( val < digits ) {
                    return i - 1;
                }
            }
        }

        /// <summary>
        /// char[]を書き込む。
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="item"></param>
        public static void writeCharArray( RandomAccessFile fs, char[] item )
#if JAVA
            throws IOException
#endif
        {
            for ( int i = 0; i < item.Length; i++ ) {
                fs.write( (byte)item[i] );
            }
        }

        /// <summary>
        /// ushort値をビッグエンディアンでfsに書き込みます
        /// </summary>
        /// <param name="data"></param>
        public static void writeUnsignedShort( RandomAccessFile fs, int data )
#if JAVA
            throws IOException
#endif
        {
            byte[] dat = PortUtil.getbytes_uint16_be( data );
            fs.write( dat, 0, dat.Length );
        }

        /// <summary>
        /// uint値をビッグエンディアンでfsに書き込みます
        /// </summary>
        /// <param name="data"></param>
        public static void writeUnsignedInt( RandomAccessFile fs, long data )
#if JAVA
            throws IOException
#endif
        {
            byte[] dat = PortUtil.getbytes_uint32_be( data );
            fs.write( dat, 0, dat.Length );
        }

        /// <summary>
        /// SMFの可変長数値表現を使って、ulongをbyte[]に変換します
        /// </summary>
        /// <param name="number"></param>
        public static byte[] getBytesFlexibleLengthUnsignedLong( long number ) {
            boolean[] bits = new boolean[64];
            long val = (byte)0x1;
            bits[0] = (number & val) == val;
            for ( int i = 1; i < 64; i++ ) {
                val = val << 1;
                bits[i] = (number & val) == val;
            }
            int first = 0;
            for ( int i = 63; i >= 0; i-- ) {
                if ( bits[i] ) {
                    first = i;
                    break;
                }
            }
            // 何バイト必要か？
            int bytes = first / 7 + 1;
            byte[] ret = new byte[bytes];
            for ( int i = 1; i <= bytes; i++ ) {
                int num = 0;
                int count = (byte)0x80;
                for ( int j = (bytes - i + 1) * 7 - 1; j >= (bytes - i + 1) * 7 - 6 - 1; j-- ) {
                    count = count >> 1;
                    if ( bits[j] ) {
                        num += count;
                    }
                }
                if ( i != bytes ) {
                    num += (byte)0x80;
                }
                ret[i - 1] = (byte)num;
            }
            return ret;
        }

        /// <summary>
        /// 整数を書き込む。フォーマットはSMFの可変長数値表現。
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="number"></param>
        public static void writeFlexibleLengthUnsignedLong( RandomAccessFile fs, long number )
#if JAVA
            throws IOException
#endif
        {
            byte[] bytes = getBytesFlexibleLengthUnsignedLong( number );
            fs.write( bytes, 0, bytes.Length );
        }
    }

#if !JAVA
}
#endif
