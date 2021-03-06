/*
 * Stdafx.h
 * Copyright © 1997-2010 M.Kimura
 * Copyright © 2011 kbinani
 *
 * This file is part of CadenciiHelper.
 *
 * CadenciiHelper is free software; you can redistribute it and/or
 * modify it under the terms of the BSD License.
 *
 * CadenciiHelper is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 */
#pragma once

#define _WIN32_WINNT  0x0500
#include <windows.h>
#include <winioctl.h>
#include <stdio.h>

#ifndef REPARSE_DATA_BUFFER_HEADER_SIZE
typedef struct _REPARSE_DATA_BUFFER {
    DWORD  ReparseTag;
    WORD   ReparseDataLength;
    WORD   Reserved;
    union {
        struct {
            WORD   SubstituteNameOffset;
            WORD   SubstituteNameLength;
            WORD   PrintNameOffset;
            WORD   PrintNameLength;
            ULONG  Flags; /* 0=絶対パス, 1=相対パス */
            WCHAR  PathBuffer[1];
        } SymbolicLinkReparseBuffer;
        struct {
            WORD   SubstituteNameOffset;
            WORD   SubstituteNameLength;
            WORD   PrintNameOffset;
            WORD   PrintNameLength;
            WCHAR  PathBuffer[1];
        } MountPointReparseBuffer;
        struct {
            BYTE   DataBuffer[1];
        } GenericReparseBuffer;
    };
} REPARSE_DATA_BUFFER, *PREPARSE_DATA_BUFFER;

#define REPARSE_DATA_BUFFER_HEADER_SIZE   FIELD_OFFSET(REPARSE_DATA_BUFFER, GenericReparseBuffer)
#endif /* REPARSE_DATA_BUFFER_HEADER_SIZE */
