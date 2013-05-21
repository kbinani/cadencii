package com.github.cadencii.windows.forms;

import com.github.cadencii.BEventHandler;

public class BFormClosedEventHandler extends BEventHandler{
    public BFormClosedEventHandler( Object invoker, String method_name ){
        super( invoker, method_name, Void.TYPE, Object.class, BFormClosedEventArgs.class );
    }
    
    public BFormClosedEventHandler( Class<?> invoker, String method_name ){
        super( invoker, method_name, Void.TYPE, Object.class, BFormClosedEventArgs.class );
    }
}
