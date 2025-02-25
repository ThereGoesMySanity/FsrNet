import { useEffect, useState } from "react";

function useLocalProfiles(activeProfile) {
  const [localProfiles, setLocalProfiles] = useState(null);
  const [localProfile, setLocalProfile] = useState(null);
  const [data, setData] = useState(null);
  useEffect(() => {
    fetch('/api/local-profiles')
      .then(res => res.json())
      .then(profiles => {
        setLocalProfiles(profiles);
        if (activeProfile in profiles) {
          setLocalProfile(profiles[activeProfile]);
        }
      })
  }, [activeProfile]);
  useEffect(() => {
    if (localProfiles && localProfile) {
      fetch('/api/local-profiles/' + localProfile)
        .then(res => res.json())
        .catch(err => setData(null))
        .then(data => {
          if (data === undefined) {
            setData(null);
            return;
          }
          const ordered = Object.keys(data).sort().reduce(
            (obj, key) => {
              obj[key] = data[key];
              return obj;
            }, {});
          setData(ordered);
        });
    }
  }, [localProfile]);
  if (localProfiles === null) return null;

  return {localProfiles, localProfile, setLocalProfile, data};
}
export default useLocalProfiles