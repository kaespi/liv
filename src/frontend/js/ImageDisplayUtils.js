function adjustImageElementSizeToPicture(imageSize, imageElement, parent) {
    let heightRatio = imageSize.height / parent.height
    let widthRatio = imageSize.width / parent.width
    if ((heightRatio < 1.0) || (widthRatio < 1.0)) {
        if (heightRatio < widthRatio) {
            imageElement.anchors.fill = undefined
            imageElement.anchors.centerIn = parent
            imageElement.height = imageSize.height
        } else {
            imageElement.anchors.fill = undefined
            imageElement.anchors.centerIn = parent
            imageElement.width = imageSize.width
        }
    } else {
        imageElement.anchors.fill = parent
    }
}
