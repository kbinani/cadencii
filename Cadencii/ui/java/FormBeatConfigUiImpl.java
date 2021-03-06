package com.github.cadencii.ui;

import java.awt.Dimension;
import java.awt.Font;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JComboBox;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JSpinner;
import com.github.cadencii.FormBeatConfigUi;
import com.github.cadencii.FormBeatConfigUiListener;

public class FormBeatConfigUiImpl extends DialogBase implements FormBeatConfigUi
{
    private static final long serialVersionUID = 4414859292940722020L;
    private FormBeatConfigUiListener listener;
    private JPanel jContentPane = null;
    private JPanel groupPosition = null;
    private JLabel lblStart = null;
    private JSpinner numStart = null;
    private JLabel lblBar1 = null;
    private JCheckBox chkEnd = null;
    private JSpinner numEnd = null;
    private JLabel lblBar2 = null;
    private JPanel groupBeat = null;
    private JSpinner numNumerator = null;
    private JLabel jLabel = null;
    private JLabel jLabel1 = null;
    private JComboBox comboDenominator = null;
    private JPanel jPanel1 = null;
    private JLabel jLabel2 = null;
    private JButton btnOK = null;
    private JButton btnCancel = null;
    private JLabel jLabel4 = null;
    private JLabel jLabel7 = null;
    private JLabel jLabel8 = null;

    public FormBeatConfigUiImpl( FormBeatConfigUiListener l ){
        super();
        listener = l;
        initialize();
    }

    /**
     * This method initializes this
     *
     * @return void
     */
    private void initialize() {
        this.setTitle("Beat Change");
        this.setSize(314, 287);
        this.setContentPane(getJContentPane());
        this.setTitle("JFrame");
        setCancelButton( btnCancel );
    }

    /**
     * This method initializes jContentPane
     *
     * @return javax.swing.JPanel
     */
    private JPanel getJContentPane() {
        if (jContentPane == null) {
            GridBagConstraints gridBagConstraints18 = new GridBagConstraints();
            gridBagConstraints18.insets = new Insets(0, 0, 1, 12);
            gridBagConstraints18.gridy = 2;
            gridBagConstraints18.ipadx = 180;
            gridBagConstraints18.ipady = 42;
            gridBagConstraints18.weightx = 1.0D;
            gridBagConstraints18.weighty = 0.0D;
            gridBagConstraints18.anchor = GridBagConstraints.NORTH;
            gridBagConstraints18.fill = GridBagConstraints.BOTH;
            gridBagConstraints18.gridx = 0;
            GridBagConstraints gridBagConstraints8 = new GridBagConstraints();
            gridBagConstraints8.gridx = 0;
            gridBagConstraints8.ipadx = 141;
            gridBagConstraints8.ipady = 42;
            gridBagConstraints8.fill = GridBagConstraints.BOTH;
            gridBagConstraints8.insets = new Insets(0, 12, 0, 12);
            gridBagConstraints8.weightx = 1.0D;
            gridBagConstraints8.anchor = GridBagConstraints.NORTH;
            gridBagConstraints8.weighty = 0.5D;
            gridBagConstraints8.gridy = 1;
            GridBagConstraints gridBagConstraints6 = new GridBagConstraints();
            gridBagConstraints6.gridx = 0;
            gridBagConstraints6.ipadx = 147;
            gridBagConstraints6.ipady = 41;
            gridBagConstraints6.insets = new Insets(12, 12, 12, 12);
            gridBagConstraints6.fill = GridBagConstraints.BOTH;
            gridBagConstraints6.weightx = 1.0D;
            gridBagConstraints6.anchor = GridBagConstraints.NORTH;
            gridBagConstraints6.weighty = 0.5D;
            gridBagConstraints6.gridy = 0;
            jContentPane = new JPanel();
            jContentPane.setLayout(new GridBagLayout());
            jContentPane.add(getGroupPosition(), gridBagConstraints6);
            jContentPane.add(getGroupBeat(), gridBagConstraints8);
            jContentPane.add(getBPanel1(), gridBagConstraints18);
        }
        return jContentPane;
    }

