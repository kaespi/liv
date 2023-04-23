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

        Text {
            id: noImageText
            text: qsTr("no image")
            color: "white"
            visible: false
            anchors.fill: parent
            horizontalAlignment: Text.AlignHCenter
            verticalAlignment: Text.AlignVCenter
        }

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

            function adjustImageElementSizeToPicture() {
                let heightRatio = sourceSize.height / parent.height
                let widthRatio = sourceSize.width / parent.width
                if ((heightRatio < 1.0) || (widthRatio < 1.0)) {
                    if (heightRatio < widthRatio) {
                        anchors.fill = undefined
                        anchors.centerIn = parent
                        height = sourceSize.height
                    } else {
                        anchors.fill = undefined
                        anchors.centerIn = parent
                        width = sourceSize.width
                    }
                } else {
                    anchors.fill = parent
                }
            }

            onStatusChanged: {
                if (status === Image.Ready) {
                    loadingImage = false
                    if (sourceSize.width === 1 && sourceSize.height === 1) {
                        noImageText.visible = true
                        visible = false
                    } else {
                        noImageText.visible = false
                        adjustImageElementSizeToPicture()
                        visible = true
                    }
                }
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

            Keys.onLeftPressed: updateImage("prev")
            Keys.onRightPressed: updateImage("next")
        }
    }
}
