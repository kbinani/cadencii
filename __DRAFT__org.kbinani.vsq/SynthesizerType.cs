/*
 * SynthesizerType.cs
 * Copyright (C) 2009-2010 kbinani
 *
 * This file is part of Boare.Cadencii.
 *
 * Boare.Cadencii is free software; you can redistribute it and/or
 * modify it under the terms of the GPLv3 License.
 *
 * Boare.Cadencii is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 */
#if JAVA
package org.kbinani.vsq;
#else
namespace org.kbinani.vsq {
#endif

    public enum SynthesizerType {
        VOCALOID1,
        VOCALOID2,
    }

#if !JAVA
}
#endif
