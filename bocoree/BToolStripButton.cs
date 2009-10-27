﻿/*
 * BToolStripButton.cs
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
#if JAVA
package org.kbinani.windows.forms;

import javax.swing.*;
import java.awt.*;
import java.awt.event.*;
import org.kbinani.*;

public class BToolStripButton extends JToggleButton implements ActionListener
{
    public BEvent<BEventHandler> clickEvent = new BEvent<BEventHandler>();
    private Object tag;
    private boolean checkOnClick = true;

    public BToolStripButton()
    {
        super();
        addActionListener( this );
    }

    public boolean isCheckOnClick()
    {
        return checkOnClick;
    }

    public void setCheckOnClick( boolean value )
    {
        checkOnClick = value;
    }

    public void actionPerformed( ActionEvent e )
    {
        if( checkOnClick )
        {
            this.setSelected( !this.isSelected() );
        }
        try
        {
            clickEvent.raise( this, new BEventArgs() );
        }
        catch( Exception ex )
        {
            System.out.println( "BToolStripButton#actionPerformed; ex=" + ex );
        }
    }

    public Object getTag()
    {
        return tag;
    }

    public void setTag( Object value )
    {
        tag = value;
    }
}

#else
#define COMPONENT_PARENT_AS_OWNERITEM
#define COMPONENT_ENABLE_TOOL_TIP_TEXT
#define ABSTRACT_BUTTON_ENABLE_IS_SELECTED
namespace bocoree.windows.forms
{

    public class BToolStripButton : System.Windows.Forms.ToolStripButton
    {
        private object tag = null;

        public object getTag()
        {
            return tag;
        }

        public void setTag( object value )
        {
            tag = value;
        }

        public bool isCheckOnClick()
        {
            return base.CheckOnClick;
        }

        public void setCheckOnClick( bool value )
        {
            base.CheckOnClick = value;
        }

        #region java.awt.Component
#if COMPONENT_ENABLE_TOOL_TIP_TEXT
        public void setToolTipText( string value )
        {
            base.ToolTipText = value;
        }

        public string getToolTipText()
        {
            return base.ToolTipText;
        }
#endif

#if COMPONENT_PARENT_AS_OWNERITEM
        public object getParent() {
            return base.OwnerItem;
        }
#else
        public object getParent()
        {
            return base.Parent;
        }
#endif

        public string getName()
        {
            return base.Name;
        }

        public void setName( string value )
        {
            base.Name = value;
        }

#if COMPONENT_ENABLE_LOCATION
        public bocoree.awt.Point getLocation()
        {
            System.Drawing.Point loc = this.Location;
            return new bocoree.awt.Point( loc.X, loc.Y );
        }

        public void setLocation( int x, int y )
        {
            base.Location = new System.Drawing.Point( x, y );
        }

        public void setLocation( bocoree.awt.Point p )
        {
            base.Location = new System.Drawing.Point( p.x, p.y );
        }
#endif

        public bocoree.awt.Rectangle getBounds()
        {
            System.Drawing.Rectangle r = base.Bounds;
            return new bocoree.awt.Rectangle( r.X, r.Y, r.Width, r.Height );
        }

#if COMPONENT_ENABLE_X
        public int getX()
        {
            return base.Left;
        }
#endif

#if COMPONENT_ENABLE_Y
        public int getY()
        {
            return base.Top;
        }
#endif

        public int getWidth()
        {
            return base.Width;
        }

        public int getHeight()
        {
            return base.Height;
        }

        public bocoree.awt.Dimension getSize()
        {
            return new bocoree.awt.Dimension( base.Size.Width, base.Size.Height );
        }

        public void setSize( int width, int height )
        {
            base.Size = new System.Drawing.Size( width, height );
        }

        public void setSize( bocoree.awt.Dimension d )
        {
            setSize( d.width, d.height );
        }

        public void setBackground( bocoree.awt.Color color )
        {
            base.BackColor = System.Drawing.Color.FromArgb( color.getRed(), color.getGreen(), color.getBlue() );
        }

        public bocoree.awt.Color getBackground()
        {
            return new bocoree.awt.Color( base.BackColor.R, base.BackColor.G, base.BackColor.B );
        }

        public void setForeground( bocoree.awt.Color color )
        {
            base.ForeColor = color.color;
        }

        public bocoree.awt.Color getForeground()
        {
            return new bocoree.awt.Color( base.ForeColor.R, base.ForeColor.G, base.ForeColor.B );
        }

        public void setFont( bocoree.awt.Font font )
        {
            base.Font = font.font;
        }

        public bool getEnabled()
        {
            return base.Enabled;
        }

        public void setEnabled( bool value )
        {
            base.Enabled = value;
        }
        #endregion

        #region javax.swing.AbstractButton
        public string getText() {
            return base.Text;
        }

        public void setText( string value ) {
            base.Text = value;
        }
#if ABSTRACT_BUTTON_ENABLE_IS_SELECTED
        public bool isSelected() {
            return base.Checked;
        }

        public void setSelected( bool value ) {
            base.Checked = value;
        }
#endif
        #endregion
    }

}
#endif