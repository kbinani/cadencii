package com.github.cadencii.windows.forms;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.Timer;
import com.github.cadencii.BEvent;
import com.github.cadencii.BEventArgs;
import com.github.cadencii.BEventHandler;

public class BTimer extends Timer implements ActionListener {
    private static final long serialVersionUID = 9174919033610117641L;
    public final BEvent<BEventHandler> tickEvent = new BEvent<BEventHandler>();
    
    public BTimer(){
        super( 100, null );
        addActionListener( this );
    }
    
    public void actionPerformed( ActionEvent e ){
        try{
            tickEvent.raise( this, new BEventArgs() );
        }catch( Exception ex ){
            System.err.println( "BTimer#actionPerformed; ex=" + ex );
        }
    }
}
