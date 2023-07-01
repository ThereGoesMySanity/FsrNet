import Container from 'react-bootstrap/Container'
import Row from 'react-bootstrap/Row'

function ValueMonitors(props) {
  const { numSensors } = props;
  return (
    <header className="App-header">
      <Container fluid style={{border: '1px solid white', height: '100vh'}}>
        <Row className="ValueMonitor-row">
          {props.children}
        </Row>
      </Container>
      <style>
        {`
        .ValueMonitor-col {
          width: ${100 / numSensors}%;
        }
        /* 15 + 15 is left and right padding (from bootstrap col class). */
        /* 75 is the minimum desired width of the canvas. */
        /* If there is not enough room for all columns and padding to fit, reduce padding. */
        @media (max-width: ${numSensors * (15 + 15 + 75)}px) {
          .ValueMonitor-col {
            padding-left: 1px;
            padding-right: 1px;
          }
        }
        `}
      </style>
    </header>
  );
}

export default ValueMonitors