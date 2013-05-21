/*
 * FormSingerStypeConfig.cs
 * Copyright © 2008-2011 kbinani
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
package cadencii;

//INCLUDE-SECTION IMPORT ./ui/java/FormSingerStyleConfig.java

import java.awt.*;
import java.awt.event.*;
import cadencii.*;
import cadencii.apputil.*;
import cadencii.windows.forms.*;
#else
using System;
using System.Windows.Forms;
using cadencii.apputil;
using cadencii;
using cadencii.java.awt;
using cadencii.java.awt.event_;
using cadencii.windows.forms;

namespace cadencii
{
    using BEventHandler = System.EventHandler;
    using boolean = System.Boolean;

#endif

#if JAVA
    public class FormSingerStyleConfig extends BDialog {
#else
    class FormSingerStyleConfig : BDialog
    {
#endif
        boolean m_apply_current_track = false;

        public FormSingerStyleConfig()
        {
#if JAVA
            super();
            initialize();
#else
            InitializeComponent();
#endif

            comboTemplate.removeAllItems();
            String[] strs = new String[]{
                "[Select a template]",
                "normal",
                "accent",
                "strong accent",
                "legato",
                "slow legate",
            };
            for( int i = 0; i < strs.Length; i++ ){
                comboTemplate.addItem( strs[i] );
            }

            registerEventHandlers();
            setResources();
            Util.applyFontRecurse( this, AppManager.editorConfig.getBaseFont() );
            applyLanguage();
            Dimension current_size = getClientSize();
        }

        #region public methods
        public void applyLanguage()
        {
            lblTemplate.setText( _( "Template" ) );
            lblTemplate.setMnemonic( KeyEvent.VK_T, comboTemplate );
            groupPitchControl.setTitle( _( "Pitch Control" ) );
            lblBendDepth.setText( _( "Bend Depth" ) );
            lblBendDepth.setMnemonic( KeyEvent.VK_B, txtBendDepth );
            lblBendLength.setText( _( "Bend Length" ) );
            lblBendLength.setMnemonic( KeyEvent.VK_L, txtBendLength );
            chkUpPortamento.setText( _( "Add portamento in rising movement" ) );
            chkUpPortamento.setMnemonic( KeyEvent.VK_R );
            chkDownPortamento.setText( _( "Add portamento in falling movement" ) );
            chkDownPortamento.setMnemonic( KeyEvent.VK_F );

            groupDynamicsControl.setTitle( _( "Dynamics Control" ) );
            lblDecay.setText( _( "Decay" ) );
            lblDecay.setMnemonic( KeyEvent.VK_D, txtDecay );
            lblAccent.setText( _( "Accent" ) );
            lblAccent.setMnemonic( KeyEvent.VK_A, txtAccent );

            btnOK.setText( _( "OK" ) );
            btnCancel.setText( _( "Cancel" ) );
            btnApply.setText( _( "Apply to current track" ) );
            btnApply.setMnemonic( KeyEvent.VK_C );

#if !JAVA
            lblTemplate.Left = comboTemplate.Left - lblTemplate.Width;
#endif
            setTitle( _( "Default Singer Style" ) );
        }

        public int getPMBendDepth()
        {
            return trackBendDepth.getValue();
        }

        public void setPMBendDepth( int value )
        {
            trackBendDepth.setValue( value );
            txtBendDepth.setText( value + "" );
        }

        public int getPMBendLength()
        {
            return trackBendLength.getValue();
        }

        public void setPMBendLength( int value )
        {
            trackBendLength.setValue( value );
            txtBendLength.setText( value + "" );
        }

        public int getPMbPortamentoUse()
        {
            int ret = 0;
            if ( chkUpPortamento.isSelected() ) {
                ret += 1;
            }
            if ( chkDownPortamento.isSelected() ) {
                ret += 2;
            }
            return ret;
        }

        public void setPMbPortamentoUse( int value )
        {
            if ( value % 2 == 1 ) {
                chkUpPortamento.setSelected( true );
            } else {
                chkUpPortamento.setSelected( false );
            }
            if ( value >= 2 ) {
                chkDownPortamento.setSelected( true );
            } else {
                chkDownPortamento.setSelected( false );
            }
        }

        public int getDEMdecGainRate()
        {
            return trackDecay.getValue();
        }

        public void setDEMdecGainRate( int value )
        {
            trackDecay.setValue( value );
            txtDecay.setText( value + "" );
        }

        public int getDEMaccent()
        {
            return trackAccent.getValue();
        }

        public void setDEMaccent( int value )
        {
            trackAccent.setValue( value );
            txtAccent.setText( value + "" );
        }

        public boolean getApplyCurrentTrack()
        {
            return m_apply_current_track;
        }
        #endregion

        #region helper methods
        private static String _( String id )
        {
            return Messaging.getMessage( id );
        }

        private void registerEventHandlers()
        {
            txtBendLength.TextChanged += new BEventHandler( txtBendLength_TextChanged );
            txtBendDepth.TextChanged += new BEventHandler( txtBendDepth_TextChanged );
            trackBendLength.ValueChanged += new BEventHandler( trackBendLength_Scroll );
            trackBendDepth.ValueChanged += new BEventHandler( trackBendDepth_Scroll );
            txtAccent.TextChanged += new BEventHandler( txtAccent_TextChanged );
            txtDecay.TextChanged += new BEventHandler( txtDecay_TextChanged );
            trackAccent.ValueChanged += new BEventHandler( trackAccent_Scroll );
            trackDecay.ValueChanged += new BEventHandler( trackDecay_Scroll );
            btnOK.Click += new BEventHandler( btnOK_Click );
            btnApply.Click += new BEventHandler( btnApply_Click );
            comboTemplate.SelectedIndexChanged += new BEventHandler( comboBox1_SelectedIndexChanged );
            btnCancel.Click += new BEventHandler( btnCancel_Click );
        }

        private void setResources()
        {
        }
        #endregion

        #region event handlers
        public void trackBendDepth_Scroll( Object sender, EventArgs e )
        {
            String s = trackBendDepth.getValue() + "";
            if( !str.compare( s, txtBendDepth.getText() ) ){
                txtBendDepth.setText( s );
            }
        }

        public void txtBendDepth_TextChanged( Object sender, EventArgs e )
        {
            try {
                int draft = str.toi( txtBendDepth.getText() );
                if ( draft < trackBendDepth.getMinimum() ) {
                    draft = trackBendDepth.getMinimum();
                    txtBendDepth.setText( draft + "" );
                } else if ( trackBendDepth.getMaximum() < draft ) {
                    draft = trackBendDepth.getMaximum();
                    txtBendDepth.setText( draft + "" );
                }
                if ( draft != trackBendDepth.getValue() ) {
                    trackBendDepth.setValue( draft );
                }
            } catch ( Exception ex ) {
                //txtBendDepth.Text = trackBendDepth.Value + "";
            }
        }

        public void trackBendLength_Scroll( Object sender, EventArgs e )
        {
            String s = trackBendLength.getValue() + "";
            if( !str.compare( s, txtBendLength.getText() ) ){
                txtBendLength.setText( s );
            }
        }

        public void txtBendLength_TextChanged( Object sender, EventArgs e )
        {
            try {
                int draft = str.toi( txtBendLength.getText() );
                if ( draft < trackBendLength.getMinimum() ) {
                    draft = trackBendLength.getMinimum();
                    txtBendLength.setText( draft + "" );
                } else if ( trackBendLength.getMaximum() < draft ) {
                    draft = trackBendLength.getMaximum();
                    txtBendLength.setText( draft + "" );
                }
                if ( draft != trackBendLength.getValue() ) {
                    trackBendLength.setValue( draft );
                }
            } catch ( Exception ex ) {
                //txtBendLength.Text = trackBendLength.Value + "";
            }
        }

        public void trackDecay_Scroll( Object sender, EventArgs e )
        {
            String s = trackDecay.getValue() + "";
            if( !str.compare( s, txtDecay.getText() ) ){
                txtDecay.setText( s );
            }
        }

        public void txtDecay_TextChanged( Object sender, EventArgs e )
        {
            try {
                int draft = str.toi( txtDecay.getText() );
                if ( draft < trackDecay.getMinimum() ) {
                    draft = trackDecay.getMinimum();
                    txtDecay.setText( draft + "" );
                } else if ( trackDecay.getMaximum() < draft ) {
                    draft = trackDecay.getMaximum();
                    txtDecay.setText( draft + "" );
                }
                if ( draft != trackDecay.getValue() ) {
                    trackDecay.setValue( draft );
                }
            } catch ( Exception ex ) {
                //txtDecay.Text = trackDecay.Value + "";
            }
        }

        public void trackAccent_Scroll( Object sender, EventArgs e )
        {
            String s = trackAccent.getValue() + "";
            if( !str.compare( s, txtAccent.getText() ) ){
                txtAccent.setText( s );
            }
        }

        public void txtAccent_TextChanged( Object sender, EventArgs e )
        {
            try {
                int draft = str.toi( txtAccent.getText() );
                if ( draft < trackAccent.getMinimum() ) {
                    draft = trackAccent.getMinimum();
                    txtAccent.setText( draft + "" );
                } else if ( trackAccent.getMaximum() < draft ) {
                    draft = trackAccent.getMaximum();
                    txtAccent.setText( draft + "" );
                }
                if ( draft != trackAccent.getValue() ) {
                    trackAccent.setValue( draft );
                }
            } catch ( Exception ex ) {
                //txtAccent.Text = trackAccent.Value + "";
            }
        }

        public void btnOK_Click( Object sender, EventArgs e )
        {
            setDialogResult( BDialogResult.OK );
        }

        public void comboBox1_SelectedIndexChanged( Object sender, EventArgs e )
        {
            int index = comboTemplate.getSelectedIndex() - 1;
            if( index < 0 || 4 < index ){
                return;
            }
            int[] pm_bend_depth      = new int[]{  8,  8,  8, 20, 20 };
            int[] pm_bend_length     = new int[]{  0,  0,  0,  0, 0 };
            int[] pmb_portamento_use = new int[]{  0,  0,  0,  3, 3 };
            int[] dem_dec_gain_rate  = new int[]{ 50, 50, 70, 50, 50 };
            int[] dem_accent         = new int[]{ 50, 68, 80, 42, 25 };
            setPMBendDepth( pm_bend_depth[index] );
            setPMBendLength( pm_bend_length[index] );
            setPMbPortamentoUse( pmb_portamento_use[index] );
            setDEMdecGainRate( dem_dec_gain_rate[index] );
            setDEMaccent( dem_accent[index] );
        }

        public void btnApply_Click( Object sender, EventArgs e )
        {
            if ( AppManager.showMessageBox( _( "Would you like to change singer style for all events?" ),
                                  FormMain._APP_NAME,
                                  cadencii.windows.forms.Utility.MSGBOX_YES_NO_OPTION,
                                  cadencii.windows.forms.Utility.MSGBOX_WARNING_MESSAGE ) == BDialogResult.YES ) {
                m_apply_current_track = true;
                setDialogResult( BDialogResult.OK );
            }
        }

        public void btnCancel_Click( Object sender, EventArgs e )
        {
            setDialogResult( BDialogResult.CANCEL );
        }
        #endregion

        #region UI implementation
#if JAVA
        #region UI Impl for Java
        //INCLUDE-SECTION FIELD ./ui/java/FormSingerStyleConfig.java
        //INCLUDE-SECTION METHOD ./ui/java/FormSingerStyleConfig.java
        #endregion
#else
        #region UI Impl for C#
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

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupPitchControl = new BGroupBox();
            this.label5 = new BLabel();
            this.label4 = new BLabel();
            this.txtBendLength = new cadencii.NumberTextBox();
            this.txtBendDepth = new cadencii.NumberTextBox();
            this.trackBendLength = new BSlider();
            this.trackBendDepth = new BSlider();
            this.chkDownPortamento = new BCheckBox();
            this.chkUpPortamento = new BCheckBox();
            this.lblBendLength = new BLabel();
            this.lblBendDepth = new BLabel();
            this.groupDynamicsControl = new BGroupBox();
            this.label7 = new BLabel();
            this.label6 = new BLabel();
            this.txtAccent = new cadencii.NumberTextBox();
            this.txtDecay = new cadencii.NumberTextBox();
            this.trackAccent = new BSlider();
            this.trackDecay = new BSlider();
            this.lblAccent = new BLabel();
            this.lblDecay = new BLabel();
            this.lblTemplate = new BLabel();
            this.btnCancel = new BButton();
            this.btnOK = new BButton();
            this.btnApply = new BButton();
            this.comboTemplate = new BComboBox();
            this.groupPitchControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBendLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBendDepth)).BeginInit();
            this.groupDynamicsControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackAccent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackDecay)).BeginInit();
            this.SuspendLayout();
            // 
            // groupPitchControl
            // 
            this.groupPitchControl.Controls.Add( this.label5 );
            this.groupPitchControl.Controls.Add( this.label4 );
            this.groupPitchControl.Controls.Add( this.txtBendLength );
            this.groupPitchControl.Controls.Add( this.txtBendDepth );
            this.groupPitchControl.Controls.Add( this.trackBendLength );
            this.groupPitchControl.Controls.Add( this.trackBendDepth );
            this.groupPitchControl.Controls.Add( this.chkDownPortamento );
            this.groupPitchControl.Controls.Add( this.chkUpPortamento );
            this.groupPitchControl.Controls.Add( this.lblBendLength );
            this.groupPitchControl.Controls.Add( this.lblBendDepth );
            this.groupPitchControl.Location = new System.Drawing.Point( 8, 38 );
            this.groupPitchControl.Name = "groupPitchControl";
            this.groupPitchControl.Size = new System.Drawing.Size( 367, 130 );
            this.groupPitchControl.TabIndex = 0;
            this.groupPitchControl.TabStop = false;
            this.groupPitchControl.Text = "Pitch Control";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point( 345, 42 );
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size( 11, 12 );
            this.label5.TabIndex = 9;
            this.label5.Text = "%";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point( 345, 16 );
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size( 11, 12 );
            this.label4.TabIndex = 8;
            this.label4.Text = "%";
            // 
            // txtBendLength
            // 
            this.txtBendLength.BackColor = System.Drawing.SystemColors.Window;
            this.txtBendLength.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtBendLength.Location = new System.Drawing.Point( 300, 39 );
            this.txtBendLength.Name = "txtBendLength";
            this.txtBendLength.Size = new System.Drawing.Size( 39, 19 );
            this.txtBendLength.TabIndex = 5;
            this.txtBendLength.Text = "0";
            this.txtBendLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtBendLength.Type = cadencii.NumberTextBox.ValueType.Integer;
            // 
            // txtBendDepth
            // 
            this.txtBendDepth.BackColor = System.Drawing.SystemColors.Window;
            this.txtBendDepth.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtBendDepth.Location = new System.Drawing.Point( 300, 13 );
            this.txtBendDepth.Name = "txtBendDepth";
            this.txtBendDepth.Size = new System.Drawing.Size( 39, 19 );
            this.txtBendDepth.TabIndex = 2;
            this.txtBendDepth.Text = "8";
            this.txtBendDepth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtBendDepth.Type = cadencii.NumberTextBox.ValueType.Integer;
            // 
            // trackBendLength
            // 
            this.trackBendLength.AutoSize = false;
            this.trackBendLength.Location = new System.Drawing.Point( 138, 40 );
            this.trackBendLength.Maximum = 100;
            this.trackBendLength.Name = "trackBendLength";
            this.trackBendLength.Size = new System.Drawing.Size( 156, 18 );
            this.trackBendLength.TabIndex = 4;
            this.trackBendLength.TickFrequency = 10;
            // 
            // trackBendDepth
            // 
            this.trackBendDepth.AutoSize = false;
            this.trackBendDepth.Location = new System.Drawing.Point( 138, 14 );
            this.trackBendDepth.Maximum = 100;
            this.trackBendDepth.Name = "trackBendDepth";
            this.trackBendDepth.Size = new System.Drawing.Size( 156, 18 );
            this.trackBendDepth.TabIndex = 1;
            this.trackBendDepth.TickFrequency = 10;
            this.trackBendDepth.Value = 8;
            // 
            // chkDownPortamento
            // 
            this.chkDownPortamento.AutoSize = true;
            this.chkDownPortamento.Location = new System.Drawing.Point( 20, 96 );
            this.chkDownPortamento.Name = "chkDownPortamento";
            this.chkDownPortamento.Size = new System.Drawing.Size( 224, 16 );
            this.chkDownPortamento.TabIndex = 7;
            this.chkDownPortamento.Text = "Add portamento in falling movement(&F)";
            this.chkDownPortamento.UseVisualStyleBackColor = true;
            // 
            // chkUpPortamento
            // 
            this.chkUpPortamento.AutoSize = true;
            this.chkUpPortamento.Location = new System.Drawing.Point( 20, 71 );
            this.chkUpPortamento.Name = "chkUpPortamento";
            this.chkUpPortamento.Size = new System.Drawing.Size( 222, 16 );
            this.chkUpPortamento.TabIndex = 6;
            this.chkUpPortamento.Text = "Add portamento in rising movement(&R)";
            this.chkUpPortamento.UseVisualStyleBackColor = true;
            // 
            // lblBendLength
            // 
            this.lblBendLength.AutoSize = true;
            this.lblBendLength.Location = new System.Drawing.Point( 20, 46 );
            this.lblBendLength.Name = "lblBendLength";
            this.lblBendLength.Size = new System.Drawing.Size( 83, 12 );
            this.lblBendLength.TabIndex = 3;
            this.lblBendLength.Text = "Bend Length(&L)";
            // 
            // lblBendDepth
            // 
            this.lblBendDepth.AutoSize = true;
            this.lblBendDepth.Location = new System.Drawing.Point( 20, 20 );
            this.lblBendDepth.Name = "lblBendDepth";
            this.lblBendDepth.Size = new System.Drawing.Size( 81, 12 );
            this.lblBendDepth.TabIndex = 0;
            this.lblBendDepth.Text = "Bend Depth(&B)";
            // 
            // groupDynamicsControl
            // 
            this.groupDynamicsControl.Controls.Add( this.label7 );
            this.groupDynamicsControl.Controls.Add( this.label6 );
            this.groupDynamicsControl.Controls.Add( this.txtAccent );
            this.groupDynamicsControl.Controls.Add( this.txtDecay );
            this.groupDynamicsControl.Controls.Add( this.trackAccent );
            this.groupDynamicsControl.Controls.Add( this.trackDecay );
            this.groupDynamicsControl.Controls.Add( this.lblAccent );
            this.groupDynamicsControl.Controls.Add( this.lblDecay );
            this.groupDynamicsControl.Location = new System.Drawing.Point( 8, 174 );
            this.groupDynamicsControl.Name = "groupDynamicsControl";
            this.groupDynamicsControl.Size = new System.Drawing.Size( 367, 74 );
            this.groupDynamicsControl.TabIndex = 1;
            this.groupDynamicsControl.TabStop = false;
            this.groupDynamicsControl.Text = "Dynamics Control";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point( 345, 46 );
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size( 11, 12 );
            this.label7.TabIndex = 11;
            this.label7.Text = "%";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point( 345, 20 );
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size( 11, 12 );
            this.label6.TabIndex = 10;
            this.label6.Text = "%";
            // 
            // txtAccent
            // 
            this.txtAccent.BackColor = System.Drawing.SystemColors.Window;
            this.txtAccent.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtAccent.Location = new System.Drawing.Point( 300, 43 );
            this.txtAccent.Name = "txtAccent";
            this.txtAccent.Size = new System.Drawing.Size( 39, 19 );
            this.txtAccent.TabIndex = 13;
            this.txtAccent.Text = "50";
            this.txtAccent.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtAccent.Type = cadencii.NumberTextBox.ValueType.Integer;
            // 
            // txtDecay
            // 
            this.txtDecay.BackColor = System.Drawing.SystemColors.Window;
            this.txtDecay.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtDecay.Location = new System.Drawing.Point( 300, 17 );
            this.txtDecay.Name = "txtDecay";
            this.txtDecay.Size = new System.Drawing.Size( 39, 19 );
            this.txtDecay.TabIndex = 10;
            this.txtDecay.Text = "50";
            this.txtDecay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDecay.Type = cadencii.NumberTextBox.ValueType.Integer;
            // 
            // trackAccent
            // 
            this.trackAccent.AutoSize = false;
            this.trackAccent.Location = new System.Drawing.Point( 138, 44 );
            this.trackAccent.Maximum = 100;
            this.trackAccent.Name = "trackAccent";
            this.trackAccent.Size = new System.Drawing.Size( 156, 18 );
            this.trackAccent.TabIndex = 12;
            this.trackAccent.TickFrequency = 10;
            this.trackAccent.Value = 50;
            // 
            // trackDecay
            // 
            this.trackDecay.AutoSize = false;
            this.trackDecay.Location = new System.Drawing.Point( 138, 18 );
            this.trackDecay.Maximum = 100;
            this.trackDecay.Name = "trackDecay";
            this.trackDecay.Size = new System.Drawing.Size( 156, 18 );
            this.trackDecay.TabIndex = 9;
            this.trackDecay.TickFrequency = 10;
            this.trackDecay.Value = 50;
            // 
            // lblAccent
            // 
            this.lblAccent.AutoSize = true;
            this.lblAccent.Location = new System.Drawing.Point( 18, 50 );
            this.lblAccent.Name = "lblAccent";
            this.lblAccent.Size = new System.Drawing.Size( 57, 12 );
            this.lblAccent.TabIndex = 11;
            this.lblAccent.Text = "Accent(&A)";
            // 
            // lblDecay
            // 
            this.lblDecay.AutoSize = true;
            this.lblDecay.Location = new System.Drawing.Point( 18, 24 );
            this.lblDecay.Name = "lblDecay";
            this.lblDecay.Size = new System.Drawing.Size( 53, 12 );
            this.lblDecay.TabIndex = 8;
            this.lblDecay.Text = "Decay(&D)";
            // 
            // lblTemplate
            // 
            this.lblTemplate.AutoSize = true;
            this.lblTemplate.Location = new System.Drawing.Point( 163, 15 );
            this.lblTemplate.Name = "lblTemplate";
            this.lblTemplate.Size = new System.Drawing.Size( 67, 12 );
            this.lblTemplate.TabIndex = 2;
            this.lblTemplate.Text = "Template(&T)";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point( 290, 259 );
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size( 78, 23 );
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point( 203, 259 );
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size( 78, 23 );
            this.btnOK.TabIndex = 15;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point( 8, 259 );
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size( 153, 23 );
            this.btnApply.TabIndex = 14;
            this.btnApply.Text = "Apply to current track(&C)";
            this.btnApply.UseVisualStyleBackColor = true;
            // 
            // comboTemplate
            // 
            this.comboTemplate.FormattingEnabled = true;
            this.comboTemplate.Items.AddRange( new Object[] {
            "[Select a template]",
            "normal",
            "accent",
            "strong accent",
            "legato",
            "slow legato"} );
            this.comboTemplate.Location = new System.Drawing.Point( 253, 12 );
            this.comboTemplate.Name = "comboTemplate";
            this.comboTemplate.Size = new System.Drawing.Size( 121, 20 );
            this.comboTemplate.TabIndex = 0;
            this.comboTemplate.Text = "[Select a template]";
            // 
            // FormSingerStyleConfig
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 12F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size( 384, 308 );
            this.Controls.Add( this.btnCancel );
            this.Controls.Add( this.btnOK );
            this.Controls.Add( this.btnApply );
            this.Controls.Add( this.lblTemplate );
            this.Controls.Add( this.groupDynamicsControl );
            this.Controls.Add( this.groupPitchControl );
            this.Controls.Add( this.comboTemplate );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size( 6000, 6000 );
            this.MinimizeBox = false;
            this.Name = "FormSingerStyleConfig";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Default Singer Style";
            this.groupPitchControl.ResumeLayout( false );
            this.groupPitchControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBendLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBendDepth)).EndInit();
            this.groupDynamicsControl.ResumeLayout( false );
            this.groupDynamicsControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackAccent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackDecay)).EndInit();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private BGroupBox groupPitchControl;
        private BGroupBox groupDynamicsControl;
        private BLabel lblBendDepth;
        private BLabel lblTemplate;
        private BLabel lblBendLength;
        private BCheckBox chkDownPortamento;
        private BCheckBox chkUpPortamento;
        private BSlider trackBendDepth;
        private BSlider trackBendLength;
        private BSlider trackAccent;
        private BSlider trackDecay;
        private BLabel lblAccent;
        private BLabel lblDecay;
        private NumberTextBox txtBendLength;
        private NumberTextBox txtBendDepth;
        private NumberTextBox txtAccent;
        private NumberTextBox txtDecay;
        private BLabel label5;
        private BLabel label4;
        private BLabel label7;
        private BLabel label6;
        private BButton btnCancel;
        private BButton btnOK;
        private BButton btnApply;
        private BComboBox comboTemplate;
        #endregion
#endif
        #endregion

    }

#if !JAVA
}
#endif