    /**
     * This method initializes groupPosition
     *
     * @return javax.swing.BPanel
     */
    private JPanel getGroupPosition() {
        if (groupPosition == null) {
            GridBagConstraints gridBagConstraints61 = new GridBagConstraints();
            gridBagConstraints61.gridx = 3;
            gridBagConstraints61.gridy = 2;
            jLabel8 = new JLabel();
            jLabel8.setText("     ");
            GridBagConstraints gridBagConstraints51 = new GridBagConstraints();
            gridBagConstraints51.gridx = 3;
            gridBagConstraints51.gridy = 0;
            jLabel7 = new JLabel();
            jLabel7.setText("     ");
            GridBagConstraints gridBagConstraints9 = new GridBagConstraints();
            gridBagConstraints9.fill = GridBagConstraints.VERTICAL;
            gridBagConstraints9.gridy = -1;
            gridBagConstraints9.weightx = 1.0;
            gridBagConstraints9.gridx = -1;
            GridBagConstraints gridBagConstraints5 = new GridBagConstraints();
            gridBagConstraints5.gridx = 2;
            gridBagConstraints5.anchor = GridBagConstraints.WEST;
            gridBagConstraints5.insets = new Insets(0, 9, 0, 0);
            gridBagConstraints5.gridy = 2;
            lblBar2 = new JLabel();
            lblBar2.setText("Beat");
            GridBagConstraints gridBagConstraints4 = new GridBagConstraints();
            gridBagConstraints4.fill = GridBagConstraints.HORIZONTAL;
            gridBagConstraints4.gridy = 2;
            gridBagConstraints4.weightx = 1.0;
            gridBagConstraints4.insets = new Insets(3, 0, 3, 0);
            gridBagConstraints4.gridx = 1;
            GridBagConstraints gridBagConstraints3 = new GridBagConstraints();
            gridBagConstraints3.gridx = 0;
            gridBagConstraints3.insets = new Insets(0, 16, 0, 0);
            gridBagConstraints3.gridy = 2;
            GridBagConstraints gridBagConstraints2 = new GridBagConstraints();
            gridBagConstraints2.gridx = 2;
            gridBagConstraints2.anchor = GridBagConstraints.WEST;
            gridBagConstraints2.insets = new Insets(0, 9, 0, 0);
            gridBagConstraints2.gridy = 0;
            lblBar1 = new JLabel();
            lblBar1.setText("Measure");
            GridBagConstraints gridBagConstraints1 = new GridBagConstraints();
            gridBagConstraints1.fill = GridBagConstraints.HORIZONTAL;
            gridBagConstraints1.gridy = 0;
            gridBagConstraints1.weightx = 0.0D;
            gridBagConstraints1.insets = new Insets(3, 0, 3, 0);
            gridBagConstraints1.gridx = 1;
            GridBagConstraints gridBagConstraints = new GridBagConstraints();
            gridBagConstraints.gridx = 0;
            gridBagConstraints.insets = new Insets(0, 16, 0, 0);
            gridBagConstraints.gridy = 0;
            lblStart = new JLabel();
            lblStart.setText("From");
            groupPosition = createGroupPosition();
            groupPosition.setLayout(new GridBagLayout());
            groupPosition.add(lblStart, gridBagConstraints);
            groupPosition.add(getNumStart(), gridBagConstraints1);
            groupPosition.add(lblBar1, gridBagConstraints2);
            groupPosition.add(getChkEnd(), gridBagConstraints3);
            groupPosition.add(getNumEnd(), gridBagConstraints4);
            groupPosition.add(lblBar2, gridBagConstraints5);
            groupPosition.add(jLabel7, gridBagConstraints51);
            groupPosition.add(jLabel8, gridBagConstraints61);
        }
        return groupPosition;
    }

