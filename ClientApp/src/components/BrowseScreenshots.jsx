import { useState, useEffect } from "react";
import { ListGroup } from 'react-bootstrap';

import "./BrowseScreenshots.css"

function BrowseScreenshots({useLocalProfiles}) {
  if (!useLocalProfiles) return false;

  const {localProfiles, localProfile, setLocalProfile, data} = useLocalProfiles;
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
          <ListGroup.Item key={p} action onClick={() => setLocalProfile(localProfiles[p])} active={p in localProfiles && localProfiles[p] === localProfile}>
            {p}
          </ListGroup.Item>
        ))}
      </ListGroup>
      <div className="ss-outer">
      {data && Object.keys(data).sort().reverse().map(path => 
        <div key={path} className="ss-category">
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