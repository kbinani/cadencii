package com.github.cadencii.ui;

import java.awt.Dimension;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JLabel;
import javax.swing.JPanel;

import com.github.cadencii.FormAskKeySoundGenerationUi;
import com.github.cadencii.FormAskKeySoundGenerationUiListener;

public class FormAskKeySoundGenerationUiImpl extends DialogBase implements FormAskKeySoundGenerationUi
{
    private static final long serialVersionUID = 1L;
    private JPanel jPanel = null;
    private JLabel lblMessage = null;
    private JPanel jPanel1 = null;
    private JButton btnYes = null;
    private JButton btnNo = null;
    private JCheckBox chkAlwaysPerformThisCheck = null;
    private FormAskKeySoundGenerationUiListener mListener;

    public FormAskKeySoundGenerationUiImpl(
        FormAskKeySoundGenerationUiListener listener )
    {
        super();
        mListener = listener;
        initialize();
    }

    /**
     * This method initializes this
     *
     */
    private void initialize()
    {
        this.setSize( new Dimension( 376, 190 ) );
        this.setContentPane( getJPanel() );
        setCancelButton( btnNo );
    }

    public void close(
        boolean value )
    {
        if( value ){
            super.setDialogResult( false );
        }else{
            super.setDialogResult( true );
        }
    }

    /**
     * This method initializes jPanel
     *
     * @return javax.swing.JPanel
     */
    private JPanel getJPanel()
    {
        if( jPanel == null ){
            GridBagConstraints gridBagConstraints4 = new GridBagConstraints();
            gridBagConstraints4.gridx = 0;
            gridBagConstraints4.weightx = 1.0D;
            gridBagConstraints4.fill = GridBagConstraints.HORIZONTAL;
            gridBagConstraints4.ipadx = 0;
            gridBagConstraints4.ipady = 16;
            gridBagConstraints4.insets = new Insets( 0, 0, 16, 0 );
            gridBagConstraints4.gridy = 2;
            GridBagConstraints gridBagConstraints1 = new GridBagConstraints();
            gridBagConstraints1.gridx = 0;
            gridBagConstraints1.ipady = 16;
            gridBagConstraints1.anchor = GridBagConstraints.WEST;
            gridBagConstraints1.insets = new Insets( 0, 16, 0, 0 );
            gridBagConstraints1.gridy = 1;
            GridBagConstraints gridBagConstraints = new GridBagConstraints();
            gridBagConstraints.gridx = 0;
            gridBagConstraints.weightx = 1.0D;
            gridBagConstraints.weighty = 1.0D;
            gridBagConstraints.fill = GridBagConstraints.BOTH;
            gridBagConstraints.ipadx = 0;
            gridBagConstraints.insets = new Insets( 16, 16, 8, 16 );
            gridBagConstraints.gridy = 0;
            lblMessage = createJLabel();
            jPanel = new JPanel();
            jPanel.setLayout( new GridBagLayout() );
            jPanel.add( lblMessage, gridBagConstraints );
            jPanel.add( getJCheckBox(), gridBagConstraints1 );
            jPanel.add( getJPanel1(), gridBagConstraints4 );
        }
        return jPanel;
    }

    /**
     * This method initializes jPanel1
     *
     * @return javax.swing.JPanel
     */
    private JPanel getJPanel1()
    {
        if( jPanel1 == null ){
            GridBagConstraints gridBagConstraints3 = new GridBagConstraints();
            gridBagConstraints3.gridx = 0;
            gridBagConstraints3.weightx = 1.0D;
            gridBagConstraints3.gridy = 0;
            GridBagConstraints gridBagConstraints2 = new GridBagConstraints();
            gridBagConstraints2.gridx = 1;
            gridBagConstraints2.weightx = 1.0D;
            gridBagConstraints2.gridy = 0;
            jPanel1 = new JPanel();
            jPanel1.setLayout( new GridBagLayout() );
            jPanel1.add( getJButton(), gridBagConstraints2 );
            jPanel1.add( getJButton1(), gridBagConstraints3 );
        }
        return jPanel1;
    }

    /**
     * This method initializes jButton
     *
     * @return javax.swing.JButton
     */
    private JButton getJButton()
    {
        if( btnYes == null ){
            btnYes = new JButton();
            btnYes.setText( "Yes" );
            btnYes.setPreferredSize( new Dimension( 100, 29 ) );
            btnYes.addActionListener( new java.awt.event.ActionListener()
            {
                public void actionPerformed(
                    java.awt.event.ActionEvent e )
                {
                    mListener.buttonOkClickedSlot();
                }
            } );
        }
        return btnYes;
    }

    /**
     * This method initializes jButton1
     *
     * @return javax.swing.JButton
     */
    private JButton getJButton1()
    {
        if( btnNo == null ){
            btnNo = new JButton();
            btnNo.setText( "No" );
            btnNo.setPreferredSize( new Dimension( 100, 29 ) );
            btnNo.addActionListener( new java.awt.event.ActionListener()
            {
                public void actionPerformed(
                    java.awt.event.ActionEvent e )
                {
                    mListener.buttonCancelClickedSlot();
                }
            } );
        }
        return btnNo;
    }

    /**
     * This method initializes jCheckBox
     *
     * @return javax.swing.JCheckBox
     */
    private JCheckBox getJCheckBox()
    {
        if( chkAlwaysPerformThisCheck == null ){
            chkAlwaysPerformThisCheck = new JCheckBox();
            chkAlwaysPerformThisCheck.setText( "Always perform this check when starting Cadencii." );
        }
        return chkAlwaysPerformThisCheck;
    }

    public int showDialog( Object parent_form )
    {
        return super.doShowDialog( parent_form );
    }

    public void setAlwaysPerformThisCheck( boolean value )
    {
        chkAlwaysPerformThisCheck.setSelected( value );
    }

    public boolean isAlwaysPerformThisCheck()
    {
        return chkAlwaysPerformThisCheck.isSelected();
    }

    // SECTION-END-METHOD

    public void setMessageLabelText( String value )
    {
        lblMessage.setText( value );
    }

    public void setAlwaysPerformThisCheckCheckboxText( String value )
    {
        chkAlwaysPerformThisCheck.setText( value );
    }

    public void setYesButtonText( String value )
    {
        btnYes.setText( value );
    }

    public void setNoButtonText( String value )
    {
        btnNo.setText( value );
    }

    /**
     * @wbp.factory
     */
    public static JLabel createJLabel() {
        JLabel label = DialogBase.AutoElipsisLabel.createAutoEllipsisLabel();
        label.setText( "It seems some key-board sounds are missing.\nDo you want to re-generate them now?" );
        return label;
    }
} // @jve:decl-index=0:visual-constraint="10,10"
