/*
 * InputBox.cs
 * Copyright © 2008-2011 kbinani
 *
 * This file is part of org.kbinani.windows.forms.
 *
 * org.kbinani.windows.forms is free software; you can redistribute it and/or
 * modify it under the terms of the BSD License.
 *
 * org.kbinani.windows.forms is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 */
#if JAVA
package cadencii.windows.forms;

//INCLUDE-SECTION IMPORT ./InputBox.java

import cadencii.*;

#else

using System;
using System.Windows.Forms;
using cadencii.windows.forms;

namespace cadencii.windows.forms
{
    using BEventArgs = System.EventArgs;
    using BEventHandler = System.EventHandler;
#endif

#if JAVA
    public class InputBox extends BDialog
#else
    public class InputBox : BDialog
#endif
    {
        public InputBox( String message )
        {
#if JAVA
            super();
            initialize();
#else
            InitializeComponent();
#endif
            registerEventHandlers();
            lblMessage.setText( message );
        }

        public String getResult()
        {
            return txtInput.getText();
        }

        public void setResult( String value )
        {
            txtInput.setText( value );
        }

        public void btnCancel_Click( Object sender, BEventArgs e )
        {
            setDialogResult( BDialogResult.CANCEL );
        }

        public void btnOk_Click( Object sender, BEventArgs e )
        {
#if DEBUG
            sout.println( "InputBox#btnOk_Click" );
#endif
            setDialogResult( BDialogResult.OK );
        }

        private void registerEventHandlers()
        {
            btnOk.Click += new BEventHandler( btnOk_Click );
            btnCancel.Click += new BEventHandler( btnCancel_Click );
        }

#if JAVA
        //INCLUDE-SECTION FIELD ./InputBox.java
        //INCLUDE-SECTION METHOD ./InputBox.java
#else
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose( bool disposing ) {
            if ( disposing && (components != null) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this.txtInput = new cadencii.windows.forms.BTextBox();
            this.btnOk = new cadencii.windows.forms.BButton();
            this.lblMessage = new cadencii.windows.forms.BLabel();
            this.btnCancel = new cadencii.windows.forms.BButton();
            this.SuspendLayout();
            //
            // txtInput
            //
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.Location = new System.Drawing.Point( 12, 24 );
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size( 309, 19 );
            this.txtInput.TabIndex = 0;
            //
            // btnOk
            //
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point( 165, 49 );
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size( 75, 23 );
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            //
            // lblMessage
            //
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point( 12, 9 );
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size( 0, 12 );
            this.lblMessage.TabIndex = 2;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point( 246, 49 );
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size( 75, 23 );
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // InputBox
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 12F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size( 333, 82 );
            this.Controls.Add( this.btnCancel );
            this.Controls.Add( this.lblMessage );
            this.Controls.Add( this.btnOk );
            this.Controls.Add( this.txtInput );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "InputBox";
            this.ResumeLayout( false );
            this.PerformLayout();

        }
        #endregion

        private BLabel lblMessage;
        private BButton btnCancel;
        private BTextBox txtInput;
        private BButton btnOk;
#endif
    }

#if !JAVA
}
#endif
