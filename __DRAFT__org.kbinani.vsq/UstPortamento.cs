/*
 * UstPortamento.cs
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

import java.util.*;
import java.io.*;
import org.kbinani.*;
#else
using System;
using System.Collections.Generic;

namespace org.kbinani.vsq {
    using boolean = System.Boolean;
#endif

#if JAVA
    public class UstPortamento implements Cloneable, Serializable {
#else
    [Serializable]
    public class UstPortamento : ICloneable {
#endif
        public List<UstPortamentoPoint> Points = new List<UstPortamentoPoint>();
        public int Start;

        /// <summary>
        /// このクラスの指定した名前のプロパティが総称型引数を用いる型である場合に，
        /// その型の限定名を返します．それ以外の場合は空文字を返します．
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String getGenericTypeName( String name ) {
            if ( name != null ) {
                if ( name.Equals( "Points" ) ) {
                    return "org.kbinani.vsq.UstPortamentoPoint";
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

        public void print( BufferedWriter sw )
#if JAVA
            throws IOException
#endif
        {
            String pbw = "";
            String pby = "";
            String pbm = "";
            int count = vec.size( Points );
            for ( int i = 0; i < count; i++ ) {
                String comma = (i == 0 ? "" : ",");
                pbw += comma + vec.get( Points, i ).Step;
                pby += comma + vec.get( Points, i ).Value;
                String type = "";
                UstPortamentoType ut = vec.get( Points, i ).Type;
                if ( ut == UstPortamentoType.S ) {
                    type = "";
                } else if ( ut == UstPortamentoType.Linear ) {
                    type = "s";
                } else if ( ut == UstPortamentoType.R ) {
                    type = "r";
                } else if ( ut == UstPortamentoType.J ) {
                    type = "j";
                }
                pbm += comma + type;
            }
            sw.write( "PBW=" + pbw );
            sw.newLine();
            sw.write( "PBS=" + Start );
            sw.newLine();
            sw.write( "PBY=" + pby );
            sw.newLine();
            sw.write( "PBM=" + pbm );
            sw.newLine();
        }

        public Object clone() {
            UstPortamento ret = new UstPortamento();
            int count = vec.size( Points );
            for ( int i = 0; i < count; i++ ) {
                vec.add( ret.Points, vec.get( Points, i ) );
            }
            ret.Start = Start;
            return ret;
        }

#if !JAVA
        public object Clone() {
            return clone();
        }
#endif

        /*
        PBW=50,50,46,48,56,50,50,50,50
        PBS=-87
        PBY=-15.9,-20,-31.5,-26.6
        PBM=,s,r,j,s,s,s,s,s
        */
        public void ParseLine( String line ) {
            line = line.ToLower();
#if JAVA
            Vector<String> spl = new Vector<String>();
#elif __cplusplus
            vector<string> spl;
#else
            List<string> spl = new List<string>();
#endif
            int len = str.split( line, spl, "=", false );
            if ( len == 0 ) {
                return;
            }
            List<string> values = new List<string>();
            str.split( vec.get( spl, 1 ), values, ",", false );// PortUtil.splitString( spl[1], ',' );
            if ( str.startsWith( line, "pbs=" ) ) {
                Start = str.toi( values[0] );
            } else if ( line.StartsWith( "pbw=" ) ) {
                for ( int i = 0; i < values.Length; i++ ) {
                    if ( i >= Points.size() ) {
                        Points.add( new UstPortamentoPoint() );
                    }
                    UstPortamentoPoint up = Points.get( i );
                    up.Step = (float)str.toi( values[i] );
                    Points.set( i, up );
                }
            } else if ( line.StartsWith( "pby=" ) ) {
                for ( int i = 0; i < values.Length; i++ ) {
                    if ( i >= Points.size() ) {
                        Points.add( new UstPortamentoPoint() );
                    }
                    UstPortamentoPoint up = Points.get( i );
                    up.Value = (float)str.tof( values[i] );
                    Points.set( i, up );
                }
            } else if ( line.StartsWith( "pbm=" ) ) {
                for ( int i = 0; i < values.Length; i++ ) {
                    if ( i >= Points.size() ) {
                        Points.add( new UstPortamentoPoint() );
                    }
                    UstPortamentoPoint up = Points.get( i );
                    String search = values[i].ToLower();
                    if ( search.Equals( "s" ) ) {
                        up.Type = UstPortamentoType.Linear;
                    } else if ( search.Equals( "r" ) ) {
                        up.Type = UstPortamentoType.R;
                    } else if ( search.Equals( "j" ) ) {
                        up.Type = UstPortamentoType.J;
                    } else {
                        up.Type = UstPortamentoType.S;
                    }
                    Points.set( i, up );
                }
            } else if ( line.StartsWith( "pbs=" ) ) {

            }
        }
    }

#if !JAVA
}
#endif