    /**
     * This method initializes numStart
     *
     * @return javax.swing.BComboBox
     */
    private JSpinner getNumStart() {
        if (numStart == null) {
            numStart = createNumStart();
            numStart.setPreferredSize(new Dimension(31, 29));
        }
        return numStart;
    }

    /**
     * This method initializes chkEnd
     *
     * @return javax.swing.JCheckBox
     */
    private JCheckBox getChkEnd() {
        if (chkEnd == null) {
            chkEnd = new JCheckBox();
            chkEnd.setText("To");
            chkEnd.addItemListener( new java.awt.event.ItemListener()
            {
                public void itemStateChanged( java.awt.event.ItemEvent e )
                {
                    if( listener != null ){
                        listener.checkboxEndCheckedChangedSlot();
                    }
                }
            } );
        }
        return chkEnd;
    }

    /**
     * This method initializes numEnd
     *
     * @return javax.swing.BComboBox
     */
    private JSpinner getNumEnd() {
        if (numEnd == null) {
            numEnd = createNumEnd();
            numEnd.setPreferredSize(new Dimension(31, 29));
            numEnd.setEnabled(false);
        }
        return numEnd;
    }

    /**
     * This method initializes groupBeat
     *
     * @return javax.swing.BPanel
     */
    private JPanel getGroupBeat() {
        if (groupBeat == null) {
            GridBagConstraints gridBagConstraints13 = new GridBagConstraints();
            gridBagConstraints13.gridx = 4;
            gridBagConstraints13.gridy = 0;
            jLabel2 = new JLabel();
            jLabel2.setText("     ");
            GridBagConstraints gridBagConstraints12 = new GridBagConstraints();
            gridBagConstraints12.fill = GridBagConstraints.HORIZONTAL;
            gridBagConstraints12.gridy = 0;
            gridBagConstraints12.weightx = 0.5D;
            gridBagConstraints12.insets = new Insets(3, 0, 3, 0);
            gridBagConstraints12.gridx = 3;
            GridBagConstraints gridBagConstraints11 = new GridBagConstraints();
            gridBagConstraints11.gridx = 2;
            gridBagConstraints11.gridy = 0;
            jLabel1 = new JLabel();
            jLabel1.setText(" /    ");
            GridBagConstraints gridBagConstraints10 = new GridBagConstraints();
            gridBagConstraints10.gridx = 1;
            gridBagConstraints10.gridy = 0;
            jLabel = new JLabel();
            jLabel.setText(" (1-255) ");
            GridBagConstraints gridBagConstraints7 = new GridBagConstraints();
            gridBagConstraints7.fill = GridBagConstraints.HORIZONTAL;
            gridBagConstraints7.gridy = 0;
            gridBagConstraints7.weightx = 0.5D;
            gridBagConstraints7.insets = new Insets(3, 16, 3, 0);
            gridBagConstraints7.gridx = 0;
            groupBeat = createGroupBeat();
            groupBeat.setLayout(new GridBagLayout());
            groupBeat.add(getNumNumerator(), gridBagConstraints7);
            groupBeat.add(jLabel, gridBagConstraints10);
            groupBeat.add(jLabel1, gridBagConstraints11);
            groupBeat.add(getComboDenominator(), gridBagConstraints12);
            groupBeat.add(jLabel2, gridBagConstraints13);
        }
        return groupBeat;
    }

    /**
     * This method initializes numNumerator
     *
     * @return javax.swing.BComboBox
     */
    private JSpinner getNumNumerator() {
        if (numNumerator == null) {
            numNumerator = creatNumNumerator();
            numNumerator.setPreferredSize(new Dimension(31, 29));
        }
        return numNumerator;
    }

    /**
     * This method initializes comboDenominator
     *
     * @return javax.swing.JComboBox
     */
    private JComboBox getComboDenominator() {
        if (comboDenominator == null) {
            comboDenominator = new JComboBox();
            comboDenominator.setPreferredSize(new Dimension(31, 27));
        }
        return comboDenominator;
    }

