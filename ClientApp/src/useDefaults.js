import { useEffect, useState, useCallback } from "react";

function useDefaults() {
  const [defaults, setDefaults] = useState(undefined);

  const reloadDefaults = useCallback(() => setDefaults(undefined), [setDefaults]);

  // Load defaults at mount and reload any time they are cleared.
  useEffect(() => {
    let cleaningUp = false;
    let timeoutId = 0;

    const getDefaults = () => {
      clearTimeout(timeoutId);
      fetch('/api/profiles').then(res => res.json()).then(data => {
        if (!cleaningUp) {
          setDefaults(data);
        }
      }).catch(_ => {
        if (!cleaningUp) {
          timeoutId = setTimeout(getDefaults, 1000);
        }
      });
    }

    if (!defaults) {
      getDefaults();
    }

    return () => {
      cleaningUp = true;
      clearTimeout(timeoutId);
    };
  }, [defaults]);

  return { defaults, reloadDefaults };
}

export default useDefaults