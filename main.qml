import QtQuick
import QtQuick.Layouts

Window {
    width: 640
    height: 480
    visible: true
    title: qsTr("liv")
    ColumnLayout {
        anchors.fill: parent

        Image {
            id: img1

            source: "image://imgprovider/1"
            Layout.fillHeight: true
            Layout.fillWidth: true

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
        Image {
            id: img2

            source: "image://imgprovider/2"
            Layout.fillHeight: true
            Layout.fillWidth: true

            MouseArea {
                anchors.fill: parent
                onPressed: {
                    parent.source = "image://imgprovider/yellow"
                }
                onReleased: {
                    parent.source = "image://imgprovider/2"
                }
            }
        }
    }
}
