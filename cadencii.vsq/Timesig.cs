/*
 * Timesig.cs
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
package cadencii.vsq;
#else
namespace cadencii.vsq
{
#endif

    public struct Timesig
    {
        public int numerator;
        public int denominator;

#if JAVA
        public Timesig(){
        }
#endif

        public Timesig( int numerator, int denominator )
        {
            this.numerator = numerator;
            this.denominator = denominator;
        }
    }

#if !JAVA
}
#endif
