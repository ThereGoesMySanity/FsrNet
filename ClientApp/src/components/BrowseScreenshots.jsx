import { useState, useEffect } from "react";
import { ListGroup } from 'react-bootstrap';

import "./BrowseScreenshots.css"

function BrowseScreenshots(props) {
  const { activeProfile } = props;
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
          if (data === undefined) setData(null);
          const ordered = Object.keys(data).sort().reduce(
            (obj, key) => {
              obj[key] = data[key];
              return obj;
            }, {});
          setData(ordered);
        });
    }
  }, [localProfile]);
  function downloadSS(link, file) {
    const element = document.createElement("a");
    element.href = link;
    element.download = file;
    
    document.body.appendChild(element);
    element.click();
  }
  return (
    <header className="App-header" style={{flexDirection: "row"}}>
      <ListGroup className="profile-list">
        {localProfiles && Object.keys(localProfiles).map(p => (
          <ListGroup.Item action onClick={() => setLocalProfile(localProfiles[p])} active={p in localProfiles && localProfiles[p] === localProfile}>
            {p}
          </ListGroup.Item>
        ))}
      </ListGroup>
      <div className="ss-outer">
      {data && Object.keys(data).sort().reverse().slice(0, 5).map(path => 
        <div className="ss-category">
          <h1>{path}</h1>
          <div className="ss-cat-inner">
          {data[path].map(i => 
            <div className="ss-list-item" onClick={() => downloadSS(`/api/screenshots/${localProfile}/${path}/${i}`, i)}>
              <img src={`/api/screenshots/${localProfile}/${path}/${i}`} alt=""/>
            </div>
          )}
          </div>
        </div>
      )}
      </div>
    </header>
  );
}

export default BrowseScreenshots