/*
 * VocaloSysUtil.s
 * Copyright (C) 2009-2010 kbinani
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
#if JAVA
package org.kbinani.vsq;

import java.io.*;
import java.util.*;
import org.kbinani.*;
#else
using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace org.kbinani.vsq {
    using boolean = System.Boolean;
#endif

    /// <summary>
    /// VOCALOID / VOCALOID2システムについての情報を取得するユーティリティ。
    /// </summary>
    public class VocaloSysUtil {
        private static Dictionary<SynthesizerType, SingerConfigSys> s_singer_config_sys = new Dictionary<SynthesizerType, SingerConfigSys>();
        private static Dictionary<SynthesizerType, ExpressionConfigSys> s_exp_config_sys = new Dictionary<SynthesizerType, ExpressionConfigSys>();
        private static Dictionary<SynthesizerType, String> s_path_vsti = new Dictionary<SynthesizerType, String>();
        private static Dictionary<SynthesizerType, String> s_path_editor = new Dictionary<SynthesizerType, String>();
        private static Boolean isInitialized = false;
        /// <summary>
        /// VOCALOID1の、デフォルトのSynthesize Engineバージョン。1.0の場合100, 1.1の場合101。規定では100(1.0)。
        /// initメソッドにおいて、VOCALOID.iniから読み取る
        /// </summary>
        private static int defaultDseVersion = 100;
        /// <summary>
        /// VOCALOID1にて、バージョン1.1のSynthesize Engineが利用可能かどうか。
        /// 既定ではfalse。DSE1_1.dllが存在するかどうかで判定。
        /// </summary>
        private static boolean dseVersion101Available = false;

        private VocaloSysUtil() {
        }

        public static SingerConfigSys getSingerConfigSys( SynthesizerType type ) {
            if ( dic.containsKey( s_singer_config_sys, type ) ) {
                return dic.get( s_singer_config_sys, type );
            } else {
                return null;
            }
        }

        /// <summary>
        /// VOCALOID1にて、バージョン1.1のSynthesize Engineが利用可能かどうか。
        /// 既定ではfalse。DSE1_1.dllが存在するかどうかで判定。
        /// </summary>
        public static boolean isDSEVersion101Available() {
            return dseVersion101Available;
        }

        /// <summary>
        /// VOCALOID1の、デフォルトのSynthesize Engineバージョンを取得します。
        /// 1.0の場合100, 1.1の場合101。規定では100(1.0)。
        /// </summary>
        public static int getDefaultDseVersion() {
            if ( !isInitialized ) {
                init();
            }
            return defaultDseVersion;
        }



        /// <summary>
        /// インストールされているVOCALOID / VOCALOID2についての情報を読み込み、初期化します。
        /// </summary>
        public static void init() {
            init( null );
        }

        /// <summary>
        /// インストールされているVOCALOID / VOCALOID2についての情報を読み込み、初期化します。
        /// </summary>
        /// <param name="sw"></param>
        public static void init( BufferedWriter sw ) {
            if ( isInitialized ) {
                return;
            }
            ExpressionConfigSys exp_config_sys1 = null;
            try {
                List<String> dir1 = new List<String>();
                ByRef<String> path_voicedb1 = new ByRef<String>( "" );
                ByRef<String> path_expdb1 = new ByRef<String>( "" );
                List<String> installed_singers1 = new List<String>();
                String header1 = "HKLM\\SOFTWARE\\VOCALOID";
                initPrint( "SOFTWARE\\VOCALOID", header1, dir1 );

                // テキストファイルにレジストリの内容をプリントアウト
                boolean close = false;
#if DEBUG
                if ( sw == null ) {
                    close = true;
                    sw = new BufferedWriter( new FileWriter( 
                        PortUtil.combinePath( PortUtil.getApplicationStartupPath(), "reg_keys_vocalo1.txt" ) ) );
                }
#endif
                if ( sw != null ) {
                    try {
                        sw.write( new String( '#', 72 ) );
                        sw.newLine();
                        foreach ( String s in dir1 ) {
                            sw.write( s );
                            sw.newLine();
                        }
                    } catch ( Exception ex ) {
                        //Logger.write( typeof( VocaloSysUtil ) + ".init; ex=" + ex + "\n" );
                        //PortUtil.stderr.println( "VocaloSysUtil#init; ex=" + ex );
                    } finally {
                        // ファイルは閉じない
                        if ( close && sw != null ) {
                            try {
                                sw.close();
                            } catch ( Exception ex2 ) {
                                //Logger.write( typeof( VocaloSysUtil ) + ".init; ex=" + ex2 + "\n" );
                                //PortUtil.stderr.println( typeof( VocaloSysUtil ) + ".init; ex2=" + ex2 );
                            }
                            sw = null;
                        }
                    }
                }

                ByRef<String> path_vsti = new ByRef<String>( "" );
                ByRef<String> path_editor = new ByRef<String>( "" );
                initExtract( dir1,
                         header1,
                         path_vsti,
                         path_voicedb1,
                         path_expdb1,
                         path_editor,
                         installed_singers1 );
                dic.put( s_path_vsti, SynthesizerType.VOCALOID1, path_vsti.value );
                dic.put( s_path_editor, SynthesizerType.VOCALOID1, path_editor.value );
                SingerConfigSys singer_config_sys = 
                    new SingerConfigSys( path_voicedb1.value, installed_singers1.toArray( new String[] { } ) );
                String expression_map = PortUtil.combinePath( path_expdb1.value, "expression.map" );
                if ( PortUtil.isFileExists( expression_map ) ) {
                    exp_config_sys1 = new ExpressionConfigSys( path_editor.value, path_expdb1.value );
                }
                dic.put( s_singer_config_sys, SynthesizerType.VOCALOID1, singer_config_sys );

                // DSE1_1.dllがあるかどうか？
                if ( !path_vsti.value.Equals( "" ) ) {
                    String path_dll = PortUtil.getDirectoryName( path_vsti.value );
                    String dse1_1 = PortUtil.combinePath( path_dll, "DSE1_1.dll" );
                    dseVersion101Available = PortUtil.isFileExists( dse1_1 );
                } else {
                    dseVersion101Available = false;
                }

                // VOCALOID.iniから、DSEVersionを取得
                if ( path_editor.value != null && !path_editor.value.Equals( "" ) && 
                     PortUtil.isFileExists( path_editor.value ) ) {
                    String dir = PortUtil.getDirectoryName( path_editor.value );
                    String ini = PortUtil.combinePath( dir, "VOCALOID.ini" );
                    if ( PortUtil.isFileExists( ini ) ) {
                        BufferedReader br = null;
                        try {
                            br = new BufferedReader( new InputStreamReader( new FileInputStream( ini ), "Shift_JIS" ) );
                            while ( br.ready() ) {
                                String line = br.readLine();
                                if ( line == null ) continue;
                                if ( line.Equals( "" ) ) continue;
                                if ( line.StartsWith( "DSEVersion" ) ) {
                                    String[] spl = PortUtil.splitString( line, '=' );
                                    if ( spl.Length >= 2 ) {
                                        String str_dse_version = spl[1];
                                        try {
                                            defaultDseVersion = str.toi( str_dse_version );
                                        } catch ( Exception ex ) {
                                            PortUtil.stderr.println( "VocaloSysUtil#init; ex=" + ex );
                                            defaultDseVersion = 100;
                                        }
                                    }
                                    break;
                                }
                            }
                        } catch ( Exception ex ) {
                            PortUtil.stderr.println( "VocaloSysUtil#init; ex=" + ex );
                        } finally {
                            if ( br != null ) {
                                try {
                                    br.close();
                                } catch ( Exception ex2 ) {
                                    PortUtil.stderr.println( "VocaloSysUtil#init; ex2=" + ex2 );
                                }
                            }
                        }
                    }
                }
            } catch ( Exception ex ) {
                PortUtil.stderr.println( "VocaloSysUtil#init; ex=" + ex );
                SingerConfigSys singer_config_sys = new SingerConfigSys( "", new String[] { } );
                exp_config_sys1 = null;
                dic.put( s_singer_config_sys, SynthesizerType.VOCALOID1, singer_config_sys );
            }
            if ( exp_config_sys1 == null ) {
                exp_config_sys1 = ExpressionConfigSys.getVocaloid1Default();
            }
            dic.put( s_exp_config_sys, SynthesizerType.VOCALOID1, exp_config_sys1 );

            ExpressionConfigSys exp_config_sys2 = null;
            try {
                List<String> dir2 = new List<String>();
                ByRef<String> path_voicedb2 = new ByRef<String>( "" );
                ByRef<String> path_expdb2 = new ByRef<String>( "" );
                List<String> installed_singers2 = new List<String>();
                String header2 = "HKLM\\SOFTWARE\\VOCALOID2";
                initPrint( "SOFTWARE\\VOCALOID2", header2, dir2 );

                // レジストリの中身をファイルに出力
                boolean close = false;
#if DEBUG
                if ( sw == null ) {
                    sw = new BufferedWriter( new FileWriter(
                        PortUtil.combinePath( PortUtil.getApplicationStartupPath(), "reg_keys_vocalo2.txt" ) ) );
                    close = true;
                }
#endif
                if ( sw != null ) {
                    try {
                        sw.write( new String( '#', 72 ) );
                        sw.newLine();
                        foreach ( String s in dir2 ) {
                            sw.write( s );
                            sw.newLine();
                        }
                    } catch ( Exception ex ) {
                        //Logger.write( typeof( VocaloSysUtil ) + ".init; ex=" + ex + "\n" );
                        //PortUtil.stderr.println( "VocaloSysUtil#.cctor; ex=" + ex );
                    } finally {
                        if ( close && sw != null ) {
                            try {
                                sw.close();
                            } catch ( Exception ex2 ) {
                                //Logger.write( typeof( VocaloSysUtil ) + ".init; ex=" + ex2 + "\n" );
                                //PortUtil.stderr.println( "VocaloSysUtil#.cctor; ex2=" + ex2 );
                            }
                        }
                    }
                }

                ByRef<String> path_vsti = new ByRef<String>( "" );
                ByRef<String> path_editor = new ByRef<String>( "" );
                initExtract( dir2,
                         header2,
                         path_vsti,
                         path_voicedb2,
                         path_expdb2,
                         path_editor,
                         installed_singers2 );
                dic.put( s_path_vsti, SynthesizerType.VOCALOID2, path_vsti.value );
                dic.put( s_path_editor, SynthesizerType.VOCALOID2, path_editor.value );
                SingerConfigSys singer_config_sys = new SingerConfigSys( path_voicedb2.value, installed_singers2.toArray( new String[] { } ) );
                if ( PortUtil.isFileExists( PortUtil.combinePath( path_expdb2.value, "expression.map" ) ) ) {
                    exp_config_sys2 = new ExpressionConfigSys( path_editor.value, path_expdb2.value );
                }
                dic.put( s_singer_config_sys, SynthesizerType.VOCALOID2, singer_config_sys );
            } catch ( Exception ex ) {
                PortUtil.stderr.println( "VocaloSysUtil..cctor; ex=" + ex );
                SingerConfigSys singer_config_sys = new SingerConfigSys( "", new String[] { } );
                exp_config_sys2 = null;
                dic.put( s_singer_config_sys, SynthesizerType.VOCALOID2, singer_config_sys );
            }
            if ( exp_config_sys2 == null ) {
#if DEBUG
                PortUtil.println( "VocaloSysUtil#.ctor; loading default ExpressionConfigSys..." );
#endif
                exp_config_sys2 = ExpressionConfigSys.getVocaloid2Default();
            }
            dic.put( s_exp_config_sys, SynthesizerType.VOCALOID2, exp_config_sys2 );

            isInitialized = true;
        }

        /// <summary>
        /// ビブラートのプリセットタイプから，VibratoHandleを作成します
        /// </summary>
        /// <param name="icon_id"></param>
        /// <param name="vibrato_length"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static VibratoHandle getDefaultVibratoHandle( String icon_id, int vibrato_length, SynthesizerType type ) {
            if ( !isInitialized ) {
                init();
            }
            if ( dic.containsKey( s_exp_config_sys, type ) ) {
                for ( Iterator<VibratoHandle> itr = dic.get( s_exp_config_sys, type ).vibratoConfigIterator(); itr.hasNext(); ) {
                    VibratoHandle vconfig = itr.next();
                    if ( vconfig.IconID.Equals( icon_id ) ) {
                        VibratoHandle ret = (VibratoHandle)vconfig.clone();
                        ret.setLength( vibrato_length );
                        return ret;
                    }
                }
            }
            VibratoHandle empty = new VibratoHandle();
            empty.IconID = "$04040000";
            return empty;
        }

        private static void initExtract( List<String> dir,
                                     String header,
                                     ByRef<String> path_vsti,
                                     ByRef<String> path_voicedb,
                                     ByRef<String> path_expdb,
                                     ByRef<String> path_editor,
                                     List<String> installed_singers ) {
            List<String> application = new List<String>();
            List<String> expression = new List<String>();
            List<String> voice = new List<String>();
            path_vsti.value = "";
            path_expdb.value = "";
            path_voicedb.value = "";
            path_editor.value = "";
            int size = vec.size( dir );
            for ( int i = 0; i < size; i++ ) {
                String s = vec.get( dir, i );
                if ( s.StartsWith( header + "\\APPLICATION" ) ) {
                    vec.add( application, s.Substring( str.length( header + "\\APPLICATION" ) ) );
                } else if ( s.StartsWith( header + "\\DATABASE\\EXPRESSION" ) ) {
                    vec.add( expression, s.Substring( str.length( header + "\\DATABASE\\EXPRESSION" ) ) );
                } else if ( s.StartsWith( header + "\\DATABASE\\VOICE" ) ) {
                    vec.add( voice, s.Substring( str.length( header + "\\DATABASE\\VOICE\\" ) ) );
                }
            }

            // path_vstiを取得
            size = vec.size( application );
            for ( int i = 0; i < size; i++ ) {
                String s = vec.get( application, i );
                String[] spl = PortUtil.splitString( s, '\t' );
                if ( spl.Length >= 3 && spl[1].Equals( "PATH" ) ) {
                    if ( spl[2].ToLower().EndsWith( ".dll" ) ) {
                        path_vsti.value = spl[2];
                    } else if ( spl[2].ToLower().EndsWith( ".exe" ) ) {
                        path_editor.value = spl[2];
                    }
                }
            }

            // path_vicedbを取得
            Dictionary<String, String> install_dirs = new Dictionary<String, String>();
            size = vec.size( voice );
            for ( int i = 0; i < size; i++ ) {
                String s = vec.get( voice, i );
                String[] spl = PortUtil.splitString( s, '\t' );
                if ( spl.Length < 2 ) {
                    continue;
                }

                if ( spl[0].Equals( "VOICEDIR" ) ) {
                    path_voicedb.value = spl[1];
                } else if ( spl.Length >= 3 ) {
                    String[] spl2 = PortUtil.splitString( spl[0], '\\' );
                    if ( spl2.Length == 1 ) {
                        String id = spl2[0]; // BHXXXXXXXXXXXXみたいなシリアル
                        if ( !dic.containsKey( install_dirs, id ) ) {
                            dic.put( install_dirs, id, "" );
                        }
                        if ( spl[1].Equals( "INSTALLDIR" ) ) {
                            // VOCALOID1の場合は、ここには到達しないはず
                            String installdir = spl[2];
                            dic.put( install_dirs, id, PortUtil.combinePath( installdir, id ) );
                        }
                    }
                }
            }

            // installed_singersに追加
            for ( Iterator<String> itr = install_dirs.keySet().iterator(); itr.hasNext(); ) {
                String id = itr.next();
                String install = dic.get( install_dirs, id );
                if ( install.Equals( "" ) ) {
                    install = PortUtil.combinePath( path_voicedb.value, id );
                }
                vec.add( installed_singers, install );
            }

            // path_expdbを取得
            List<String> exp_ids = new List<String>();
            // 最初はpath_expdbの取得と、id（BHXXXXXXXXXXXXXXXX）のようなシリアルを取得
            size = vec.size( expression );
            for ( int i = 0; i < size; i++ ) {
                String s = vec.get( expression, i );
                String[] spl = PortUtil.splitString( s, new char[] { '\t' }, true );
                if ( spl.Length >= 3 ) {
                    if ( spl[1].Equals( "EXPRESSIONDIR" ) ) {
                        path_expdb.value = spl[2];
                    } else if ( spl.Length >= 3 ) {
                        String[] spl2 = PortUtil.splitString( spl[0], '\\' );
                        if ( spl2.Length == 1 ) {
                            if ( !vec.contains( exp_ids, spl2[0] ) ) {
                                vec.add( exp_ids, spl2[0] );
                            }
                        }
                    }
                }
            }
#if DEBUG
            PortUtil.println( "path_vsti=" + path_vsti.value );
            PortUtil.println( "path_voicedb=" + path_voicedb.value );
            PortUtil.println( "path_expdb=" + path_expdb.value );
            PortUtil.println( "installed_singers=" );
#endif
        }

        /// <summary>
        /// レジストリkey内の値を再帰的に検索し、ファイルfpに順次出力する
        /// </summary>
        /// <param name="reg_key_name"></param>
        /// <param name="parent_name"></param>
        /// <param name="list"></param>
        private static void initPrint( String reg_key_name, String parent_name, List<String> list ) {
#if JAVA
#else
            try {
                RegistryKey key = Registry.LocalMachine.OpenSubKey( reg_key_name, false );
                if ( key == null ) {
                    return;
                }

                // 直下のキー内を再帰的にリストアップ
                String[] subkeys = key.GetSubKeyNames();
                foreach ( String s in subkeys ) {
                    initPrint( reg_key_name + "\\" + s, parent_name + "\\" + s, list );
                }

                // 直下の値を出力
                String[] valuenames = key.GetValueNames();
                foreach ( String s in valuenames ) {
                    RegistryValueKind kind = key.GetValueKind( s );
                    if ( kind == RegistryValueKind.String ) {
                        String str = parent_name + "\t" + s + "\t" + (String)key.GetValue( s, "" );
                        vec.add( list, str );
                    }
                }
                key.Close();
            } catch ( Exception ex ) {
                PortUtil.stderr.println( "VocaloSysUtil#initPrint; ex=" + ex );
            }
#endif
        }

        /// <summary>
        /// アタック設定を順に返す反復子を取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Iterator<NoteHeadHandle> attackConfigIterator( SynthesizerType type ) {
            if ( !isInitialized ) {
                init();
            }
            if ( dic.containsKey( s_exp_config_sys, type ) ) {
                return dic.get( s_exp_config_sys, type ).attackConfigIterator();
            } else {
                return (new List<NoteHeadHandle>()).iterator();
            }
        }

        /// <summary>
        /// ビブラート設定を順に返す反復子を取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Iterator<VibratoHandle> vibratoConfigIterator( SynthesizerType type ) {
            if ( !isInitialized ) {
                init();
            }
            if ( dic.containsKey( s_exp_config_sys, type ) ) {
                return dic.get( s_exp_config_sys, type ).vibratoConfigIterator();
            } else {
                return (new List<VibratoHandle>()).iterator();
            }
        }

        /// <summary>
        /// 強弱記号設定を順に返す反復子を取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Iterator<IconDynamicsHandle> dynamicsConfigIterator( SynthesizerType type ) {
            if ( !isInitialized ) {
                init();
            }
            if ( dic.containsKey( s_exp_config_sys, type ) ) {
                return dic.get( s_exp_config_sys, type ).dynamicsConfigIterator();
            } else {
                return (new List<IconDynamicsHandle>()).iterator();
            }
        }

        /// <summary>
        /// 指定した歌声合成システムに登録されている指定した名前の歌手について、その派生元の歌手名を取得します。
        /// </summary>
        /// <param name="language"></param>
        /// <param name="program"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static String getOriginalSinger( int language, int program, SynthesizerType type ) {
            if ( !isInitialized ) {
                init();
            }
            String voiceidstr = "";
            if ( !dic.containsKey( s_singer_config_sys, type ) ) {
                return "";
            }
            SingerConfigSys scs = dic.get( s_singer_config_sys, type );
            SingerConfig[] singer_configs = scs.getSingerConfigs();
            for ( int i = 0; i < singer_configs.Length; i++ ) {
                if ( language == singer_configs[i].Language && program == singer_configs[i].Program ) {
                    voiceidstr = singer_configs[i].VOICEIDSTR;
                    break;
                }
            }
            if ( voiceidstr.Equals( "" ) ) {
                return "";
            }
            SingerConfig[] installed_singers = scs.getInstalledSingers();
            for ( int i = 0; i < installed_singers.Length; i++ ) {
                if ( voiceidstr.Equals( installed_singers[i].VOICEIDSTR ) ) {
                    return installed_singers[i].VOICENAME;
                }
            }
            return "";
        }

        /// <summary>
        /// 指定した歌声合成システムに登録されている指定した名前の歌手について、その歌手を表現するVsqIDのインスタンスを取得します。
        /// </summary>
        /// <param name="language"></param>
        /// <param name="program"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static VsqID getSingerID( int language, int program, SynthesizerType type ) {
            if ( !isInitialized ) {
                init();
            }
            if ( !dic.containsKey( s_singer_config_sys, type ) ) {
                return null;
            } else {
                return dic.get( s_singer_config_sys, type ).getSingerID( language, program );
            }
        }

        /// <summary>
        /// 指定した歌声合成システムの、エディタの実行ファイルのパスを取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static String getEditorPath( SynthesizerType type ) {
            if ( !isInitialized ) {
                init();
            }
            if ( !dic.containsKey( s_path_editor, type ) ) {
                return "";
            } else {
                return s_path_editor.get( type );
            }
        }

        /// <summary>
        /// 指定した歌声合成システムの、VSTi DLL本体のパスを取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static String getDllPathVsti( SynthesizerType type ) {
            if ( !isInitialized ) {
                init();
            }
            if ( !s_path_vsti.containsKey( type ) ) {
                return "";
            } else {
                return s_path_vsti.get( type );
            }
        }

        /// <summary>
        /// 指定された歌声合成システムに登録されている歌手設定のリストを取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SingerConfig[] getSingerConfigs( SynthesizerType type ) {
            if ( !isInitialized ) {
                init();
            }
            if ( !s_singer_config_sys.containsKey( type ) ) {
                return new SingerConfig[] { };
            } else {
                return s_singer_config_sys.get( type ).getSingerConfigs();
            }
        }

        /// <summary>
        /// 指定した名前の歌手の歌唱言語を取得します。
        /// </summary>
        /// <param name="name">name of singer</param>
        /// <returns></returns>
        public static VsqVoiceLanguage getLanguageFromName( String name ) {
            if ( !isInitialized ) {
                init();
            }
            String search = name.ToLower();
            if ( search.Equals( "meiko" ) ||
                search.Equals( "kaito" ) ||
                search.Equals( "miku" ) ||
                search.Equals( "rin" ) ||
                search.Equals( "len" ) ||
                search.Equals( "rin_act2" ) ||
                search.Equals( "len_act2" ) ||
                search.Equals( "gackpoid" ) ||
                search.Equals( "luka_jpn" ) ||
                search.Equals( "megpoid" ) ||
                search.Equals( "sfa2_miki" ) ||
                search.Equals( "yuki" ) ||
                search.Equals( "kiyoteru" ) ||
                search.Equals( "miku_sweet" ) ||
                search.Equals( "miku_dark" ) ||
                search.Equals( "miku_soft" ) ||
                search.Equals( "miku_light" ) ||
                search.Equals( "miku_vivid" ) ||
                search.Equals( "miku_solid" ) )
            {
                return VsqVoiceLanguage.Japanese;
            } else if ( search.Equals( "sweet_ann" ) ||
                search.Equals( "prima" ) ||
                search.Equals( "luka_eng" ) ||
                search.Equals( "sonika" ) ||
                search.Equals( "lola" ) ||
                search.Equals( "leon" ) ||
                search.Equals( "miriam" ) ||
                search.Equals( "big_al" ) )
            {
                return VsqVoiceLanguage.English;
            }
            return VsqVoiceLanguage.Japanese;
        }

        /// <summary>
        /// 指定したPAN値における、左チャンネルの増幅率を取得します。
        /// </summary>
        /// <param name="pan"></param>
        /// <returns></returns>
        public static double getAmplifyCoeffFromPanLeft( int pan ) {
            return pan / -64.0 + 1.0;
        }

        /// <summary>
        /// 指定したPAN値における、右チャンネルの増幅率を取得します。
        /// </summary>
        /// <param name="pan"></param>
        /// <returns></returns>
        public static double getAmplifyCoeffFromPanRight( int pan ) {
            return pan / 64.0 + 1.0;
        }

        /// <summary>
        /// 指定したFEDER値における、増幅率を取得します。
        /// </summary>
        /// <param name="feder"></param>
        /// <returns></returns>
        public static double getAmplifyCoeffFromFeder( int feder ) {
            return Math.Exp( 1.18448420e-01 * feder / 10.0 );
        }
    }

#if !JAVA
}
#endif
