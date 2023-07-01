import './App.css';
import useDefaults from './useDefaults';
import useHubConnection from './useHubConnection';
import FSRWebUI from './components/FSRWebUI'
import LoadingScreen from './components/LoadingScreen';

function App() {
  const { defaults, reloadDefaults } = useDefaults();
  const {
    emit,
    isWsReady,
    webUIDataRef,
    wsRef
  } = useHubConnection({ defaults, onCloseWs: reloadDefaults });

  if (defaults && isWsReady) {
    return (
      <FSRWebUI
        defaults={defaults}
        emit={emit}
        webUIDataRef={webUIDataRef}
        wsRef={wsRef}
      />
    );
  } else {
    return <LoadingScreen />
  }
}

export default App
