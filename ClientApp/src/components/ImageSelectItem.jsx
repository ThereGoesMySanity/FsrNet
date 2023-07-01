import ListGroup from 'react-bootstrap/ListGroup'
import CloseButton from 'react-bootstrap/esm/CloseButton';

export default function ImageSelectItem({image, callbacks, active}) {
    const isDefault = ["default.gif", "alternate.gif"].includes(image);
    return (
        <ListGroup.Item key={image} active={active} action={!active} onClick={() => callbacks.updateImage(image)}>
            <img src={"images/previews/"+image} alt=""/>
            {isDefault || <CloseButton onClick={() => callbacks.removeImage(image)} style={{position: "absolute", top: 0, right: 0}}/>}
        </ListGroup.Item>
    );
}