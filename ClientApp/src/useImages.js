import { useEffect, useState } from "react";

function useImages(defaults, wsRef) {
  const [images, setImages] = useState(defaults.images);
  const [curImage, setCurImage] = useState(defaults.data.image);
  useEffect(() => {
    wsRef.current.on("images", setImages);
    wsRef.current.on("image", setCurImage);

    return () => {
      wsRef.current.off("images");
      wsRef.current.off("image");
    }
  }, [images, wsRef]);

  if (images === null) return null;

  return {images, curImage};
}

export default useImages