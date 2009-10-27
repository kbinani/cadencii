﻿/*
 * Array.cs
 * Copyright (c) 2009 kbinani
 *
 * This file is part of bocoree.
 *
 * bocoree is free software; you can redistribute it and/or
 * modify it under the terms of the BSD License.
 *
 * bocoree is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 */
#if !JAVA
using System;
using System.Collections.Generic;

namespace bocoree.util {

    public static class Arrays {
        public static Vector<T> asList<T>( T[] a ) {
            return new Vector<T>( a );
        }
    }

}
#endif