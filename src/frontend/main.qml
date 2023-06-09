import QtQuick
import QtQuick.Layouts

import "qrc:/js/ImageDisplayUtils.js" as ImageDisplayUtils

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

            onStatusChanged: {
                if (status === Image.Ready) {
                    loadingImage = false
                    if (sourceSize.width === 1 && sourceSize.height === 1) {
                        noImageText.visible = true
                        visible = false
                    } else {
                        noImageText.visible = false
                        ImageDisplayUtils.adjustImageElementSizeToPicture(
                                    sourceSize, this, parent)
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
