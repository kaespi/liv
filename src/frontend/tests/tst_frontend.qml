import QtQuick
import QtTest

import "qrc:/js/ImageDisplayUtils.js" as ImageDisplayUtils

TestCase {
    name: "ImageDisplayUtils"

    function test_GIVEN_image_smaller_than_window_WHEN_image_element_size_is_adjusted_THEN_image_element_is_scaled_to_image_size() {

        let imageSize = {
            "height": 30,
            "width": 40
        }

        let imageElement = {
            "anchors": {
                "fill": "parent",
                "centerIn": undefined
            },
            "height": 600,
            "width": 500
        }

        let parent = {
            "height": 600,
            "width": 500
        }

        ImageDisplayUtils.adjustImageElementSizeToPicture(imageSize,
                                                          imageElement, parent)

        verify(imageElement.height === imageSize.height,
               "Image element is expected to be scaled to the image's size")
        verify(imageElement.anchors.fill === undefined,
               "Image element is expected to not be anchored")
        verify(imageElement.anchors.centerIn === parent,
               "Image element is expected to be centered in its parent")
    }
}
