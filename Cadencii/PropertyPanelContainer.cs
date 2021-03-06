#if ENABLE_PROPERTY
/*
 * PropertyPanelContainer.cs
 * Copyright © 2009-2011 kbinani
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
package com.github.cadencii;

//INCLUDE-SECTION IMPORT ./ui/java/PropertyPanelContainer.java
import javax.swing.*;
import java.awt.*;
import com.github.cadencii.*;
import com.github.cadencii.windows.forms.*;
#else

using System;
using System.Windows.Forms;
using com.github.cadencii.java.awt;
using com.github.cadencii.windows.forms;

namespace com.github.cadencii
{
    using BMouseEventArgs = System.Windows.Forms.MouseEventArgs;
    using BEventHandler = System.EventHandler;
    using BMouseEventHandler = System.Windows.Forms.MouseEventHandler;
    using boolean = System.Boolean;
#endif

#if JAVA
    public class PropertyPanelContainer extends BPanel
#else
    public class PropertyPanelContainer : UserControl
#endif
    {
        public const int _TITLE_HEIGHT = 29;
#if JAVA
        public BEvent<StateChangeRequiredEventHandler> stateChangeRequiredEvent = new BEvent<StateChangeRequiredEventHandler>();
#else
        public event StateChangeRequiredEventHandler StateChangeRequired;
#endif

        public PropertyPanelContainer()
        {
#if JAVA
            super();
            initialize();
#else
            InitializeComponent();
#endif
            registerEventHandlers();
            setResources();
        }

#if !JAVA
        public void addComponent( Control c )
        {
            panelMain.Controls.Add( c );
            c.Dock = DockStyle.Fill;
        }
#endif

#if !JAVA
        public void panelTitle_MouseDoubleClick( Object sender, BMouseEventArgs e )
        {
            handleRestoreWindow();
        }
#endif

#if !JAVA
        public void btnClose_Click( Object sender, EventArgs e )
        {
            handleClose();
        }
#endif

#if !JAVA
        public void btnWindow_Click( Object sender, EventArgs e )
        {
            handleRestoreWindow();
        }
#endif

        private void handleClose()
        {
            invokeStateChangeRequiredEvent( PanelState.Hidden );
        }

        private void handleRestoreWindow()
        {
            invokeStateChangeRequiredEvent( PanelState.Window );
        }

        private void invokeStateChangeRequiredEvent( PanelState state )
        {
#if JAVA
            try{
                stateChangeRequiredEvent.raise( this, state );
            }catch( Exception ex ){
                serr.println( "PropertyPanelContainer#invokeStateChangeRequiredEvent; ex=" + ex );
            }
#else
            if ( StateChangeRequired != null ) {
                StateChangeRequired( this, state );
            }
#endif
        }

#if !JAVA
        /// <summary>
        /// javaは自動レイアウトなのでいらない
        /// </summary>
        private void panelMain_SizeChanged( Object sender, EventArgs e )
        {
            panelTitle.Left = 0;
            panelTitle.Top = 0;
            panelTitle.Height = _TITLE_HEIGHT;
            panelTitle.Width = this.Width;

            panelMain.Top = _TITLE_HEIGHT;
            panelMain.Left = 0;
            panelMain.Width = this.Width;
            panelMain.Height = this.Height - _TITLE_HEIGHT;
        }
#endif

        private void registerEventHandlers()
        {
#if !JAVA
            this.panelMain.SizeChanged += new BEventHandler( panelMain_SizeChanged );
            this.btnClose.Click += new BEventHandler( btnClose_Click );
            this.btnWindow.Click += new BEventHandler( btnWindow_Click );
            this.panelTitle.MouseDoubleClick += new BMouseEventHandler( panelTitle_MouseDoubleClick );
#endif
        }

        private void setResources()
        {
#if !JAVA
            this.btnClose.setIcon( new ImageIcon( Resources.get_cross_small() ) );
            this.btnWindow.setIcon( new ImageIcon( Resources.get_chevron_small_collapse() ) );
#endif
        }

#if JAVA
        #region ui impl for Java
        //INCLUDE-SECTION FIELD ./ui/java/PropertyPanelContainer.java
        //INCLUDE-SECTION METHOD ./ui/java/PropertyPanelContainer.java
        #endregion
#else
        #region ui impl for C#
        /// <summary> 
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose( boolean disposing )
        {
            if ( disposing && (components != null) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region コンポーネント デザイナで生成されたコード

        /// <summary> 
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.panelMain = new System.Windows.Forms.Panel();
            this.btnClose = new BButton();
            this.btnWindow = new BButton();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.panelTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMain.Location = new System.Drawing.Point( 0, 29 );
            this.panelMain.Margin = new System.Windows.Forms.Padding( 0 );
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size( 159, 283 );
            this.panelMain.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point( 133, 3 );
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size( 23, 23 );
            this.btnClose.TabIndex = 1;
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // btnWindow
            // 
            this.btnWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWindow.Location = new System.Drawing.Point( 104, 3 );
            this.btnWindow.Name = "btnWindow";
            this.btnWindow.Size = new System.Drawing.Size( 23, 23 );
            this.btnWindow.TabIndex = 2;
            this.btnWindow.UseVisualStyleBackColor = true;
            // 
            // panelTitle
            // 
            this.panelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTitle.Controls.Add( this.btnWindow );
            this.panelTitle.Controls.Add( this.btnClose );
            this.panelTitle.Location = new System.Drawing.Point( 0, 0 );
            this.panelTitle.Margin = new System.Windows.Forms.Padding( 0 );
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size( 159, 29 );
            this.panelTitle.TabIndex = 3;
            // 
            // PropertyPanelContainer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add( this.panelTitle );
            this.Controls.Add( this.panelMain );
            this.Name = "PropertyPanelContainer";
            this.Size = new System.Drawing.Size( 159, 312 );
            this.panelTitle.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private BButton btnClose;
        private BButton btnWindow;
        private System.Windows.Forms.Panel panelTitle;
        #endregion
#endif
    }

#if !JAVA
}
#endif
#endif