    /**
     * This method initializes jPanel1
     *
     * @return javax.swing.JPanel
     */
    private JPanel getBPanel1() {
        if (jPanel1 == null) {
            GridBagConstraints gridBagConstraints17 = new GridBagConstraints();
            gridBagConstraints17.gridx = 0;
            gridBagConstraints17.fill = GridBagConstraints.BOTH;
            gridBagConstraints17.weightx = 1.0D;
            gridBagConstraints17.gridy = 0;
            jLabel4 = new JLabel();
            jLabel4.setText(" ");
            GridBagConstraints gridBagConstraints16 = new GridBagConstraints();
            gridBagConstraints16.gridx = 1;
            gridBagConstraints16.anchor = GridBagConstraints.EAST;
            gridBagConstraints16.insets = new Insets(0, 0, 0, 0);
            gridBagConstraints16.gridy = 0;
            GridBagConstraints gridBagConstraints15 = new GridBagConstraints();
            gridBagConstraints15.gridx = 2;
            gridBagConstraints15.anchor = GridBagConstraints.EAST;
            gridBagConstraints15.insets = new Insets(0, 0, 0, 0);
            gridBagConstraints15.gridy = 0;
            jPanel1 = new JPanel();
            jPanel1.setLayout(new GridBagLayout());
            jPanel1.add(getBtnOk(), gridBagConstraints15);
            jPanel1.add(getBtnCancel(), gridBagConstraints16);
            jPanel1.add(jLabel4, gridBagConstraints17);
        }
        return jPanel1;
    }

    /**
     * This method initializes btnOk
     *
     * @return javax.swing.JButton
     */
    private JButton getBtnOk() {
        if (btnOK == null) {
            btnOK = new JButton();
            btnOK.setText("OK");
            btnOK.setPreferredSize(new Dimension(100, 29));
            btnOK.addActionListener( new java.awt.event.ActionListener()
            {
                public void actionPerformed( java.awt.event.ActionEvent e )
                {
                    if( listener != null ){
                        listener.buttonOkClickedSlot();
                    }
                }
            } );
        }
        return btnOK;
    }

    /**
     * This method initializes btnCancel
     *
     * @return javax.swing.JButton
     */
    private JButton getBtnCancel() {
        if (btnCancel == null) {
            btnCancel = new JButton();
            btnCancel.setText("Cancel");
            btnCancel.addActionListener( new java.awt.event.ActionListener()
            {
                public void actionPerformed( java.awt.event.ActionEvent e )
                {
                    if( listener != null ){
                        listener.buttonCancelClickedSlot();
                    }
                }
            } );
            btnCancel.setPreferredSize(new Dimension(100, 29));
        }
        return btnCancel;
    }

    public int showDialog( Object parent_form )
    {
        return super.doShowDialog( parent_form );
    }

    public void setFont( String fontName, float fontSize )
    {
        super.setFont( new Font( fontName, Font.PLAIN, (int)fontSize ) );
    }

    @Override
    public void close()
    {
        super.doClose();
    }

    public void setTextBar1Label( String value )
    {
        this.lblBar1.setText( value );
    }

    public void setTextBar2Label( String value )
    {
        this.lblBar2.setText( value );
    }

    public void setTextStartLabel( String value )
    {
        this.lblStart.setText( value );
    }

    public void setTextOkButton( String value )
    {
        this.btnOK.setText( value );
    }

    public void setTextCancelButton( String value )
    {
        this.btnCancel.setText( value );
    }

    public void setTextBeatGroup( String value )
    {
        DialogBase.GroupBox.setTitle( this.groupBeat, value );
    }

    public void setTextPositionGroup( String value )
    {
        DialogBase.GroupBox.setTitle( this.groupPosition, value );
    }

    public void setEnabledStartNum( boolean value )
    {
        this.numStart.setEnabled( value );
    }

    public void setMinimumStartNum( int value )
    {
        DialogBase.Spinner.setMinimum( numStart, value );
    }

