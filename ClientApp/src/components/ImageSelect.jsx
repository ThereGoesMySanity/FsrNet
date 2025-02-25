import { useState, useEffect, useRef } from 'react'
import ListGroup from 'react-bootstrap/ListGroup'
import { Upload } from 'react-bootstrap-icons'

import ImageSelectItem from "./ImageSelectItem";

function ImageSelect({emit, useImages}) {
  const fileInputRef = useRef(null);
  function handleChange() {
    let data = new FormData();
    data.append('img', fileInputRef.current.files[0]);
    fetch('/api/images/upload', {
      method: 'POST',
      body: data
    });
  }
  if (useImages === null) return false;
  let {images, curImage} = useImages;

  function updateImage(img) {
    emit('updateImage', img);
  }
  function removeImage(img) {
    emit('removeImage', img);
  }
  const callbacks = {updateImage, removeImage};


  return (
    <header className="App-header" style={{overflow: "auto"}}>
      <ListGroup style={{width: "60%"}}>
        {curImage && <ImageSelectItem key={curImage} image={curImage} active callbacks={callbacks}/>}
        {images && images.filter(i => i !== curImage)
          .map(i => <ImageSelectItem key={i} image={i} callbacks={callbacks}/>)}
        <ListGroup.Item key="/upload" onClick={() => fileInputRef.current.click()} action>
          <Upload size="192px"/>
        </ListGroup.Item>
      </ListGroup>
      <input onChange={handleChange} ref={fileInputRef} type='file' accept="image/*" style={{display: "none"}}/>
    </header>
  );
}

export default ImageSelect