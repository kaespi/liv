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
            property bool loadingImage: false

            source: "image://imgprovider/" + imgCnt
            fillMode: Image.PreserveAspectFit

            Timer {
                id: imgLoadingTimer
            }

            function delay(delayTime, cb) {
                imgLoadingTimer.interval = delayTime
                imgLoadingTimer.repeat = false
                imgLoadingTimer.triggered.connect(waitImageLoaded)
                imgLoadingTimer.start()
            }

            onSourceChanged: {
                loadingImage = false
            }

            function waitImageLoaded() {
                if (loadingImage) {
                    delay(10, waitImageLoaded)
                }
            }

            function updateImage(direction) {
                imgCnt++
                waitImageLoaded()
                loadingImage = true
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