    public void setMaximumStartNum( int value )
    {
        DialogBase.Spinner.setMaximum( numStart, value );
    }

    public int getMaximumStartNum()
    {
        return (int)DialogBase.Spinner.getMaximum( numStart );
    }

    public int getMinimumStartNum()
    {
        return (int)DialogBase.Spinner.getMinimum( numStart );
    }

    public void setValueStartNum( int value )
    {
        DialogBase.Spinner.setFloatValue( numStart, value );
    }

    public int getValueStartNum()
    {
        return (int)DialogBase.Spinner.getFloatValue( numStart );
    }

    public void setEnabledEndNum( boolean value )
    {
        this.numEnd.setEnabled( value );
    }

    public void setMinimumEndNum( int value )
    {
        DialogBase.Spinner.setMinimum( this.numEnd, value );
    }

    public void setMaximumEndNum( int value )
    {
        DialogBase.Spinner.setMaximum( this.numEnd, value );
    }

    public int getMaximumEndNum()
    {
        return (int)DialogBase.Spinner.getMaximum( this.numEnd );
    }

    public int getMinimumEndNum()
    {
        return (int)DialogBase.Spinner.getMinimum( this.numEnd );
    }

    public void setValueEndNum( int value )
    {
        DialogBase.Spinner.setFloatValue( this.numEnd, value );
    }

    public int getValueEndNum()
    {
        return (int)DialogBase.Spinner.getFloatValue( this.numEnd );
    }

    public boolean isCheckedEndCheckbox()
    {
        return this.chkEnd.isSelected();
    }

    public void setEnabledEndCheckbox( boolean value )
    {
        this.chkEnd.setEnabled( value );
    }

    public boolean isEnabledEndCheckbox()
    {
        return this.chkEnd.isEnabled();
    }

    public void setTextEndCheckbox( String value )
    {
        this.chkEnd.setText( value );
    }

    public void removeAllItemsDenominatorCombobox()
    {
        this.comboDenominator.removeAllItems();
    }

    public void addItemDenominatorCombobox( String value )
    {
        this.comboDenominator.addItem( value );
    }

    public void setSelectedIndexDenominatorCombobox( int value )
    {
        this.comboDenominator.setSelectedIndex( value );
    }

    public int getSelectedIndexDenominatorCombobox()
    {
        return this.comboDenominator.getSelectedIndex();
    }

    public int getMaximumNumeratorNum()
    {
        return (int)DialogBase.Spinner.getMaximum( this.numNumerator );
    }

    public int getMinimumNumeratorNum()
    {
        return (int)DialogBase.Spinner.getMinimum( this.numNumerator );
    }

    public void setValueNumeratorNum( int value )
    {
        DialogBase.Spinner.setFloatValue( this.numNumerator, value );
    }

    public int getValueNumeratorNum()
    {
        return (int)DialogBase.Spinner.getFloatValue( this.numNumerator );
    }

    public void setDialogResult( boolean dialogResult )
    {
        super.setDialogResult( dialogResult );
    }

    /**
     * @wbp.factory
     */
    public static JSpinner createNumStart() {
        JSpinner spinner = DialogBase.Spinner.create();
        return spinner;
    }

    /**
     * @wbp.factory
     */
    public static JSpinner createNumEnd() {
        JSpinner spinner = DialogBase.Spinner.create();
        return spinner;
    }

    /**
     * @wbp.factory
     */
    public static JSpinner creatNumNumerator() {
        JSpinner spinner = DialogBase.Spinner.create();
        return spinner;
    }

    /**
     * @wbp.factory
     */
    public static JPanel createGroupPosition() {
        JPanel panel = DialogBase.GroupBox.create();
        DialogBase.GroupBox.setTitle( panel, "Position" );
        return panel;
    }

    /**
     * @wbp.factory
     * @return
     */
    public static JPanel createGroupBeat()
    {
        JPanel panel = DialogBase.GroupBox.create();
        DialogBase.GroupBox.setTitle( panel, "Position" );
        return panel;
    }
}
