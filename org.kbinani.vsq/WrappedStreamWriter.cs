/*
 * WrappedStreamWriter.cs
 * Copyright © 2009-2011 kbinani
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
package com.github.cadencii.vsq;

import java.io.*;
#else
using System;
using com.github.cadencii;
using com.github.cadencii.java.io;

namespace com.github.cadencii.vsq
{
#endif

#if JAVA
    public class WrappedStreamWriter implements ITextWriter {
#else
    public class WrappedStreamWriter : ITextWriter
    {
#endif
        BufferedWriter m_writer;

        public WrappedStreamWriter( BufferedWriter stream_writer )
        {
            m_writer = stream_writer;
        }

        public void newLine()
#if JAVA
            throws java.io.IOException
#endif
        {
            m_writer.newLine();
        }

        public void write( String value )
#if JAVA
            throws IOException
#endif
        {
            m_writer.write( value );
        }

        public void writeLine( String value )
#if JAVA
            throws IOException
#endif
        {
            m_writer.write( value );
            m_writer.newLine();
        }

        public void close()
#if JAVA
            throws IOException
#endif
        {
            m_writer.close();
        }
    }

#if !JAVA
}
#endif
