﻿/*
 * AssemblyInfo.cs
 * Copyright (c) 2008-2009 kbinani
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
package org.kbinani.Cadencii;

#else
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle( "Cadencii" )]
[assembly: AssemblyDescription( "" )]
[assembly: AssemblyConfiguration( Boare.Cadencii.BAssemblyInfo.id )]
[assembly: AssemblyCompany( "Boare" )]
[assembly: AssemblyProduct( "Cadencii" )]
[assembly: AssemblyCopyright( "Copyright (C) 2008-2009 kbinani. All Rights Reserved." )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]
[assembly: ComVisible( false )]
[assembly: Guid( "5028b296-d7be-4278-a799-ffaf50026128" )]
[assembly: AssemblyVersion( "1.0.0.0" )]
[assembly: AssemblyFileVersion( Boare.Cadencii.BAssemblyInfo.fileVersion )]

namespace Boare.Cadencii {
#endif

    public class BAssemblyInfo {
        public const String id = "$Id: AssemblyInfo.cs 623 2009-10-31 13:42:11Z kbinani $";
        public const String fileVersion = "3.1.0_DRAFT_7Nov2009";
    }

#if !JAVA
}
#endif