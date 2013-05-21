package com.github.cadencii.windows.forms;

import java.awt.event.ComponentEvent;
import java.awt.event.ComponentListener;
import javax.swing.JPopupMenu;
import javax.swing.event.PopupMenuEvent;
import javax.swing.event.PopupMenuListener;
import com.github.cadencii.BEvent;
import com.github.cadencii.BEventArgs;
import com.github.cadencii.BEventHandler;
import com.github.cadencii.componentmodel.BCancelEventArgs;
import com.github.cadencii.componentmodel.BCancelEventHandler;

public class BPopupMenu extends JPopupMenu 
                        implements ComponentListener,
                                   PopupMenuListener
{
    private static final long serialVersionUID = 363411779635481115L;

    public BPopupMenu()
    {
        super();
        addComponentListener( this );
        addPopupMenuListener( this );
    }

    /* root impl of PopupMenuListener */
    // root impl of PopupMenuListener is in BPopupMenu
    public final BEvent<BCancelEventHandler> openingEvent = new BEvent<BCancelEventHandler>();
    public void popupMenuCanceled( PopupMenuEvent e ){
    }
    public void popupMenuWillBecomeInvisible( PopupMenuEvent e ){
    }
    public void popupMenuWillBecomeVisible( PopupMenuEvent e ){
        try{
            BCancelEventArgs e1 = new BCancelEventArgs();
            openingEvent.raise( this, e1 );
            if( e1.Cancel ){
                setVisible( false );
            }
        }catch( Exception ex ){
            System.err.println( "BPopupMenu#popupMenuWillBecomeVisible; ex=" + ex );
        }
    }

    // root impl of ComponentListener is in BButton
    public final BEvent<BEventHandler> visibleChangedEvent = new BEvent<BEventHandler>();
    public final BEvent<BEventHandler> resizeEvent = new BEvent<BEventHandler>();
    public void componentHidden(ComponentEvent e) {
        try{
            visibleChangedEvent.raise( this, new BEventArgs() );
        }catch( Exception ex ){
            System.err.println( "BButton#componentHidden; ex=" + ex );
        }
    }
    public void componentMoved(ComponentEvent e) {
    }
    public void componentResized(ComponentEvent e) {
        try{
            resizeEvent.raise( this, new BEventArgs() );
        }catch( Exception ex ){
            System.err.println( "BButton#componentResized; ex=" + ex );
        }
    }
    public void componentShown(ComponentEvent e) {
        try{
            visibleChangedEvent.raise( this, new BEventArgs() );
        }catch( Exception ex ){
            System.err.println( "BButton#componentShown; ex=" + ex );
        }
    }
}
