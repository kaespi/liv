import QtQuick
import QtQuick.Layouts

Window {
    width: 640
    height: 480
    visible: true
    title: qsTr("liv")

    Rectangle {
        anchors.fill: parent
        color: "black"

        Image {
            id: img1
            anchors.fill: parent

            source: "image://imgprovider/1"
            fillMode: Image.PreserveAspectFit

            MouseArea {
                anchors.fill: parent
                onPressed: {
                    parent.source = "image://imgprovider/red"
                }
                onReleased: {
                    parent.source = "image://imgprovider/1"
                }
            }
        }
    }
}
