import { useState, useEffect, useRef } from 'react'
import ListGroup from 'react-bootstrap/ListGroup'
import { Upload } from 'react-bootstrap-icons'

import ImageSelectItem from "./ImageSelectItem";

function ImageSelect({emit, defaults, wsRef}) {
  const [images, setImages] = useState(defaults.images);
  const [curImage, setCurImage] = useState(defaults.data.image);
  const fileInputRef = useRef(null);

  function updateImage(img) {
    emit('updateImage', img);
  }
  function removeImage(img) {
    emit('removeImage', img);
  }
  function handleChange() {
    let data = new FormData();
    data.append('img', fileInputRef.current.files[0]);
    fetch('/api/images/upload', {
      method: 'POST',
      body: data
    });
  }

  useEffect(() => {
    wsRef.current.on("images", setImages);
    wsRef.current.on("image", setCurImage);

    return () => {
      wsRef.current.off("images");
      wsRef.current.off("image");
    }
  }, [images, wsRef]);

  const callbacks = {updateImage, removeImage};

  return (
    <header className="App-header">
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