﻿/*
 * WaveGenerator.cs
 * Copyright (C) 2010 kbinani
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
#else
using System;

namespace org.kbinani.cadencii {
#endif

    /// <summary>
    /// 音声波形の生成器のためのインターフェース．
    /// このインターフェースを実装するクラスは，WaveUnitクラスを継承すること．
    /// </summary>
    public interface WaveGenerator {
        /// <summary>
        /// 音声波形の合成を開始します．
        /// このメソッドの前には，setGlobalConfig, setConfig, initメソッドをこの順で呼び出して
        /// 必要なパラメータを全て渡すようにしてください．
        /// (この順番に呼ばれることを前提とした実装をしなくてはならない)
        /// </summary>
        /// <param name="samples"></param>
        void begin( long samples );

        /// <summary>
        /// この音声波形器が生成した波形を受け取る装置を設定します．
        /// </summary>
        /// <param name="r"></param>
        void setReceiver( WaveReceiver r );

        /// <summary>
        /// 音声波形の合成に必要な引数を設定します．
        /// </summary>
        /// <param name="vsq"></param>
        /// <param name="track"></param>
        /// <param name="start_clock"></param>
        /// <param name="end_clock"></param>
        void init( VsqFileEx vsq, int track, int start_clock, int end_clock );
    }

#if !JAVA
}
#endif