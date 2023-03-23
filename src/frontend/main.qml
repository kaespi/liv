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
            focus: true
            cache: false
            property int imgCnt: 1

            source: "image://imgprovider/" + imgCnt
            fillMode: Image.PreserveAspectFit

            onSourceChanged: {
                console.log("new image ready")
            }

            function updateImage(direction) {
                imgCnt++
                source = "image://imgprovider/" + imgCnt + "_" + direction
            }

            MouseArea {
                anchors.fill: parent
                onPressed: {
                    parent.source = "image://imgprovider/red"
                }
                onReleased: {
                    parent.source = "image://imgprovider/1"
                }
            }

            Keys.onLeftPressed: updateImage("prev")
            Keys.onRightPressed: updateImage("next")
        }
    }
}
