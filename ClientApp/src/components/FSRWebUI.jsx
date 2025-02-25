import BrowseScreenshots from "./BrowseScreenshots";
import ImageSelect from "./ImageSelect";
import ValueMonitor from "./ValueMonitor";
import ValueMonitors from "./ValueMonitors";
import Plot from "./Plot";

import Navbar from 'react-bootstrap/Navbar'
import Nav from 'react-bootstrap/Nav'
import NavDropdown from 'react-bootstrap/NavDropdown'

import Form from 'react-bootstrap/Form'
import Button from 'react-bootstrap/Button'

import {
  BrowserRouter as Router,
  Routes,
  Route,
  Link
} from "react-router-dom";

import { useState, useEffect } from "react";
import useImages from "../useImages";
import useLocalProfiles from "../useLocalProfiles";

function FSRWebUI(props) {
  const { emit, defaults, webUIDataRef, wsRef} = props;
  const numSensors = defaults.data.thresholds.length;
  const [profiles, setProfiles] = useState(defaults.profiles);
  const [activeProfile, setActiveProfile] = useState(defaults.currentProfile);
  const images = useImages(defaults, wsRef);
  const localProfiles = useLocalProfiles(activeProfile);

  useEffect(() => {
    const ws = wsRef.current;

    ws.on("get_profiles", setProfiles);
    
    ws.on("get_cur_profile", setActiveProfile);

    return () => {
      ws.off("get_profiles");
      ws.off("get_cur_profile");
    };
  }, [profiles, wsRef]);

  function AddProfile(e) {
    // Only add a profile on the enter key.
    if (e.keyCode === 13) {
      emit('addProfile', e.target.value, {"thresholds": webUIDataRef.current.curThresholds, "image": webUIDataRef.current.curImage});
      // Reset the text box.
      e.target.value = "";
    }
    return false;
  }

  function RemoveProfile(e) {
    // The X button is inside the Change Profile button, so stop the event from bubbling up and triggering the ChangeProfile handler.
    e.stopPropagation();
    // Strip out the "X " added by the button.
    let profile_name = e.target.parentNode.innerText;
    if (profile_name === 'X') profile_name = '';
    profile_name = profile_name.replace('X ', '');
    emit('removeProfile', profile_name);
  }

  function ChangeProfile(e) {
    // Strip out the "X " added by the button.
    let profile_name = e.target.innerText;
    if (profile_name === 'X') profile_name = '';
    profile_name = profile_name.replace('X ', '');
    emit('changeProfile', profile_name);
  }
  
  return (
    <div className="App">
      <Router>
        <Navbar bg="light">
          <Navbar.Brand as={Link} to="/">FSR WebUI</Navbar.Brand>
          <Nav>
            <Nav.Item>
              <Nav.Link as={Link} to="/plot">Plot</Nav.Link>
            </Nav.Item>
            {images !== null &&
            <Nav.Item>
              <Nav.Link as={Link} to="/image-select">GIFs</Nav.Link>
            </Nav.Item>
            }
            {localProfiles !== null &&
            <Nav.Item>
              <Nav.Link as={Link} to="/browse-screenshots">Screenshots</Nav.Link>
            </Nav.Item>
            }
          </Nav>
          <Nav className="ms-auto">
            <NavDropdown align="end" title="Profile" id="collasible-nav-dropdown">
              {profiles.map(function(profile) {
                if (profile === activeProfile) {
                  return(
                    <NavDropdown.Item key={profile} style={{paddingLeft: "0.5rem"}}
                        onClick={ChangeProfile} active>
                      <Button variant="light" onClick={RemoveProfile}>X</Button>{' '}{profile}
                    </NavDropdown.Item>
                  );
                } else {
                  return(
                    <NavDropdown.Item key={profile} style={{paddingLeft: "0.5rem"}}
                        onClick={ChangeProfile}>
                      <Button variant="light" onClick={RemoveProfile}>X</Button>{' '}{profile}
                    </NavDropdown.Item>
                  );
                }
              })}
              <NavDropdown.Divider />
              <Form inline="true" onSubmit={(e) => e.preventDefault()}>
                <Form.Control
                    onKeyDown={AddProfile}
                    style={{marginLeft: "0.5rem", marginRight: "0.5rem", maxWidth: "calc(100% - 1rem)"}}
                    type="text"
                    placeholder="New Profile" />
              </Form>
            </NavDropdown>
          </Nav>
        </Navbar>
        <Routes>
          <Route exact path="/" element={
            <ValueMonitors numSensors={numSensors}>
              {[...Array(numSensors).keys()].map(index => (
                <ValueMonitor emit={emit} index={index} key={index} webUIDataRef={webUIDataRef} />)
              )}
            </ValueMonitors>
          }/>
          <Route path="/plot" element={
            <Plot numSensors={numSensors} webUIDataRef={webUIDataRef} />
          }/>
          <Route path="/image-select" element={
            <ImageSelect emit={emit} useImages={images}/>
          }/>
          <Route path="/browse-screenshots" element={
            <BrowseScreenshots useLocalProfiles={localProfiles} />
          }/>
        </Routes>
      </Router>
    </div>
  );
}

export default FSRWebUI