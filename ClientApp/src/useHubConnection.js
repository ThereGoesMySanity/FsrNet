import { useState, useEffect, useRef, useCallback } from 'react'
import { HubConnectionBuilder } from '@microsoft/signalr'
function useHubConnection({defaults, onCloseWs}) {
  const [isWsReady, setIsWsReady] = useState(false);
  // Some values such as sensor readings are stored in a mutable array in a ref so that
  // they are not subject to the React render cycle, for performance reasons.
  const webUIDataRef = useRef({

    // A history of the past 'MAX_SIZE' values fetched from the backend.
    // Used for plotting and displaying live values.
    // We use a cyclical array to save memory.
    curValues: [],
    oldest: 0,

    // Keep track of the current thresholds fetched from the backend.
    curThresholds: [],
    curImage: null,
  });

  const wsRef = useRef();

  const emit = useCallback((endpoint, ...args) => {
    // App should wait for isWsReady to send messages.
    if (!wsRef.current || !isWsReady) {
      throw new Error("emit() called when isWsReady !== true.");
    }

    wsRef.current.invoke(endpoint, ...args);
  }, [isWsReady, wsRef]);

  useEffect(() => {
    let cleaningUp = false;
    const webUIData = webUIDataRef.current;

    if (!defaults) {
      // If defaults are loading or reloading, don't connect.
      return;
    }

    // Ensure values history reset and default thresholds are set.
    webUIData.curValues.length = 0;
    webUIData.curValues.push(new Array(defaults.data.thresholds.length).fill(0));
    webUIData.oldest = 0;
    webUIData.curImage = defaults.data.image;
    webUIDataRef.current.curThresholds.length = 0;
    webUIDataRef.current.curThresholds.push(...defaults.data.thresholds);

    const newConnection = new HubConnectionBuilder()
        .withUrl("http://" + window.location.host + "/profilehub")
        .withAutomaticReconnect()
        .build();
    wsRef.current = newConnection;

    newConnection.onclose(function(ev) {
      if (!cleaningUp) {
        onCloseWs();
      }
    });

    newConnection.start()
      .then(_ => {
        wsRef.current.on("values", function(values) {
          const webUIData = webUIDataRef.current;
          if (webUIData.curValues.length < MAX_SIZE) {
            webUIData.curValues.push(values);
          } else {
            webUIData.curValues[webUIData.oldest] = values;
            webUIData.oldest = (webUIData.oldest + 1) % MAX_SIZE;
          }
        });
      
        wsRef.current.on("thresholds", function(thresholds) {
          // Modify thresholds array in place instead of replacing it so that animation loops can have a stable reference.
          webUIDataRef.current.curThresholds.length = 0;
          webUIDataRef.current.curThresholds.push(...thresholds);
        });
      
        setIsWsReady(true);
      });

    return () => {
      cleaningUp = true;
      setIsWsReady(false);
      wsRef.current.stop();
    };
  }, [defaults, onCloseWs]);

  return { emit, isWsReady, webUIDataRef, wsRef };
}

export default useHubConnection