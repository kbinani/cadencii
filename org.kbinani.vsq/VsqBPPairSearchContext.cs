/*
 * VsqBPPairSearchContext.cs
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
#else
namespace com.github.cadencii.vsq
{
#endif

    public struct VsqBPPairSearchContext
    {
        public int clock;
        public int index;
        public VsqBPPair point;
    }

#if !JAVA
}
#endif
